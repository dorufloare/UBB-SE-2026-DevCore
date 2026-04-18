using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DevCoreHospital.Configuration;
using DevCoreHospital.Data;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using DevCoreHospital.ViewModels.Base;

namespace DevCoreHospital.ViewModels.Doctor
{
    public class HangoutViewModel : ObservableObject
    {
        private readonly IHangoutService hangoutService;
        private readonly DatabaseManager dbManager;

        public ObservableCollection<Hangout> Hangouts { get; } = new ObservableCollection<Hangout>();

        public ObservableCollection<DoctorScheduleViewModel.DoctorOption> Doctors { get; } = new ObservableCollection<DoctorScheduleViewModel.DoctorOption>();

        private DoctorScheduleViewModel.DoctorOption? selectedDoctor;
        public DoctorScheduleViewModel.DoctorOption? SelectedDoctor
        {
            get => selectedDoctor;
            set
            {
                SetProperty(ref selectedDoctor, value);
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                SetProperty(ref title, value);
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private string description = string.Empty;
        public string Description
        {
            get => description;
            set
            {
                SetProperty(ref description, value);
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private DateTimeOffset selectedDate = DateTimeOffset.Now.AddDays(7);
        public DateTimeOffset SelectedDate
        {
            get => selectedDate;
            set
            {
                SetProperty(ref selectedDate, value);
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        private int maxParticipants = 5;
        public int MaxParticipants
        {
            get => maxParticipants;
            set
            {
                SetProperty(ref maxParticipants, value);
                CreateCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<int> MaxParticipantsOptions { get; } = new ObservableCollection<int> { 2, 3, 4, 5, 10, 15, 20 };

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        private string successMessage = string.Empty;
        public string SuccessMessage
        {
            get => successMessage;
            set => SetProperty(ref successMessage, value);
        }

        public RelayCommand CreateCommand { get; }

        private static HangoutRepository globalRepo = new HangoutRepository();

        public HangoutViewModel()
        {
            hangoutService = new HangoutService(globalRepo);
            dbManager = new DatabaseManager(AppSettings.ConnectionString);

            CreateCommand = new RelayCommand(CreateHangout, CanCreateHangout);
            LoadHangouts();
            _ = LoadDoctorsAsync();
        }

        private async Task LoadDoctorsAsync()
        {
            Doctors.Clear();
            try
            {
                var allDoctors = await dbManager.GetAllDoctorsAsync();
                foreach (var d in allDoctors.OrderBy(x => x.DoctorName))
                {
                    Doctors.Add(new DoctorScheduleViewModel.DoctorOption
                    {
                        DoctorId = d.DoctorId,
                        DoctorName = d.DoctorName,
                        FirstName = DoctorScheduleViewModel.DoctorOption.SplitFirstLast(d.DoctorName).FirstName,
                        LastName = DoctorScheduleViewModel.DoctorOption.SplitFirstLast(d.DoctorName).LastName,
                    });
                }

                if (Doctors.Any())
                {
                    SelectedDoctor = Doctors.First();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load doctors: {ex.Message}";
            }
        }

        private void LoadHangouts()
        {
            Hangouts.Clear();
            foreach (var h in hangoutService.GetAllHangouts())
            {
                Hangouts.Add(h);
            }
        }

        private bool CanCreateHangout() => Title.Length >= 5 && Title.Length <= 25 && Description.Length <= 100 && SelectedDoctor != null;

        private void CreateHangout()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            try
            {
                var currentDoctor = new Models.Doctor
                {
                    StaffID = SelectedDoctor!.DoctorId,
                    FirstName = SelectedDoctor.FirstName,
                    LastName = SelectedDoctor.LastName,
                };

                hangoutService.CreateHangout(Title, Description, SelectedDate.DateTime, MaxParticipants, currentDoctor);
                SuccessMessage = "Hangout created successfully!";
                LoadHangouts();

                Title = string.Empty;
                Description = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public void JoinHangoutById(int id)
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (SelectedDoctor == null)
            {
                ErrorMessage = "Please select a doctor to join the hangout.";
                return;
            }

            try
            {
                var currentDoctor = new Models.Doctor
                {
                    StaffID = SelectedDoctor.DoctorId,
                    FirstName = SelectedDoctor.FirstName,
                    LastName = SelectedDoctor.LastName,
                };

                hangoutService.JoinHangout(id, currentDoctor);
                SuccessMessage = "Joined hangout successfully!";
                LoadHangouts();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}