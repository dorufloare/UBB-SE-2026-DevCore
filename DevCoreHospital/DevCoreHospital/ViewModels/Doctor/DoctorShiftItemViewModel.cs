using System;
using DevCoreHospital.Models;

namespace DevCoreHospital.ViewModels.Doctor
{
    public sealed class DoctorShiftItemViewModel
    {
        public int Id { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public string Location { get; }
        public string Status { get; }

        public string DateText => StartTime.ToString("dd MMM yyyy");
        public string TimeRangeText => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
        public string LocationText => string.IsNullOrWhiteSpace(Location) ? "Location TBD" : Location;

        public DoctorShiftItemViewModel(Shift shift)
        {
            Id = shift.Id;
            StartTime = shift.StartTime;
            EndTime = shift.EndTime;
            Location = shift.Location ?? string.Empty;
            Status = shift.Status.ToString();
        }
    }
}

