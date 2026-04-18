using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;

namespace DevCoreHospital.Services;

public sealed class PharmacyScheduleService : IPharmacyScheduleService
{
    private readonly IShiftRepository shiftRepo;

    public PharmacyScheduleService(IShiftRepository shiftRepo)
    {
        this.shiftRepo = shiftRepo;
    }

    public Task<IReadOnlyList<Shift>> GetShiftsAsync(int pharmacistStaffId, DateTime rangeStart, DateTime rangeEnd)
    {
        return Task.Run<IReadOnlyList<Shift>>(
            () => shiftRepo.GetShiftsForStaffInRange(pharmacistStaffId, rangeStart, rangeEnd));
    }
}
