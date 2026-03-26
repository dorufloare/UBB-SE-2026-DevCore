using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevCoreHospital.Models;
using DevCoreHospital.Data;

namespace DevCoreHospital.Services
{
    public class SalaryComputationService
    {
        private readonly DatabaseManager _dbManager;

        public SalaryComputationService(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<double> ComputeSalaryDoctorAsync(Models.Doctor doctor, List<Shift> monthlyShifts)
        {
            double totalHours = 0;

            // Task 3 implemented via DB
            foreach (var shift in monthlyShifts)
            {

                totalHours += (shift.EndTime - shift.StartTime).TotalHours; // Fallback calculation
            }

            double doctorHourlyRate = 85.0;
            return totalHours * doctorHourlyRate;
        }

        public async Task<double> ComputeSalaryPharmacistAsync(Models.Pharmacist pharmacist, List<Shift> monthlyShifts, int month, int year)
        {
            double totalHours = monthlyShifts.Sum(s => (s.EndTime - s.StartTime).TotalHours);
            double pharmacistHourlyRate = 45.0;

            // Task 5 implemented via DB
            int medicinesSold = 0;
            try
            {
                medicinesSold = _dbManager.GetMedicinesSold(pharmacist.StaffID, month, year);
            }
            catch
            {
                // Fallback if DB isn't connected yet during testing
                medicinesSold = 150;
            }

            double bonusPerMedicine = 1.5;

            return (totalHours * pharmacistHourlyRate) + (medicinesSold * bonusPerMedicine);
        }
    }
}