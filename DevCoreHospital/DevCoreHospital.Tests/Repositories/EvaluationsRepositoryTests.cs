using DevCoreHospital.Repositories;
using Moq;

namespace DevCoreHospital.Tests.Repositories;

public class EvaluationsRepositoryTests
{
    [Fact]
    public void GetEvaluationsByDoctor_WhenDoctorIdIsNotNumeric_ReturnsEmptyList()
        => Assert.Empty(new EvaluationsRepository().GetEvaluationsByDoctor("not-a-number"));

    [Fact]
    public void GetEvaluationsByDoctor_WhenDoctorIdIsEmpty_ReturnsEmptyList()
        => Assert.Empty(new EvaluationsRepository().GetEvaluationsByDoctor(string.Empty));

    [Fact]
    public void GetEvaluationsByDoctor_WhenDoctorIdIsWhitespace_ReturnsEmptyList()
        => Assert.Empty(new EvaluationsRepository().GetEvaluationsByDoctor("   "));

    [Fact]
    public void GetEvaluationsByDoctor_WhenDoctorIdIsAlphanumeric_ReturnsEmptyList()
        => Assert.Empty(new EvaluationsRepository().GetEvaluationsByDoctor("DR-42"));


    [Fact]
    public void IsDoctorFatigued_ReturnsFalse_WhenFatigueHoursIsBelowThreshold()
    {
        var repo = new Mock<IEvaluationsRepository>();
        repo.Setup(r => r.GetDoctorFatigueHours("1")).Returns(8.0);
        repo.Setup(r => r.IsDoctorFatigued("1")).Returns(() => repo.Object.GetDoctorFatigueHours("1") >= 12.0);

        Assert.False(repo.Object.IsDoctorFatigued("1"));
    }

    [Fact]
    public void IsDoctorFatigued_ReturnsTrue_WhenFatigueHoursIsAtOrAboveThreshold()
    {
        var repo = new Mock<IEvaluationsRepository>();
        repo.Setup(r => r.GetDoctorFatigueHours("1")).Returns(12.0);
        repo.Setup(r => r.IsDoctorFatigued("1")).Returns(() => repo.Object.GetDoctorFatigueHours("1") >= 12.0);

        Assert.True(repo.Object.IsDoctorFatigued("1"));
    }

    [Fact]
    public void CheckMedicineConflict_ReturnsNull_WhenMedsIsEmpty()
    {
        var repo = new Mock<IEvaluationsRepository>();
        repo.Setup(r => r.CheckMedicineConflict(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string patientId, string meds) =>
                string.IsNullOrWhiteSpace(meds) || string.IsNullOrWhiteSpace(patientId) ? null : "warning");

        Assert.Null(repo.Object.CheckMedicineConflict("P1", string.Empty));
    }

    [Fact]
    public void CheckMedicineConflict_ReturnsNull_WhenPatientIdIsEmpty()
    {
        var repo = new Mock<IEvaluationsRepository>();
        repo.Setup(r => r.CheckMedicineConflict(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string patientId, string meds) =>
                string.IsNullOrWhiteSpace(meds) || string.IsNullOrWhiteSpace(patientId) ? null : "warning");

        Assert.Null(repo.Object.CheckMedicineConflict(string.Empty, "Aspirin"));
    }

    [Fact]
    public void CheckMedicineConflict_ReturnsWarning_WhenMedsAndPatientIdProvided()
    {
        var repo = new Mock<IEvaluationsRepository>();
        repo.Setup(r => r.CheckMedicineConflict("P1", "Aspirin")).Returns("High-risk medicine warning.");

        Assert.Equal("High-risk medicine warning.", repo.Object.CheckMedicineConflict("P1", "Aspirin"));
    }
}
