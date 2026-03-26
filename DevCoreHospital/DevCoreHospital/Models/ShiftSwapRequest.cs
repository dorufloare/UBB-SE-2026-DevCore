using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevCoreHospital.Models
{
    class ShiftSwapRequest
    {
        public int SwapId { get; set; }
        public int ShiftId { get; set; }
        public int RequesterId { get; set; }
        public int ColleagueId { get; set; }

        public ShiftSwapRequest(int swapId, int shiftId, int requesterId, int colleagueId)
        {
            SwapId = swapId;
            ShiftId = shiftId;
            RequesterId = requesterId;
            ColleagueId = colleagueId;
        }
    }
}
