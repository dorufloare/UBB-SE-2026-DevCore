using System;
using System.Collections.Generic;
using DevCoreHospital.Models;

namespace DevCoreHospital.Repositories
{
    public interface IHangoutRepository
    {
        int AddHangout(Hangout hangout);
        void AddParticipant(int hangoutId, int staffId);
        List<Hangout> GetAllHangouts();
        Hangout? GetHangoutById(int id);
        IReadOnlyList<string> GetAppointmentStatusesForStaffOnDate(int staffId, DateTime date);
    }
}
