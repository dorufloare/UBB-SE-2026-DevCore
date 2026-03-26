using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DevCoreHospital.ViewModels;

namespace DevCoreHospital.Views
{
    public sealed partial class MedicalEvaluationView : Page
    {
        public MedicalEvaluationViewModel ViewModel { get; } = new MedicalEvaluationViewModel();

        public MedicalEvaluationView()
        {
            this.InitializeComponent();

            // Setting DataContext allows standard {Binding} to work alongside {x:Bind}
            this.DataContext = ViewModel;
        }
    }
}