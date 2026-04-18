using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using DevCoreHospital.Models;

namespace DevCoreHospital.Services
{
    public interface IDoctorAppointmentService
    {
        Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsAsync(int doctorUserId, DateTime fromDate, int skip, int take);
        Task<IReadOnlyList<(int DoctorId, string DoctorName)>> GetAllDoctorsAsync();
        Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId);
        Task<IReadOnlyList<Appointment>> GetAppointmentsForAdminAsync(int doctorId);
        Task BookAppointmentAsync(Appointment appointment);
        Task FinishAppointmentAsync(Appointment appointment);
        Task CancelAppointmentAsync(Appointment appointment);
    }
}