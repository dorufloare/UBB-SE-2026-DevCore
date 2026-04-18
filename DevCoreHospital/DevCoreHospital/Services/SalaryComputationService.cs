using System.Collections.Generic;
using System.Threading.Tasks;
using DevCoreHospital.Data;
using DevCoreHospital.Models;

namespace DevCoreHospital.Services
{
    public class SalaryComputationService : ISalaryComputationService
    {
        private readonly DatabaseManager dbManager;

        public SalaryComputationService(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public Task<double> ComputeSalaryDoctorAsync(Doctor doctor, List<Shift> monthlyShifts, int month, int year)
        {
            double initialSalary = 0;
            double doctorHourlyRate = 85.0;

            foreach (var shift in monthlyShifts)
            {
                double dbHours = dbManager.GetShiftHoursFromDb(shift.Id);
                double shiftHours = dbHours > 0 ? dbHours : (shift.EndTime - shift.StartTime).TotalHours;

                double shiftSalary = shiftHours * doctorHourlyRate;

                if (shift.StartTime.DayOfWeek == System.DayOfWeek.Saturday)
                {
                    shiftSalary *= 1.15;
                }
                else if (shift.StartTime.DayOfWeek == System.DayOfWeek.Sunday)
                {
                    shiftSalary *= 1.25;
                }

                bool isNightShift = shift.StartTime.Hour >= 20 || shift.StartTime.Hour <= 6 || shift.EndTime.Hour <= 6;
                if (isNightShift)
                {
                    shiftSalary *= 1.20;
                }

                initialSalary += shiftSalary;
            }

            double finalSalary = initialSalary;

            double specBonusPercentage = 0;
            string spec = doctor.Specialization?.ToLower() ?? string.Empty;

            if (spec.Contains("surgeon") || spec.Contains("surgery"))
            {
                specBonusPercentage = 0.20;
            }
            else if (spec.Contains("cardiologist"))
            {
                specBonusPercentage = 0.15;
            }
            else if (spec.Contains("er") || spec.Contains("emergency"))
            {
                specBonusPercentage = 0.10;
            }

            finalSalary += (initialSalary * specBonusPercentage);
            finalSalary += (initialSalary * (doctor.YearsOfExperience * 0.02));

            try
            {
                if (dbManager.DidStaffParticipateInHangout(doctor.StaffID, month, year))
                {
                    finalSalary *= 1.05;
                }
            }
            catch
            {
            }

            return Task.FromResult(finalSalary);
        }

        public Task<double> ComputeSalaryPharmacistAsync(Pharmacyst pharmacist, List<Shift> monthlyShifts, int month, int year)
        {
            double initialSalary = 0;
            double pharmacistHourlyRate = 45.0;

            foreach (var shift in monthlyShifts)
            {
                double dbHours = dbManager.GetShiftHoursFromDb(shift.Id);
                double shiftHours = dbHours > 0 ? dbHours : (shift.EndTime - shift.StartTime).TotalHours;

                double shiftSalary = shiftHours * pharmacistHourlyRate;

                if (shift.StartTime.DayOfWeek == System.DayOfWeek.Saturday)
                {
                    shiftSalary *= 1.15;
                }
                else if (shift.StartTime.DayOfWeek == System.DayOfWeek.Sunday)
                {
                    shiftSalary *= 1.25;
                }

                bool isNightShift = shift.StartTime.Hour >= 20 || shift.StartTime.Hour <= 6 || shift.EndTime.Hour <= 6;
                if (isNightShift)
                {
                    shiftSalary *= 1.20;
                }

                initialSalary += shiftSalary;
            }

            double finalSalary = initialSalary;

            int medicinesSold;
            try
            {
                medicinesSold = dbManager.GetMedicinesSold(pharmacist.StaffID, month, year);
            }
            catch
            {
                medicinesSold = 0;
            }

            double medicineBonusPercent = (medicinesSold / 10) * 0.01;
            if (medicineBonusPercent > 0.30)
            {
                medicineBonusPercent = 0.30;
            }

            finalSalary += (initialSalary * medicineBonusPercent);
            finalSalary += (initialSalary * (pharmacist.YearsOfExperience * 0.02));

            try
            {
                if (dbManager.DidStaffParticipateInHangout(pharmacist.StaffID, month, year))
                {
                    finalSalary *= 1.05;
                }
            }
            catch
            {
            }

            return Task.FromResult(finalSalary);
        }
    }
}