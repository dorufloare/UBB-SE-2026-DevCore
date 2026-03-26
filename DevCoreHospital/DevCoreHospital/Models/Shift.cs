using System;

namespace DevCoreHospital.Models;

public class Shift
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string RotationAssignment { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = "ACTIVE";
}
