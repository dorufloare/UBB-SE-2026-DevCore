using System.Collections.Generic;
using System;
using DevCoreHospital.Models;

namespace DevCoreHospital.Repositories;

public interface IShiftRepository
{
    float GetWeeklyHours(int staffId);

    IReadOnlyList<Shift> GetShiftsForStaffInRange(int staffId, DateTime rangeStart, DateTime rangeEnd);
}
