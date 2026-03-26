namespace DevCoreHospital.Models;

public class Staff
{
    public int Id { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public bool IsAvailable { get; set; } = true;
}
