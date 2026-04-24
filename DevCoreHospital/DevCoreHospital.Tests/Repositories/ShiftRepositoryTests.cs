using System;
using System.Linq;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using Microsoft.Data.SqlClient;

namespace DevCoreHospital.Tests.Repositories;

public class ShiftRepositoryTests : IClassFixture<SqlTestFixture>
{
    private readonly SqlTestFixture db;
    private const string InvalidConnectionString = "InvalidConnectionString";

    public ShiftRepositoryTests(SqlTestFixture db) => this.db = db;

    // ── Graceful degradation ─────────────────────────────────────────────────

    [Fact]
    public void GetShifts_WhenConnectionFails_ReturnsEmptyList()
    {
        var shiftRepo = new ShiftRepository(InvalidConnectionString, new StaffRepository(InvalidConnectionString));

        Assert.Empty(shiftRepo.GetShifts());
    }

    [Fact]
    public void GetShiftById_WhenConnectionFails_ReturnsNull()
    {
        var shiftRepo = new ShiftRepository(InvalidConnectionString, new StaffRepository(InvalidConnectionString));

        Assert.Null(shiftRepo.GetShiftById(1));
    }

    [Fact]
    public void GetShiftsByStaffID_WhenConnectionFails_ReturnsEmptyList()
    {
        var shiftRepo = new ShiftRepository(InvalidConnectionString, new StaffRepository(InvalidConnectionString));

        Assert.Empty(shiftRepo.GetShiftsByStaffID(1));
    }

    [Fact]
    public void AddShift_WhenConnectionFails_DoesNotThrow()
    {
        var shiftRepo = new ShiftRepository(InvalidConnectionString, new StaffRepository(InvalidConnectionString));
        var doctor = new Doctor(1, "John", "Doe", string.Empty, string.Empty, true, "Cardiology", "L-1", DoctorStatus.AVAILABLE, 1);
        var shift = new Shift(0, doctor, "ER", DateTime.Today.AddHours(8), DateTime.Today.AddHours(12), ShiftStatus.SCHEDULED);

        var ex = Record.Exception(() => shiftRepo.AddShift(shift));

        Assert.Null(ex);
    }

    [Fact]
    public void UpdateShiftStatus_WhenConnectionFails_DoesNotThrow()
    {
        var shiftRepo = new ShiftRepository(InvalidConnectionString, new StaffRepository(InvalidConnectionString));

        var ex = Record.Exception(() => shiftRepo.UpdateShiftStatus(1, ShiftStatus.ACTIVE));

        Assert.Null(ex);
    }

    [Fact]
    public void CancelShift_WhenConnectionFails_DoesNotThrow()
    {
        var shiftRepo = new ShiftRepository(InvalidConnectionString, new StaffRepository(InvalidConnectionString));

        var ex = Record.Exception(() => shiftRepo.CancelShift(1));

        Assert.Null(ex);
    }

    // ── Real database tests ──────────────────────────────────────────────────

    [Fact]
    public void GetShifts_ReturnsShiftFromDatabase()
    {
        using var conn = db.OpenConnection();
        var staffId = db.InsertStaff(conn, "Doctor", "Alice", "ShiftLoad", "Cardiology");
        var start = DateTime.Today.AddDays(1).AddHours(8);
        var shiftId = db.InsertShift(conn, staffId, "Ward A", start, start.AddHours(8));
        try
        {
            var shiftRepo = new ShiftRepository(db.ConnectionString, new StaffRepository(db.ConnectionString));

            Assert.Contains(shiftRepo.GetShifts(), s => s.Id == shiftId);
        }
        finally
        {
            db.DeleteShift(conn, shiftId);
            db.DeleteStaff(conn, staffId);
        }
    }

    [Fact]
    public void AddShift_PersistsShiftToDatabase()
    {
        using var conn = db.OpenConnection();
        var staffId = db.InsertStaff(conn, "Doctor", "AddShift", "DbTest", "Neurology");
        try
        {
            var staffRepo = new StaffRepository(db.ConnectionString);
            var shiftRepo = new ShiftRepository(db.ConnectionString, staffRepo);
            var staff = staffRepo.GetStaffById(staffId)!;
            var start = DateTime.Today.AddDays(40).AddHours(8);
            var shift = new Shift(0, staff, "Ward B", start, start.AddHours(8), ShiftStatus.SCHEDULED);
            var countBefore = shiftRepo.GetShiftsByStaffID(staffId).Count;

            shiftRepo.AddShift(shift);

            Assert.Equal(countBefore + 1, shiftRepo.GetShiftsByStaffID(staffId).Count);
        }
        finally
        {
            DeleteShiftsByStaff(conn, staffId);
            db.DeleteStaff(conn, staffId);
        }
    }

    [Fact]
    public void UpdateShiftStatus_ChangesStatusInDatabase()
    {
        using var conn = db.OpenConnection();
        var staffId = db.InsertStaff(conn, "Doctor", "UpdateStatus", "DbTest", "Oncology");
        var start = DateTime.Today.AddDays(41).AddHours(9);
        var shiftId = db.InsertShift(conn, staffId, "Ward C", start, start.AddHours(8), "SCHEDULED");
        try
        {
            var shiftRepo = new ShiftRepository(db.ConnectionString, new StaffRepository(db.ConnectionString));

            shiftRepo.UpdateShiftStatus(shiftId, ShiftStatus.ACTIVE);

            Assert.Equal("ACTIVE", db.GetShiftStatus(conn, shiftId));
        }
        finally
        {
            db.DeleteShift(conn, shiftId);
            db.DeleteStaff(conn, staffId);
        }
    }

    [Fact]
    public void CancelShift_RemovesShiftFromDatabase()
    {
        using var conn = db.OpenConnection();
        var staffId = db.InsertStaff(conn, "Doctor", "CancelShift", "DbTest", "Cardiology");
        var start = DateTime.Today.AddDays(42).AddHours(10);
        var shiftId = db.InsertShift(conn, staffId, "Ward D", start, start.AddHours(8));
        try
        {
            var shiftRepo = new ShiftRepository(db.ConnectionString, new StaffRepository(db.ConnectionString));

            shiftRepo.CancelShift(shiftId);

            Assert.Null(db.GetShiftStatus(conn, shiftId));
        }
        finally
        {
            db.DeleteStaff(conn, staffId);
        }
    }

    [Fact]
    public void GetShiftById_ReturnsCorrectShift()
    {
        using var conn = db.OpenConnection();
        var staffId = db.InsertStaff(conn, "Doctor", "GetById", "DbTest", "Neurology");
        var start = DateTime.Today.AddDays(43).AddHours(8);
        var shiftId = db.InsertShift(conn, staffId, "Ward E", start, start.AddHours(8));
        try
        {
            var shiftRepo = new ShiftRepository(db.ConnectionString, new StaffRepository(db.ConnectionString));

            var result = shiftRepo.GetShiftById(shiftId);

            Assert.NotNull(result);
            Assert.Equal(shiftId, result!.Id);
        }
        finally
        {
            db.DeleteShift(conn, shiftId);
            db.DeleteStaff(conn, staffId);
        }
    }

    [Fact]
    public void GetShiftById_WhenShiftDoesNotExist_ReturnsNull()
    {
        var shiftRepo = new ShiftRepository(db.ConnectionString, new StaffRepository(db.ConnectionString));

        Assert.Null(shiftRepo.GetShiftById(int.MaxValue));
    }

    [Fact]
    public void GetShiftsByStaffID_ReturnsOnlyMatchingStaffShifts()
    {
        using var conn = db.OpenConnection();
        var doctorOneId = db.InsertStaff(conn, "Doctor", "StaffA", "ShiftFilter", "Cardiology");
        var doctorTwoId = db.InsertStaff(conn, "Doctor", "StaffB", "ShiftFilter", "Neurology");
        var start = DateTime.Today.AddDays(44).AddHours(8);
        var shiftAId = db.InsertShift(conn, doctorOneId, "Ward F", start, start.AddHours(4));
        var shiftBId = db.InsertShift(conn, doctorTwoId, "Ward F", start.AddHours(4), start.AddHours(8));
        try
        {
            var shiftRepo = new ShiftRepository(db.ConnectionString, new StaffRepository(db.ConnectionString));

            var result = shiftRepo.GetShiftsByStaffID(doctorOneId);

            Assert.Contains(result, s => s.Id == shiftAId);
            Assert.DoesNotContain(result, s => s.Id == shiftBId);
        }
        finally
        {
            db.DeleteShift(conn, shiftAId);
            db.DeleteShift(conn, shiftBId);
            db.DeleteStaff(conn, doctorOneId);
            db.DeleteStaff(conn, doctorTwoId);
        }
    }

    [Fact]
    public void GetShiftsForStaffInRange_ReturnsOnlyShiftsWithinRange()
    {
        using var conn = db.OpenConnection();
        var staffId = db.InsertStaff(conn, "Doctor", "Dave", "ShiftRange", "Cardiology");
        var baseDate = DateTime.Today.AddDays(50);
        var inRange = db.InsertShift(conn, staffId, "Ward D", baseDate.AddHours(8), baseDate.AddHours(16));
        var outRange = db.InsertShift(conn, staffId, "Ward D", baseDate.AddDays(10).AddHours(8), baseDate.AddDays(10).AddHours(16));
        try
        {
            var shiftRepo = new ShiftRepository(db.ConnectionString, new StaffRepository(db.ConnectionString));

            var result = shiftRepo.GetShiftsForStaffInRange(staffId, baseDate, baseDate.AddDays(2));

            Assert.Contains(result, s => s.Id == inRange);
            Assert.DoesNotContain(result, s => s.Id == outRange);
        }
        finally
        {
            db.DeleteShift(conn, inRange);
            db.DeleteShift(conn, outRange);
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
