using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevCoreHospital.Data;
using DevCoreHospital.Models;

namespace DevCoreHospital.Repositories
{
    public class AppointmentRepository : IDoctorAppointmentDataSource
    {
        private readonly DatabaseManager dbManager;

        public AppointmentRepository(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skip, int take)
        {
            var toDate = fromDate.Date.AddDays(31);
            return await dbManager.GetUpcomingAppointmentsAsync(doctorUserId, fromDate.Date, toDate, skip, take);
        }

        public async Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync()
        {
            return await dbManager.GetAllDoctorsAsync();
        }

        public async Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId)
        {
            return await dbManager.GetAppointmentDetailsAsync(appointmentId);
        }

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsForAdminAsync(int doctorId)
        {
            return await dbManager.GetAppointmentsForAdminAsync(doctorId);
        }

        public async Task AddAppointmentAsync(Appointment appt)
        {
            string rawPatientInput = appt.PatientName?.Replace("PAT-", string.Empty).Trim() ?? "0";
            int.TryParse(rawPatientInput, out int patientId);

            DateTime startTimeDb = appt.Date.Date.Add(appt.StartTime);
            DateTime endTimeDb = appt.Date.Date.Add(appt.EndTime);

            await dbManager.AddAppointmentAsync(patientId, appt.DoctorId, startTimeDb, endTimeDb);
        }

        public async Task UpdateAppointmentStatusAsync(int id, string status)
        {
            await dbManager.UpdateAppointmentStatusAsync(id, status);
        }

        public async Task<int> GetActiveAppointmentsCountForDoctorAsync(int doctorId)
        {
            return await dbManager.GetActiveAppointmentsCountAsync(doctorId);
        }

        public async Task UpdateDoctorStatusAsync(int doctorId, string status)
        {
            await dbManager.UpdateDoctorStatusAsync(doctorId, status);
        }
    }
}