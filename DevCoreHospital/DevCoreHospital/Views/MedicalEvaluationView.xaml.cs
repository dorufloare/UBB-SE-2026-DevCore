using DevCoreHospital.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace DevCoreHospital.Views
{
    public sealed partial class MedicalEvaluationView : Page
    {
        public MedicalEvaluationViewModel ViewModel { get; } = new MedicalEvaluationViewModel();

        public MedicalEvaluationView()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}