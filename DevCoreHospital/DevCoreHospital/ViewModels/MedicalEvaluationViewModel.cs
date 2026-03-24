using System;
using System.Collections.ObjectModel; 
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DevCoreHospital.Models;
using DevCoreHospital.Data;

namespace DevCoreHospital.ViewModels
{
    public class MedicalEvaluationViewModel : INotifyPropertyChanged
    {
        private readonly MedicalDataService _dataService = new MedicalDataService();

        // Task 7: This is the source for your ListView
        public ObservableCollection<MedicalEvaluation> PastEvaluations { get; set; } = new ObservableCollection<MedicalEvaluation>();

        private string _symptoms = string.Empty;
        public string Symptoms { get => _symptoms; set { _symptoms = value; OnPropertyChanged(); } }


        public ICommand SaveDiagnosisCommand { get; }

        public MedicalEvaluationViewModel()
        {
            SaveDiagnosisCommand = new RelayCommand(SaveDiagnosis);
            LoadHistory();
        }

        private void LoadHistory()
        {
            // Task 7: Pull existing records for the current doctor
            var history = _dataService.GetEvaluationsByDoctor("DOC001");
            foreach (var item in history)
            {
                PastEvaluations.Add(item);
            }
        }

        private void SaveDiagnosis()
        {
            var newRecord = new MedicalEvaluation
            {
                Symptoms = this.Symptoms,
                MedsList = "N/A", // Simplified for now
                DoctorNotes = "N/A",
                EvaluationDate = DateTime.Now,
                Evaluator = new Doctor { Id = "DOC001", Name = "Dr. Vlad" }
            };

            _dataService.SaveEvaluation(newRecord);

            // Task 7: Add to the observable collection so the UI updates INSTANTLY
            PastEvaluations.Insert(0, newRecord); // Adds to the top of the list
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}