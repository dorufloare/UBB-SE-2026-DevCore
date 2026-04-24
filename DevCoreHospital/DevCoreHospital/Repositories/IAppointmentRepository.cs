using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevCoreHospital.Models;

namespace DevCoreHospital.Repositories
{
    public interface IAppointmentRepository
    {
        Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skip, int take);
        Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync();
        Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId);
        Task<IReadOnlyList<Appointment>> GetAppointmentsForAdminAsync(int doctorId);
        Task AddAppointmentAsync(Appointment appt);
        Task UpdateAppointmentStatusAsync(int id, string status);
        Task<int> GetActiveAppointmentsCountForDoctorAsync(int doctorId);
        Task UpdateDoctorStatusAsync(int doctorId, string status);
    }
}
