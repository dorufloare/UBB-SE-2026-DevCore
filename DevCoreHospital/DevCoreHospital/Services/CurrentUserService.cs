namespace DevCoreHospital.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    public int UserId { get; } = 1;
    /// <summary>Use Doctor for Doctor Schedule; Pharmacist for Pharmacy Schedule (mock roster uses PHARM{UserId}).</summary>
    public string Role { get; } = "Pharmacist";
}