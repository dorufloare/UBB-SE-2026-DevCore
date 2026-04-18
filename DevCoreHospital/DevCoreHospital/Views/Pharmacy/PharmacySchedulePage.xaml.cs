using System;
using DevCoreHospital.Configuration;
using DevCoreHospital.Data;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using DevCoreHospital.ViewModels.Pharmacy;
using Microsoft.UI.Xaml.Controls;

namespace DevCoreHospital.Views.Pharmacy;

public sealed partial class PharmacySchedulePage : Page
{
    public PharmacyScheduleViewModel ViewModel { get; }

    public PharmacySchedulePage()
    {
        InitializeComponent();

        ICurrentUserService currentUser = new CurrentUserService();
        var sqlFactory = new SqlConnectionFactory();
        var dbManager = new DatabaseManager(AppSettings.ConnectionString);
        var shiftRepo = new ShiftRepository(dbManager);
        var staffRepo = new StaffRepository(dbManager);
        var scheduleService = new PharmacyScheduleService(shiftRepo);
        ViewModel = new PharmacyScheduleViewModel(currentUser, scheduleService, staffRepo);
        DataContext = ViewModel;

        Loaded += PharmacySchedulePage_Loaded;
    }

    private async void PharmacySchedulePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= PharmacySchedulePage_Loaded;
        await ViewModel.InitializeAsync();
    }

    private void DateCalendar_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
    {
        if (sender.SelectedDates == null || sender.SelectedDates.Count == 0)
        {
            return;
        }

        var picked = sender.SelectedDates[0].Date;
        var minSqlDate = new DateTime(1753, 1, 1);

        if (picked < minSqlDate)
        {
            return;
        }

        ViewModel.AnchorDate = picked;
    }
}
