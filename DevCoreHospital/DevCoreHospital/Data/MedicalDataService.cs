using System.Collections.Generic;
using DevCoreHospital.Models;
using System.Linq;
using System;

namespace DevCoreHospital.Data
{
    public class MedicalDataService
    {
        private static List<MedicalEvaluation> _mockTable = new List<MedicalEvaluation>();

        // --- testing table for Shifts to test ---
        private static List<Shift> _shiftsMockTable = new List<Shift>();

        public MedicalDataService()
        {
          
            if (_shiftsMockTable.Count == 0)
            {
                _shiftsMockTable.Add(new Shift(1, new Doctor(1, "John", "Doe", "0700-000 000", true, "Cardiology", "12345", DoctorStatus.AVAILABLE), "Cardiology", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.ACTIVE));
                _shiftsMockTable.Add(new Shift(2, new Doctor(2, "Jane", "Smith", "0700-000 001", false, "Neurology", "54321", DoctorStatus.IN_EXAMINATION), "Neurology", DateTime.Now, DateTime.Now.AddHours(8), ShiftStatus.SCHEDULED));
               
            }
        }

        public void SaveEvaluation(MedicalEvaluation record)
        {
            _mockTable.Add(record);
        }

        public List<MedicalEvaluation> GetEvaluationsByDoctor(string doctorId)
        {
            return _mockTable.Where(e => e.Evaluator != null && e.Evaluator.StaffID.ToString() == doctorId).ToList();
        }

        public List<MedicalEvaluation> GetPatientMedicalHistory(string patientId)
        {
            return _mockTable
                .Where(e => string.Equals(e.PatientId, patientId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.EvaluationDate)
                .ToList();
        }

        public void UpdateAppointmentStatus(string patientId, string status)
        {
            // Mock service placeholder: no backing appointment table in this in-memory implementation.
        }

        public void UpdateDoctorAvailability(string doctorId)
        {
            // Mock service placeholder: no doctor availability persistence in this in-memory implementation.
        }

        public void CreateAdminFatigueAlert(string doctorId)
        {
            // Mock service placeholder: would emit an admin notification in real DB-backed implementation.
        }

        public double GetDoctorFatigueHours(string doctorId)
        {
            return CalculateMockFatigue(doctorId);
        }

        private double CalculateMockFatigue(string doctorId)
        {
            var now = DateTime.Now;
            var dayAgo = now.AddHours(-24);

            // 1. Calculate Active Shift
            var active = _shiftsMockTable.FirstOrDefault(s => s.AppointedStaff != null && s.AppointedStaff.StaffID.ToString() == doctorId && s.Status == ShiftStatus.ACTIVE);
            double activeHours = active != null ? (now - active.StartTime).TotalHours : 0;

            // 2. Calculate Completed Shift hours (only from the last 24 hours)
            double completedHours = _shiftsMockTable
                .Where(s => s.AppointedStaff != null && s.AppointedStaff.StaffID.ToString() == doctorId && s.Status == ShiftStatus.COMPLETED && s.EndTime >= dayAgo)
                .Sum(s => (s.EndTime - s.StartTime).TotalHours);

            return activeHours + completedHours;
        }
    }
}