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
                // COMPLETED shift from 5 hours ago (lasted 4 hours)
                _shiftsMockTable.Add(new Shift
                {
                    DoctorId = "DOC001",
                    StartTime = DateTime.Now.AddHours(-9),
                    EndTime = DateTime.Now.AddHours(-5),
                    Status = "COMPLETED"
                });

                // An active shift that started 2 hours ago
                _shiftsMockTable.Add(new Shift
                {
                    DoctorId = "DOC001",
                    StartTime = DateTime.Now.AddHours(-2),
                    Status = "ACTIVE"
                });
            }
        }

        public void SaveEvaluation(MedicalEvaluation record)
        {
            _mockTable.Add(record);
        }

        public List<MedicalEvaluation> GetEvaluationsByDoctor(string doctorId)
        {
            return _mockTable.Where(e => e.Evaluator != null && e.Evaluator.Id == doctorId).ToList();
        }

        // TASK 22: The method to get the total fatigue
        public double GetDoctorFatigueHours(string doctorId)
        {

            string sqlQuery = @"
                SELECT (
                    (SELECT IFNULL((julianday('now') - julianday(StartTime)) * 24, 0)
                     FROM Shifts 
                     WHERE DoctorID = @DoctorID AND Status = 'ACTIVE' LIMIT 1)
                    +
                    (SELECT IFNULL(SUM((julianday(EndTime) - julianday(StartTime)) * 24), 0)
                     FROM Shifts 
                     WHERE DoctorID = @DoctorID 
                     AND Status = 'COMPLETED' 
                     AND EndTime >= datetime('now', '-24 hours'))
                ) AS TotalDutyTime;";


            return CalculateMockFatigue(doctorId);
        }

        private double CalculateMockFatigue(string doctorId)
        {
            var now = DateTime.Now;
            var dayAgo = now.AddHours(-24);

            // 1. Calculate Active Shift hours
            var active = _shiftsMockTable.FirstOrDefault(s => s.DoctorId == doctorId && s.Status == "ACTIVE");
            double activeHours = active != null ? (now - active.StartTime).TotalHours : 0;

            // 2. Calculate Completed Shift hours (only from the last 24 hours)
            double completedHours = _shiftsMockTable
                .Where(s => s.DoctorId == doctorId && s.Status == "COMPLETED" && s.EndTime >= dayAgo)
                .Sum(s => (s.EndTime.Value - s.StartTime).TotalHours);

            return activeHours + completedHours;
        }
    }
}