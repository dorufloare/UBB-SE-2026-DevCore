using DevCoreHospital.Models;

namespace DevCoreHospital.Services;

public class MockCurrentUserService : ICurrentUserService
{
    // Replace with real login context when Available
    public int UserId => 1;
    public UserRole RoleType { get; set; } = UserRole.Doctor;
    public string Role => RoleType.ToString();
}