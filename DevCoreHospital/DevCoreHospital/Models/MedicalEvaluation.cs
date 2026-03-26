using System;

namespace DevCoreHospital.Models
{
    public class MedicalEvaluation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PatientId { get; set; } = string.Empty;
        public string DiagnosisResult { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string MedsList { get; set; } = string.Empty;

        // Unified property for both Notes and Justifications
        public string Notes { get; set; } = string.Empty;

        public DateTime EvaluationDate { get; set; }
        public Doctor? Evaluator { get; set; }

        public string FormattedDate => EvaluationDate.ToString("dd MMM yyyy");
        public string FormattedTime => EvaluationDate.ToString("HH:mm");
    }
}