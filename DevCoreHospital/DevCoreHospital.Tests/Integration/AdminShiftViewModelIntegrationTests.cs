using System;
using System.Linq;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using DevCoreHospital.Tests.Repositories;
using DevCoreHospital.ViewModels.Admin;
using Microsoft.Data.SqlClient;
using Xunit;

namespace DevCoreHospital.Tests.Integration
{
    public class AdminShiftViewModelIntegrationTests : IClassFixture<SqlTestFixture>
    {
        private readonly SqlTestFixture db;

        public AdminShiftViewModelIntegrationTests(SqlTestFixture db) => this.db = db;

        [Fact]
        public void CreateNewShift_WhenNoOverlap_AddsShiftToRepositoryAndViewModel()
        {
            using var conn = db.OpenConnection();
            var staffId = db.InsertStaff(conn, "Doctor", "Create", "ShiftVmTest", "Cardiology");
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);
                var viewModel = new AdminShiftViewModel(service);
                var staff = staffRepo.GetStaffById(staffId)!;
                var start = DateTime.Today.AddHours(8);
                var end = DateTime.Today.AddHours(12);
                var initialCount = shiftRepo.GetShifts().Count;

                viewModel.CreateNewShift(staff, start, end, "ER");

                Assert.Equal(initialCount + 1, shiftRepo.GetShifts().Count);
                var staffShift = Assert.Single(shiftRepo.GetShifts().Where(s => s.AppointedStaff.StaffID == staffId));
                Assert.Equal("ER", staffShift.Location);
                Assert.Equal(ShiftStatus.SCHEDULED, staffShift.Status);
                Assert.Contains(viewModel.Shifts, s => s.AppointedStaff.StaffID == staffId && s.Location == "ER");
            }
            finally
            {
                DeleteShiftsByStaff(conn, staffId);
                db.DeleteStaff(conn, staffId);
            }
        }

        [Fact]
        public void SetShiftActive_WhenShiftExists_UpdatesStatusInRepositoryAndViewModel()
        {
            using var conn = db.OpenConnection();
            var staffId = db.InsertStaff(conn, "Doctor", "SetActiveVm", "ShiftVmTest", "Neurology");
            var start = DateTime.Today.AddHours(8);
            var shiftId = db.InsertShift(conn, staffId, "ER", start, start.AddHours(8), "SCHEDULED");
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);
                var viewModel = new AdminShiftViewModel(service);

                viewModel.SetShiftActive(shiftId);

                var repoShift = Assert.Single(shiftRepo.GetShifts().Where(s => s.Id == shiftId));
                Assert.Equal(ShiftStatus.ACTIVE, repoShift.Status);
                var vmShift = Assert.Single(viewModel.Shifts.Where(s => s.Id == shiftId));
                Assert.Equal(ShiftStatus.ACTIVE, vmShift.Status);
            }
            finally
            {
                db.DeleteShift(conn, shiftId);
                db.DeleteStaff(conn, staffId);
            }
        }

        [Fact]
        public void CancelShift_WhenShiftExists_UpdatesStatusInRepositoryAndViewModel()
        {
            using var conn = db.OpenConnection();
            var staffId = db.InsertStaff(conn, "Doctor", "CancelVm", "ShiftVmTest", "Oncology");
            var start = DateTime.Today.AddHours(9);
            var shiftId = db.InsertShift(conn, staffId, "ER", start, start.AddHours(8), "SCHEDULED");
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);
                var viewModel = new AdminShiftViewModel(service);

                viewModel.CancelShift(shiftId);

                var repoShift = Assert.Single(shiftRepo.GetShifts().Where(s => s.Id == shiftId));
                Assert.Equal(ShiftStatus.COMPLETED, repoShift.Status);
                var vmShift = Assert.Single(viewModel.Shifts.Where(s => s.Id == shiftId));
                Assert.Equal(ShiftStatus.COMPLETED, vmShift.Status);
            }
            finally
            {
                db.DeleteShift(conn, shiftId);
                db.DeleteStaff(conn, staffId);
            }
        }

        [Fact]
        public void SelectedDepartment_WhenSet_FiltersShiftsInViewModel()
        {
            using var conn = db.OpenConnection();
            var staffId = db.InsertStaff(conn, "Doctor", "FilterVm", "ShiftVmTest", "Cardiology");
            // Use tomorrow so the shifts fall within the current week regardless of today's day-of-week
            var tomorrow = DateTime.Today.AddDays(1);
            var erShiftId = db.InsertShift(conn, staffId, "ER", tomorrow.AddHours(8), tomorrow.AddHours(10));
            var pharmacyShiftId = db.InsertShift(conn, staffId, "Pharmacy", tomorrow.AddHours(11), tomorrow.AddHours(13));
            try
            {
                var staffRepo = new StaffRepository(db.ConnectionString);
                var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
                var service = new ShiftManagementService(staffRepo, shiftRepo);
                var viewModel = new AdminShiftViewModel(service);

                viewModel.IsWeeklyView = true;
                viewModel.SelectedDepartment = "ER";

                Assert.Contains(viewModel.Shifts, s => s.Id == erShiftId && s.Location == "ER");
                Assert.DoesNotContain(viewModel.Shifts, s => s.Id == pharmacyShiftId);
            }
            finally
            {
                db.DeleteShift(conn, erShiftId);
                db.DeleteShift(conn, pharmacyShiftId);
                db.DeleteStaff(conn, staffId);
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
