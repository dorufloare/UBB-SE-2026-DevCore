using DevCoreHospital.Configuration;
using DevCoreHospital.Data;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using DevCoreHospital.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace DevCoreHospital.Views
{
    public sealed partial class ShiftSwapPlaceholderPage : Page
    {
        public FatigueShiftAuditViewModel ViewModel { get; }

        public ShiftSwapPlaceholderPage()
        {
            InitializeComponent();

            var sqlDataSource = new SqlFatigueShiftDataSource(AppSettings.ConnectionString);
            var repository = new FatigueAuditRepository(sqlDataSource);
            var service = new FatigueAuditService(repository);

            ViewModel = new FatigueShiftAuditViewModel(service);

            DataContext = ViewModel;
        }

        private void RunAutoAudit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RunAutoAudit();
        }
    }
}