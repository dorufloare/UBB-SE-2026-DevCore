using System.Collections.Generic;
using DevCoreHospital.Models;

namespace DevCoreHospital.Repositories
{
    public interface IFatigueAuditRepository
    {
        IReadOnlyList<RosterShift> GetAllShifts();
        IReadOnlyList<StaffProfile> GetStaffProfiles();
        bool ReassignShift(int shiftId, int newStaffId);
    }
}
