using System;
using System.Collections.Generic;
using System.Linq;
using DevCoreHospital.Configuration;
using DevCoreHospital.Data;
using DevCoreHospital.Models;

namespace DevCoreHospital.Repositories
{
    public class HangoutRepository
    {
        private readonly DatabaseManager dbManager;

        public HangoutRepository()
        {
            dbManager = new DatabaseManager(AppSettings.ConnectionString);
        }

        public void AddHangout(Hangout hangout)
        {
            int newId = dbManager.InsertHangout(hangout.Title, hangout.Description, hangout.Date, hangout.MaxParticipants);

            foreach (var p in hangout.ParticipantList)
            {
                dbManager.InsertHangoutParticipant(newId, p.StaffID);
            }
        }

        public void AddParticipant(int hangoutId, int staffId)
        {
            dbManager.InsertHangoutParticipant(hangoutId, staffId);
        }

        public List<Hangout> GetAllHangouts()
        {
            var list = dbManager.GetAllHangouts();

            foreach (var h in list)
            {
                var participants = dbManager.GetHangoutParticipants(h.HangoutID);
                h.ParticipantList.AddRange(participants);
            }

            return list;
        }

        public Hangout? GetHangoutById(int id)
        {
            var h = dbManager.GetHangoutById(id);
            if (h != null)
            {
                var participants = dbManager.GetHangoutParticipants(h.HangoutID);
                h.ParticipantList.AddRange(participants);
            }

            return h;
        }

        public bool HasConflictsOnDate(int staffId, DateTime date)
        {
            var statuses = dbManager.GetAppointmentStatusesForStaffOnDate(staffId, date);

            var activeConflicts = statuses.Where(status =>
                !string.Equals(status, "Finished", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(status, "Canceled", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase));

            return activeConflicts.Any();
        }
    }
}