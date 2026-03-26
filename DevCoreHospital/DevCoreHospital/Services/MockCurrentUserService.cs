namespace DevCoreHospital.Services;

public class MockCurrentUserService : ICurrentUserService
{
    // Replace with real login context when Available
    public int UserId => 1;
    public string Role => "Doctor";
}