using DevCoreHospital.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace DevCoreHospital.Views
{
    public sealed partial class SalaryPlaceholderPage : Page
    {
        public SalaryComputationViewModel ViewModel { get; }

        public SalaryPlaceholderPage()
        {
            this.InitializeComponent();

            // Initialize the ViewModel and set it as the DataContext for XAML bindings
            ViewModel = new SalaryComputationViewModel();
            this.DataContext = ViewModel;
        }
    }
}