using System;
using System.Collections.Generic;
using DevCoreHospital.Data;
using DevCoreHospital.Models;

namespace DevCoreHospital.Repositories
{
    public sealed class FatigueAuditRepository : IFatigueAuditRepository
    {
        private readonly IFatigueShiftDataSource dataSource;

        public FatigueAuditRepository(IFatigueShiftDataSource dataSource)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        }

        public IReadOnlyList<RosterShift> GetAllShifts()
        {
            return dataSource.GetAllShifts();
        }

        public IReadOnlyList<StaffProfile> GetStaffProfiles()
        {
            return dataSource.GetStaffProfiles();
        }

        public bool ReassignShift(int shiftId, int newStaffId)
        {
            return dataSource.ReassignShift(shiftId, newStaffId);
        }
    }
}
