using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using DevCoreHospital.Models;

namespace DevCoreHospital.Services;

public interface IPharmacyScheduleService
{
    public Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd);
}
