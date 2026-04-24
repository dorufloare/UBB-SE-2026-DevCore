using System.Linq;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;

namespace DevCoreHospital.Tests.Repositories;

public class StaffRepositoryTests : IClassFixture<SqlTestFixture>
{
    private readonly SqlTestFixture db;
    private const string InvalidConnectionString = "InvalidConnectionString";

    public StaffRepositoryTests(SqlTestFixture db) => this.db = db;

    [Fact]
    public void LoadAllStaff_WhenConnectionFails_ReturnsEmptyList()
        => Assert.Empty(new StaffRepository(InvalidConnectionString).LoadAllStaff());

    [Fact]
    public void GetStaffById_WhenConnectionFails_ReturnsNull()
        => Assert.Null(new StaffRepository(InvalidConnectionString).GetStaffById(1));

    [Fact]
    public void GetPharmacists_WhenConnectionFails_ReturnsEmptyList()
        => Assert.Empty(new StaffRepository(InvalidConnectionString).GetPharmacists());

    [Fact]
    public void GetAvailableDoctors_WhenConnectionFails_ReturnsEmptyList()
        => Assert.Empty(new StaffRepository(InvalidConnectionString).GetAvailableDoctors());

    [Fact]
    public void GetDoctorsBySpecialization_WhenConnectionFails_ReturnsEmptyList()
        => Assert.Empty(new StaffRepository(InvalidConnectionString).GetDoctorsBySpecialization("Cardiology"));

    [Fact]
    public void GetPharmacystsByCertification_WhenConnectionFails_ReturnsEmptyList()
        => Assert.Empty(new StaffRepository(InvalidConnectionString).GetPharmacystsByCertification("Sterile Compounding"));

    [Fact]
    public void UpdateStaffAvailability_WhenConnectionFails_DoesNotThrow()
    {
        var ex = Record.Exception(() => new StaffRepository(InvalidConnectionString).UpdateStaffAvailability(999, true, DoctorStatus.AVAILABLE));

        Assert.Null(ex);
    }

    [Fact]
    public void LoadAllStaff_ReturnsDoctorInsertedInDatabase()
    {
        using var conn = db.OpenConnection();
        var staffId = db.InsertStaff(conn, "Doctor", "Alice", "LoadAll", "Cardiology");
        try
        {
            Assert.Contains(new StaffRepository(db.ConnectionString).LoadAllStaff(), s => s.StaffID == staffId);
        }
        finally
        {
            db.DeleteStaff(conn, staffId);
        }
    }

    [Fact]
    public void GetStaffById_ReturnsCorrectStaff()
    {
        using var conn = db.OpenConnection();
        var staffId = db.InsertStaff(conn, "Doctor", "Bob", "GetById", "Neurology");
        try
        {
            var result = new StaffRepository(db.ConnectionString).GetStaffById(staffId);

            Assert.NotNull(result);
            Assert.Equal(staffId, result!.StaffID);
            Assert.Equal("Bob", result.FirstName);
        }
        finally
        {
            db.DeleteStaff(conn, staffId);
        }
    }

    [Fact]
    public void GetAvailableDoctors_ReturnsOnlyAvailableDoctors()
    {
        using var conn = db.OpenConnection();
        var availableId = db.InsertStaff(conn, "Doctor", "Carol", "AvailDoc", "Oncology", status: "Available", isAvailable: true);
        var unavailableId = db.InsertStaff(conn, "Doctor", "Dave", "AvailDoc", "Oncology", status: "Off_Duty", isAvailable: false);
        try
        {
            var result = new StaffRepository(db.ConnectionString).GetAvailableDoctors();

            Assert.Contains(result, d => d.StaffID == availableId);
            Assert.DoesNotContain(result, d => d.StaffID == unavailableId);
        }
        finally
        {
            db.DeleteStaff(conn, availableId);
            db.DeleteStaff(conn, unavailableId);
        }
    }

    [Fact]
    public void GetPharmacists_ReturnsPharmacistInsertedInDatabase()
    {
        using var conn = db.OpenConnection();
        var pharmacistId = db.InsertStaff(conn, "Pharmacist", "Eve", "GetPharm", certification: "PharmD");
        try
        {
            Assert.Contains(new StaffRepository(db.ConnectionString).GetPharmacists(), p => p.StaffID == pharmacistId);
        }
        finally
        {
            db.DeleteStaff(conn, pharmacistId);
        }
    }

    [Fact]
    public void GetDoctorsBySpecialization_ReturnsOnlyMatchingSpecialization()
    {
        using var conn = db.OpenConnection();
        var cardiologistId = db.InsertStaff(conn, "Doctor", "Frank", "BySpec", "Cardiology");
        var neurologistId = db.InsertStaff(conn, "Doctor", "Grace", "BySpec", "Neurology");
        try
        {
            var result = new StaffRepository(db.ConnectionString).GetDoctorsBySpecialization("Cardiology");

            Assert.Contains(result, d => d.StaffID == cardiologistId);
            Assert.DoesNotContain(result, d => d.StaffID == neurologistId);
        }
        finally
        {
            db.DeleteStaff(conn, cardiologistId);
            db.DeleteStaff(conn, neurologistId);
        }
    }

    [Fact]
    public void GetPharmacystsByCertification_ReturnsOnlyMatchingCertification()
    {
        using var conn = db.OpenConnection();
        var bcpsId = db.InsertStaff(conn, "Pharmacist", "Henry", "ByCert", certification: "BCPS");
        var pharmDId = db.InsertStaff(conn, "Pharmacist", "Iris", "ByCert", certification: "PharmD");
        try
        {
            var result = new StaffRepository(db.ConnectionString).GetPharmacystsByCertification("BCPS");

            Assert.Contains(result, p => p.StaffID == bcpsId);
            Assert.DoesNotContain(result, p => p.StaffID == pharmDId);
        }
        finally
        {
            db.DeleteStaff(conn, bcpsId);
            db.DeleteStaff(conn, pharmDId);
        }
    }

    [Fact]
    public void UpdateStaffAvailability_UpdatesAvailabilityAndStatusInDatabase()
    {
        using var conn = db.OpenConnection();
        var staffId = db.InsertStaff(conn, "Doctor", "Mia", "UpdateAvail", "Cardiology", status: "Available", isAvailable: true);
        try
        {
            var repo = new StaffRepository(db.ConnectionString);

            repo.UpdateStaffAvailability(staffId, false, DoctorStatus.OFF_DUTY);

            var updated = repo.GetStaffById(staffId) as Doctor;
            Assert.NotNull(updated);
            Assert.False(updated!.Available);
            Assert.Equal(DoctorStatus.OFF_DUTY, updated.DoctorStatus);
        }
        finally
        {
            db.DeleteStaff(conn, staffId);
        }
    }

}
