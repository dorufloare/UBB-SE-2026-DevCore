using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevCoreHospital.Models;
using DevCoreHospital.Data;

namespace DevCoreHospital.ViewModels
{
    public partial class MedicalEvaluationViewModel : ObservableObject
    {
        private readonly MedicalDataService _dataService = new();
        private const string CurrentDoctorId = "DOC001";
        private const string CurrentPatientId = "7759376";

        public ObservableCollection<MedicalEvaluation> PastEvaluations { get; } = new();

        private string _symptoms = string.Empty;
        public string Symptoms
        {
            get => _symptoms;
            set { if (SetProperty(ref _symptoms, value)) RefreshButtonState(); }
        }

        private string _medsList = string.Empty;
        public string MedsList
        {
            get => _medsList;
            set { if (SetProperty(ref _medsList, value)) ValidateMedsConflict(value); }
        }

        private string _doctorNotes = string.Empty;
        public string DoctorNotes
        {
            get => _doctorNotes;
            set => SetProperty(ref _doctorNotes, value);
        }

        private string _conflictWarning = string.Empty;
        public string ConflictWarning
        {
            get => _conflictWarning;
            set => SetProperty(ref _conflictWarning, value);
        }

        private bool _isConflictVisible;
        public bool IsConflictVisible
        {
            get => _isConflictVisible;
            set
            {
                if (SetProperty(ref _isConflictVisible, value))
                {
                    OnPropertyChanged(nameof(NotesBackground));
                    IsRiskAssumed = false;
                    RefreshButtonState();
                }
            }
        }

        private bool _isRiskAssumed;
        public bool IsRiskAssumed
        {
            get => _isRiskAssumed;
            set { if (SetProperty(ref _isRiskAssumed, value)) RefreshButtonState(); }
        }

        public Brush NotesBackground => IsConflictVisible
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(100, 255, 255, 0))
            : new SolidColorBrush(Colors.Transparent);

        private bool _isFatigued;
        public bool IsFatigued
        {
            get => _isFatigued;
            set
            {
                if (SetProperty(ref _isFatigued, value))
                {
                    OnPropertyChanged(nameof(IsFormEnabled));
                    OnPropertyChanged(nameof(LockoutVisibility));
                    RefreshButtonState();
                }
            }
        }

        public bool IsFormEnabled => !IsFatigued;
        public Visibility LockoutVisibility => IsFatigued ? Visibility.Visible : Visibility.Collapsed;

        public MedicalEvaluationViewModel()
        {
            PopulateHistory();
            CheckDoctorFatigue();
        }

        private void ValidateMedsConflict(string currentMeds)
        {
            if (string.IsNullOrWhiteSpace(currentMeds))
            {
                IsConflictVisible = false;
                return;
            }

            var history = _dataService.GetPatientMedicalHistory(CurrentPatientId);
            var riskKeywords = new[] { "Allergy", "Adverse Reaction", "Allergic" };

            foreach (var record in history)
            {
                bool hasRiskKeyword = riskKeywords.Any(k =>
                    (record.Symptoms?.Contains(k, StringComparison.OrdinalIgnoreCase) ?? false));

                if (hasRiskKeyword)
                {
                    var drugsTyped = currentMeds.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var drug in drugsTyped)
                    {
                        if (drug.Length > 3 && record.Symptoms.Contains(drug, StringComparison.OrdinalIgnoreCase))
                        {
                            ConflictWarning = $"⚠️ CONFLICT: Historical allergy to '{drug}' detected!";
                            IsConflictVisible = true;
                            return;
                        }
                    }
                }
            }
            IsConflictVisible = false;
        }

        private bool CanSaveDiagnosis => !IsFatigued &&
                                         !string.IsNullOrWhiteSpace(Symptoms) &&
                                         (!IsConflictVisible || IsRiskAssumed);

        [RelayCommand(CanExecute = nameof(CanSaveDiagnosis))]
        private void SaveDiagnosis()
        {
            string finalSymptoms = this.Symptoms;

            if (IsConflictVisible && IsRiskAssumed)
            {
                finalSymptoms = $"⚠️ [RISK ACKNOWLEDGED] - {finalSymptoms}";
            }

            var newRecord = new MedicalEvaluation
            {
                PatientId = CurrentPatientId,
                Symptoms = finalSymptoms,
                MedsList = this.MedsList,
                Notes = this.DoctorNotes,
                EvaluationDate = DateTime.Now,
                Evaluator = new global::DevCoreHospital.Models.Doctor { Id = CurrentDoctorId, Name = "Dr. Vlad" }
            };

            // 1. Persist the Medical Data
            _dataService.SaveEvaluation(newRecord);
            PastEvaluations.Insert(0, newRecord);


            // 2. Set the appointent to 'Finished'
            _dataService.UpdateAppointmentStatus(CurrentPatientId, "Finished");

            // 3. Set the Doctor to 'AVAILABLE'
            _dataService.UpdateDoctorAvailability(CurrentDoctorId);


            ResetForm();
            CheckDoctorFatigue();
        }

        private void ResetForm()
        {
            _symptoms = string.Empty;
            _medsList = string.Empty;
            _doctorNotes = string.Empty;
            _isRiskAssumed = false;
            _isConflictVisible = false;

            OnPropertyChanged(nameof(Symptoms));
            OnPropertyChanged(nameof(MedsList));
            OnPropertyChanged(nameof(DoctorNotes));
            OnPropertyChanged(nameof(IsRiskAssumed));
            OnPropertyChanged(nameof(IsConflictVisible));
            OnPropertyChanged(nameof(NotesBackground));

            RefreshButtonState();
        }

        private void RefreshButtonState() => SaveDiagnosisCommand.NotifyCanExecuteChanged();

        public void PopulateHistory()
        {
            PastEvaluations.Clear();
            var records = _dataService.GetEvaluationsByDoctor(CurrentDoctorId);
            foreach (var record in records) { PastEvaluations.Add(record); }
        }

        private void CheckDoctorFatigue()
        {
            double fatigueHours = _dataService.GetDoctorFatigueHours(CurrentDoctorId);
            IsFatigued = fatigueHours >= 12.0;
            if (IsFatigued) _dataService.CreateAdminFatigueAlert(CurrentDoctorId);
        }
    }
}