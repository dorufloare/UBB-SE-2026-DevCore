using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

// Toolkit namespaces
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DevCoreHospital.Models;
using DevCoreHospital.Repositories; // Points to your new Repository
using DevCoreHospital.Configuration; // For AppSettings
using DevCoreHospital.ViewModels.Base;

namespace DevCoreHospital.ViewModels
{
    public partial class MedicalEvaluationViewModel : ObservableObject
    {
        // 1. Switch from DataService to Repository
        private readonly EvaluationsRepository _repository = new();

        private List<MedicalEvaluation> _allRecords = new List<MedicalEvaluation>();

        // 1. Using [ObservableProperty] - The toolkit creates the Public version (e.g. Symptoms) automatically.
        [ObservableProperty] private string _patientId = string.Empty;
        [ObservableProperty] private string _symptoms = string.Empty;
        [ObservableProperty] private string _medsList = string.Empty;
        [ObservableProperty] private string _doctorNotes = string.Empty;
        [ObservableProperty] private string _validationError = string.Empty;
        [ObservableProperty] private string _conflictWarning = string.Empty;
        [ObservableProperty] private bool _isConflictVisible;
        [ObservableProperty] private bool _isRiskAssumed;
        [ObservableProperty] private bool _isFatigued;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _searchText = string.Empty;

        public ObservableCollection<MedicalEvaluation> PastEvaluations { get; } = new();

        // 2. Dynamic Patient ID (No longer a constant!)
        private string _patientId = string.Empty;
        public string PatientId
        {
            get => _patientId;
            set => SetProperty(ref _patientId, value);
        }

        private MedicalEvaluation? _selectedEvaluation;
        public MedicalEvaluation? SelectedEvaluation
        {
            get => _selectedEvaluation;
            set
            {
                if (SetProperty(ref _selectedEvaluation, value))
                {
                    if (value != null)
                    {
                        Symptoms = value.Symptoms;
                        MedsList = value.MedsList;
                        DoctorNotes = value.Notes;
                    }
                    else
                    {
                        ResetForm();
                    }
                    OnPropertyChanged(nameof(IsEditing));
                    SaveDiagnosisCommand.NotifyCanExecuteChanged();
                    DeleteEvaluationCommand.NotifyCanExecuteChanged();
                }
            }
        }

        // 2. Computed Properties
        public bool IsEditing => SelectedEvaluation != null;
        public bool IsFormEnabled => !IsFatigued;
        public Visibility LockoutVisibility => IsFatigued ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ConflictVisibility => IsConflictVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EmptyStateVisibility => (!IsLoading && PastEvaluations.Count == 0) ? Visibility.Visible : Visibility.Collapsed;

        public Brush NotesBackground => IsConflictVisible
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(100, 255, 255, 0))
            : new SolidColorBrush(Microsoft.UI.Colors.Transparent);

        // 3. Use standard IRelayCommand from the Toolkit
        public IRelayCommand SaveDiagnosisCommand { get; }
        public IRelayCommand DeleteEvaluationCommand { get; }

        public MedicalEvaluationViewModel()
        {
            // Use standard RelayCommand (ensure you deleted any local 'RelayCommand.cs' file)
            SaveDiagnosisCommand = new RelayCommand(SaveDiagnosis, CanSaveDiagnosis);
            DeleteEvaluationCommand = new RelayCommand(ExecuteDeletion, () => IsEditing);

            InitializeSession();
        }

        public bool IsEmptyStateVisible => !IsLoading && PastEvaluations.Count == 0;
        public Visibility EmptyStateVisibility => IsEmptyStateVisible ? Visibility.Visible : Visibility.Collapsed;

        public RelayCommand SaveDiagnosisCommand { get; }
        public RelayCommand DeleteEvaluationCommand { get; } // Added back

        public MedicalEvaluationViewModel()
        {
            SaveDiagnosisCommand = new RelayCommand(SaveDiagnosis, CanSaveDiagnosis);
            DeleteEvaluationCommand = new RelayCommand(ExecuteDeletion, () => IsEditing);

            InitializeSession();
        }

        private void InitializeSession()
        {
            // Task: Fetch actual active patient from SQL
            PatientId = _repository.GetActivePatientId(AppSettings.DefaultDoctorId);
            PopulateHistory();
            CheckDoctorFatigue();
        }

        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(currentMeds)) { IsConflictVisible = false; return; }

            // Task 12: Check database for high-risk medicine warnings
            string warning = _repository.GetHighRiskMedicineWarning(currentMeds);
            if (!string.IsNullOrEmpty(warning))
            {
                ConflictWarning = warning;
                IsConflictVisible = true;
            }
            else
            {
                IsConflictVisible = false;
            }
        }

        private bool CanSaveDiagnosis()
        {
            if (IsFatigued) return false;
            if (string.IsNullOrWhiteSpace(Symptoms) || string.IsNullOrWhiteSpace(DoctorNotes))
            {
                ValidationError = "⚠️ Symptoms and Doctor Notes are required.";
                return false;
            }
            if (IsConflictVisible && !IsRiskAssumed)
            {
                ValidationError = "⚠️ You must acknowledge the clinical risk.";
                return false;
            }
            ValidationError = string.Empty;
            return true;
        }

        private void SaveDiagnosis()
        {
            if (IsEditing && SelectedEvaluation != null)
            {
                _repository.UpdateEvaluationNotes(SelectedEvaluation.EvaluationID, this.DoctorNotes);
                SelectedEvaluation = null;
            }
            else
            {
                var newRecord = new MedicalEvaluation
                {
                    PatientId = this.PatientId, // Use dynamic ID
                    Symptoms = IsConflictVisible && IsRiskAssumed ? $"⚠️ [RISK] - {Symptoms}" : Symptoms,
                    MedsList = this.MedsList,
                    Notes = this.DoctorNotes,
                    EvaluationDate = DateTime.Now,
                    Evaluator = new DevCoreHospital.Models.Doctor { StaffID = AppSettings.DefaultDoctorId }
                };

                _repository.SaveEvaluation(newRecord);
            }

            ResetForm();
            PopulateHistory();
        }

        public void ResetForm()
        {
            Symptoms = string.Empty;
            MedsList = string.Empty;
            DoctorNotes = string.Empty;
            IsRiskAssumed = false;
            IsConflictVisible = false;
            SelectedEvaluation = null;

            RaisePropertyChanged(nameof(Symptoms));
            RaisePropertyChanged(nameof(MedsList));
            RaisePropertyChanged(nameof(DoctorNotes));
            RaisePropertyChanged(nameof(IsRiskAssumed));
            RaisePropertyChanged(nameof(IsConflictVisible));
            RaisePropertyChanged(nameof(ConflictVisibility));
            RaisePropertyChanged(nameof(NotesBackground));
            RaisePropertyChanged(nameof(SelectedEvaluation));
            RaisePropertyChanged(nameof(IsEditing));

            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            SaveDiagnosisCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(ValidationError));
        }

        public async void PopulateHistory()
        {
            IsLoading = true;
            PastEvaluations.Clear();
            await Task.Delay(800);

            // Pull real history from SQL
            _allRecords = _repository.GetEvaluationsByDoctor(AppSettings.DefaultDoctorId.ToString());

            ApplyFilter();
            IsLoading = false;
            OnPropertyChanged(nameof(EmptyStateVisibility));
        }

        private void CheckDoctorFatigue()
        {
            // Task 33: Check SQL for total duty hours
            double fatigueHours = _repository.GetDoctorFatigueHours(AppSettings.DefaultDoctorId.ToString());
            IsFatigued = fatigueHours >= 12.0;
        }

        public void ExecuteDeletion()
        {
            if (SelectedEvaluation == null) return;
            _repository.DeleteEvaluation(SelectedEvaluation.EvaluationID);
            ResetForm();
            PopulateHistory();
        }
    }
}