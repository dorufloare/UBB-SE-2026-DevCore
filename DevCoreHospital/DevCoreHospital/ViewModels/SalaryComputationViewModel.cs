using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using DevCoreHospital.Configuration;
using DevCoreHospital.Data;
using DevCoreHospital.Models;
using DevCoreHospital.Services;
using DevCoreHospital.ViewModels.Base;

namespace DevCoreHospital.ViewModels
{
    public class SalaryComputationViewModel : ObservableObject
    {
        private readonly ISalaryComputationService salaryService;
        private readonly DatabaseManager dbManager;

        public ObservableCollection<IStaff> StaffList { get; } = new ObservableCollection<IStaff>();
        public ObservableCollection<Shift> ShiftList { get; } = new ObservableCollection<Shift>();

        private IStaff selectedStaff = default!;
        public IStaff SelectedStaff
        {
            get => selectedStaff;
            set
            {
                SetProperty(ref selectedStaff, value);
                ComputeSalaryCommand.RaiseCanExecuteChanged();
            }
        }

        private int selectedMonth = DateTime.Now.Month;
        public int SelectedMonth { get => selectedMonth; set => SetProperty(ref selectedMonth, value); }

        private int selectedYear = DateTime.Now.Year;
        public int SelectedYear { get => selectedYear; set => SetProperty(ref selectedYear, value); }

        private bool isLoading;
        public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => SetProperty(ref errorMessage, value); }

        private string salaryResult = string.Empty;
        public string SalaryResult { get => salaryResult; set => SetProperty(ref salaryResult, value); }

        public AsyncRelayCommand ComputeSalaryCommand { get; }

        public SalaryComputationViewModel()
        {
            dbManager = new DatabaseManager(AppSettings.ConnectionString);
            salaryService = new SalaryComputationService(dbManager);

            ComputeSalaryCommand = new AsyncRelayCommand(ComputeSalaryAsync, CanComputeSalary);

            LoadStaffList();
            LoadShiftList();
        }

        private void LoadStaffList()
        {
            StaffList.Clear();
            var staffFromDb = dbManager.GetStaff();
            foreach (var staff in staffFromDb)
            {
                StaffList.Add(staff);
            }
        }

        private void LoadShiftList()
        {
            ShiftList.Clear();
            var shiftsFromDb = dbManager.GetShifts();
            foreach (var shift in shiftsFromDb)
            {
                ShiftList.Add(shift);
            }
        }

        private bool CanComputeSalary()
        {
            return SelectedStaff != null && SelectedStaff.StaffID > 0;
        }

        private async Task ComputeSalaryAsync()
        {
            ErrorMessage = string.Empty;
            SalaryResult = string.Empty;
            IsLoading = true;

            try
            {
                // The ViewModel filters the shifts for the specific month/year
                var staffShifts = ShiftList.Where(s => s.AppointedStaff?.StaffID == SelectedStaff.StaffID
                                                    && s.StartTime.Month == SelectedMonth
                                                    && s.StartTime.Year == SelectedYear).ToList();

                double salary = 0;

                if (SelectedStaff is Models.Doctor doctor)
                {
                    // Pass the month and year to the doctor calculation
                    salary = await salaryService.ComputeSalaryDoctorAsync(doctor, staffShifts, SelectedMonth, SelectedYear);
                }
                else if (SelectedStaff is Models.Pharmacyst pharmacyst)
                {
                    // Pass the month and year to the pharmacyst calculation
                    salary = await salaryService.ComputeSalaryPharmacistAsync(pharmacyst, staffShifts, SelectedMonth, SelectedYear);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported staff type for salary computation.");
                }

                SalaryResult = $"Computed Salary: ${salary:F2}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Computation failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}