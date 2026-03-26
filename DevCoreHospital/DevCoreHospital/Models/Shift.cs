using System;

namespace DevCoreHospital.Models
{
    public class Shift
    {
        public int Id { get; set; }                  // optional if DB has it
        public string DoctorId { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }       // keep nullable for ACTIVE shifts
        public string Status { get; set; } = "";
        public string Location { get; set; } = "";   // #21
    }
}