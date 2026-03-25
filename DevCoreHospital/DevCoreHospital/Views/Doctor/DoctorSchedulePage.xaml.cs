using DevCoreHospital.Data;
using DevCoreHospital.Services;
using DevCoreHospital.ViewModels.Doctor;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevCoreHospital.Views.Doctor
{
    public sealed partial class DoctorSchedulePage : Page
    {
        public DoctorScheduleViewModel ViewModel { get; }

        public DoctorSchedulePage()
        {
            InitializeComponent();

            var sqlFactory = new SqlConnectionFactory();
            var appointmentService = new DoctorAppointmentService(sqlFactory);
            ICurrentUserService currentUserService = new CurrentUserService();

            ViewModel = new DoctorScheduleViewModel(currentUserService, appointmentService);
            DataContext = ViewModel;

            Loaded += DoctorSchedulePage_Loaded;
        }

        private async void DoctorSchedulePage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= DoctorSchedulePage_Loaded;
            await ViewModel.InitializeAsync();
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not AppointmentItemViewModel item)
                return;

            ViewModel.OpenDetails(item);
            Frame.Navigate(typeof(AppointmentDetailsPage), item);
        }
    }
}