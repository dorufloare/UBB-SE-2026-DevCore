using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevCoreHospital.Models
{
    public class Shift
    {
        public string Id { get; set; } = string.Empty;
        public string StaffId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; } 
        public ShiftStatus Status { get; set; } = ShiftStatus.SCHEDULED;

        public Shift() { }
        public Shift(string id, string staffId, string location, DateTime startTime, DateTime endTime, ShiftStatus status)
        {
            this.Id = id;
            this.StaffId = staffId;
            this.Location = location;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Status = status;
        }
    }
}
