using System.Collections.Generic;
using DevCoreHospital.Models;

namespace DevCoreHospital.Data
{
    public interface IFatigueShiftDataSource
    {
        IReadOnlyList<RosterShift> GetAllShifts();
        IReadOnlyList<StaffProfile> GetStaffProfiles();
        bool ReassignShift(int shiftId, int newStaffId);
    }
}
