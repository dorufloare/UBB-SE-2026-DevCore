using System;
using System.Linq;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using DevCoreHospital.Tests.Repositories;
using Microsoft.Data.SqlClient;
using Xunit;

namespace DevCoreHospital.Tests.Integration
{
    public class ShiftManagementServiceIntegrationTests : IClassFixture<SqlTestFixture>
    {
        private readonly SqlTestFixture db;

        public ShiftManagementServiceIntegrationTests(SqlTestFixture db) => this.db = db;

        [Fact]
        public void AddShift_WhenShiftIsProvided_AddsShiftToCacheAndDatabase()
        {
            using var conn = db.OpenConnection();
            var staffId = db.InsertStaff(conn, "Doctor", "Add", "ShiftTest", "Cardiology");
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);
                var staff = staffRepo.GetStaffById(staffId)!;
                var start = DateTime.Today.AddDays(30).AddHours(8);
                var shift = new Shift(0, staff, "ER", start, start.AddHours(4), ShiftStatus.SCHEDULED);
                var initialCount = shiftRepo.GetShifts().Count;

                service.AddShift(shift);

                Assert.Equal(initialCount + 1, shiftRepo.GetShifts().Count);
                Assert.Contains(shiftRepo.GetShifts(), s => s.AppointedStaff.StaffID == staffId);
            }
            finally
            {
                DeleteShiftsByStaff(conn, staffId);
                db.DeleteStaff(conn, staffId);
            }
        }

        [Fact]
        public void ValidateNoOverlap_WhenShiftOverlapsExistingShift_ReturnsFalse()
        {
            using var conn = db.OpenConnection();
            var staffId = db.InsertStaff(conn, "Doctor", "Overlap", "DoctorTest", "Neurology");
            var start = DateTime.Today.AddDays(31).AddHours(8);
            var shiftId = db.InsertShift(conn, staffId, "ER", start, start.AddHours(4));
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);

                var result = service.ValidateNoOverlap(staffId, start.AddHours(2), start.AddHours(6));

                Assert.False(result);
            }
            finally
            {
                db.DeleteShift(conn, shiftId);
                db.DeleteStaff(conn, staffId);
            }
        }

        [Fact]
        public void ValidateNoOverlap_WhenShiftDoesNotOverlapExistingShift_ReturnsTrue()
        {
            using var conn = db.OpenConnection();
            var staffId = db.InsertStaff(conn, "Doctor", "NoOverlap", "DoctorTest", "Oncology");
            var start = DateTime.Today.AddDays(32).AddHours(8);
            var shiftId = db.InsertShift(conn, staffId, "ER", start, start.AddHours(4));
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);

                var result = service.ValidateNoOverlap(staffId, start.AddHours(4), start.AddHours(8));

                Assert.True(result);
            }
            finally
            {
                db.DeleteShift(conn, shiftId);
                db.DeleteStaff(conn, staffId);
            }
        }

        [Fact]
        public void SetShiftActive_WhenShiftExists_UpdatesStatusToActiveInRepositoryAndDatabase()
        {
            using var conn = db.OpenConnection();
            var staffId = db.InsertStaff(conn, "Doctor", "SetActive", "DoctorTest", "Cardiology");
            var start = DateTime.Today.AddDays(33).AddHours(9);
            var shiftId = db.InsertShift(conn, staffId, "ER", start, start.AddHours(8));
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);

                service.SetShiftActive(shiftId);

                var cachedShift = Assert.Single(shiftRepo.GetShifts().Where(s => s.Id == shiftId));
                Assert.Equal(ShiftStatus.ACTIVE, cachedShift.Status);
                Assert.Equal("ACTIVE", db.GetShiftStatus(conn, shiftId));
            }
            finally
            {
                db.DeleteShift(conn, shiftId);
                db.DeleteStaff(conn, staffId);
            }
        }

        [Fact]
        public void CancelShift_WhenShiftExists_UpdatesStatusToCompletedInRepositoryAndDatabase()
        {
            using var conn = db.OpenConnection();
            var staffId = db.InsertStaff(conn, "Doctor", "Cancel", "DoctorTest", "Emergency Medicine");
            var start = DateTime.Today.AddDays(34).AddHours(7);
            var shiftId = db.InsertShift(conn, staffId, "ER", start, start.AddHours(8));
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);

                service.CancelShift(shiftId);

                var cachedShift = Assert.Single(shiftRepo.GetShifts().Where(s => s.Id == shiftId));
                Assert.Equal(ShiftStatus.COMPLETED, cachedShift.Status);
                Assert.Equal("COMPLETED", db.GetShiftStatus(conn, shiftId));
            }
            finally
            {
                db.DeleteShift(conn, shiftId);
                db.DeleteStaff(conn, staffId);
            }
        }

        [Fact]
        public void ReassignShift_WhenInputsAreValid_ChangesAppointedStaffAndReturnsTrue()
        {
            using var conn = db.OpenConnection();
            var originalStaffId = db.InsertStaff(conn, "Doctor", "Original", "ReassignTest", "Cardiology");
            var replacementStaffId = db.InsertStaff(conn, "Doctor", "Replacement", "ReassignTest", "Cardiology");
            var start = DateTime.Today.AddDays(35).AddHours(8);
            var shiftId = db.InsertShift(conn, originalStaffId, "ER", start, start.AddHours(4));
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);
                var shift = shiftRepo.GetShiftById(shiftId)!;
                var replacement = staffRepo.GetStaffById(replacementStaffId)!;

                var result = service.ReassignShift(shift, replacement);

                Assert.True(result);
                Assert.Equal(replacementStaffId, shift.AppointedStaff.StaffID);
            }
            finally
            {
                db.DeleteShift(conn, shiftId);
                db.DeleteStaff(conn, originalStaffId);
                db.DeleteStaff(conn, replacementStaffId);
            }
        }

        private static void DeleteShiftsByStaff(SqlConnection conn, int staffId)
        {
            using var cmd = new SqlCommand("DELETE FROM Shifts WHERE staff_id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", staffId);
            cmd.ExecuteNonQuery();
        }
    }
}
