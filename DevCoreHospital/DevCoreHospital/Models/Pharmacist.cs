using System;

namespace DevCoreHospital.Models;

public sealed class Pharmacist : IStaff
{
    public int StaffID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public bool Available { get; set; }
    public string Certification { get; set; } = string.Empty;
    public string Role { get; set; } = "Pharmacist";
    public string DisplayName
    {
        get
        {
            var fullName = $"{FirstName} {LastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? $"Pharmacist #{StaffID}" : fullName;
        }
        set
        {
            var name = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                FirstName = string.Empty;
                LastName = string.Empty;
                return;
            }

            var parts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            FirstName = parts[0];
            LastName = parts.Length > 1 ? parts[1] : string.Empty;
        }
    }
}

