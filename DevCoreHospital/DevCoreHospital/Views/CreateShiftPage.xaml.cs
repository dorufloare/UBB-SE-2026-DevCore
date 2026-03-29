using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace DevCoreHospital.Views
{
    public sealed partial class CreateShiftPage : UserControl
    {
        public CreateShiftPage()
        {
            this.InitializeComponent();
            LoadStaffData();
        }

        private void LoadStaffData()
        {
            // Exemplu de date (Mockup) până legăm de baza de date
            var staff = new List<string> { "Dr. Andrei Ionescu", "Dr. Elena Radu", "Farm. Mihai Pop" };
            EmployeeComboBox.ItemsSource = staff;
        }

        private void SaveShift_Click(object sender, RoutedEventArgs e)
        {
            // Validare de bază
            if (EmployeeComboBox.SelectedItem == null || ShiftDatePicker.Date == null ||
                StartTimePicker.SelectedTime == null || EndTimePicker.SelectedTime == null)
            {
                ShowMessage("Eroare: Te rugăm să completezi toate câmpurile.", InfoBarSeverity.Error);
                return;
            }

            var start = StartTimePicker.SelectedTime.Value;
            var end = EndTimePicker.SelectedTime.Value;

            if (end <= start)
            {
                ShowMessage("Atenție: Ora de final trebuie să fie după ora de început.", InfoBarSeverity.Warning);
                return;
            }

            // TODO: Aici va veni codul care apelează ViewModel-ul pentru salvarea în baza de date
            // ex: ViewModel.SaveShiftAsync(...);

            ShowMessage("Tura a fost salvată cu succes!", InfoBarSeverity.Success);

            // Opțional: Resetăm formularul după succes
            EmployeeComboBox.SelectedIndex = -1;
        }

        private void ShowMessage(string message, InfoBarSeverity severity)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = severity;
            StatusInfoBar.IsOpen = true;
        }
    }
}