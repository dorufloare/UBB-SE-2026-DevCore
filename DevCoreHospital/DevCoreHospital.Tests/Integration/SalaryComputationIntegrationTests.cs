using System;
using System.Threading.Tasks;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using DevCoreHospital.Tests.Repositories;
using DevCoreHospital.ViewModels;
using Xunit;

namespace DevCoreHospital.Tests.Integration;

public class SalaryComputationIntegrationTests : IClassFixture<SqlTestFixture>
{
    private readonly SqlTestFixture db;

    public SalaryComputationIntegrationTests(SqlTestFixture db) => this.db = db;

    [Fact]
    public async Task RepoServiceViewModel_Integration_ComputesSalaryThroughAllLayers()
    {
        using var conn = db.OpenConnection();

        // Insert a real doctor (Emergency medicine, 6 years) so the DB has the row
        var doctorId = db.InsertStaff(conn, "Doctor", "SalaryDoc", "Integration",
            specialization: "Emergency medicine", yearsExp: 6);

        // Insert a real 8-hour shift in May 2026 so GetShiftHoursFromDb returns 8
        var shiftStart = new DateTime(2026, 5, 1, 8, 0, 0);
        var shiftId    = db.InsertShift(conn, doctorId, "Ward A", shiftStart, shiftStart.AddHours(8));

        // Insert a hangout in May 2026 + participant so DidStaffParticipateInHangout returns true
        var hangoutId = db.InsertHangout(conn, "May Integration Hangout", new DateTime(2026, 5, 10));
        db.InsertHangoutParticipant(conn, hangoutId, doctorId);

        try
        {
            // Build model objects using the DB-assigned IDs so the repo queries match
            var doctor = new Doctor { StaffID = doctorId, Specialization = "Emergency medicine", YearsOfExperience = 6 };
            var shift  = new Shift(shiftId, doctor, "Ward A", shiftStart, shiftStart.AddHours(8), ShiftStatus.SCHEDULED);

            var repo      = new SalaryRepository(db.ConnectionString);
            var service   = new SalaryComputationService(repo);
            var viewModel = new SalaryComputationViewModel(service, new IStaff[] { doctor }, new[] { shift })
            {
                SelectedStaff = doctor,
                SelectedMonth = 5,
                SelectedYear  = 2026,
            };

            await viewModel.ComputeSalaryCommand.ExecuteAsync();

            Assert.Equal($"Computed Salary: $871{GetSeparator()}08", viewModel.SalaryResult);
            Assert.Equal(string.Empty, viewModel.ErrorMessage);
        }
        finally
        {
            db.DeleteHangoutParticipants(conn, hangoutId);
            db.DeleteHangout(conn, hangoutId);
            db.DeleteShift(conn, shiftId);
            db.DeleteStaff(conn, doctorId);
        }
    }

    private static string GetSeparator()
        => System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
}
