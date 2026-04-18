using System;
using System.Collections.Generic;
using System.Linq;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;

namespace DevCoreHospital.Services
{
    public class HangoutService : IHangoutService
    {
        private readonly HangoutRepository hangoutRepository;

        public HangoutService(HangoutRepository hangoutRepository)
        {
            this.hangoutRepository = hangoutRepository;
        }

        public void CreateHangout(string title, string description, DateTime date, int maxParticipants, IStaff creator)
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length < 5 || title.Length > 25)
            {
                throw new ArgumentException("Title must be between 5 and 25 characters.");
            }

            if (description != null && description.Length > 100)
            {
                throw new ArgumentException("Description must be at most 100 characters.");
            }

            if (date.Date < DateTime.Now.Date.AddDays(7))
            {
                throw new ArgumentException("The hangout date must be at least 1 week away from today.");
            }

            if (hangoutRepository.HasConflictsOnDate(creator.StaffID, date))
            {
                throw new InvalidOperationException("You cannot create a hangout on a day where you have active scheduled appointments.");
            }

            Hangout newHangout = new Hangout(0, title, description, date, maxParticipants);
            newHangout.ParticipantList.Add(creator);

            hangoutRepository.AddHangout(newHangout);
        }

        public void JoinHangout(int hangoutId, IStaff staff)
        {
            var hangout = hangoutRepository.GetHangoutById(hangoutId);
            if (hangout == null)
            {
                throw new ArgumentException("Hangout not found.");
            }

            if (hangout.ParticipantList.Count >= hangout.MaxParticipants)
            {
                throw new InvalidOperationException("This hangout is already full.");
            }

            if (hangout.ParticipantList.Any(p => p.StaffID == staff.StaffID))
            {
                throw new InvalidOperationException("You have already joined this hangout.");
            }

            if (hangoutRepository.HasConflictsOnDate(staff.StaffID, hangout.Date))
            {
                throw new InvalidOperationException("You cannot join a hangout on a day where you have active scheduled appointments.");
            }

            hangoutRepository.AddParticipant(hangoutId, staff.StaffID);
        }

        public List<Hangout> GetAllHangouts()
        {
            return hangoutRepository.GetAllHangouts();
        }
    }
}