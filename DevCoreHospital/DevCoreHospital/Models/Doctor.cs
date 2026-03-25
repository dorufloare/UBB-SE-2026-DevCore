namespace DevCoreHospital.Models
{
    public class Doctor : Staff
    {
        private int staffID { get; set; }
        private string firstName { get; set; }
        public string lastName { get; set; }
        public string contactInfo { get; set; }
        public bool available { get; set; }
        public string specialization { get; set; }
        public string licenseNumber { get; set; }
        public DoctorStatus doctorStatus { get; set; }

        public Doctor(int staffID, string firstName, string lastName, string contactInfo, bool available,
            string specialization, string licenseNumber, DoctorStatus doctorStatus)
        {
            this.staffID = staffID;
            this.firstName = firstName;
            this.lastName = lastName;
            this.contactInfo = contactInfo;
            this.available = available;
            this.specialization = specialization;
            this.licenseNumber = licenseNumber;
            this.doctorStatus = doctorStatus;
        }

        public void UpdateAvailability(bool newAvailability)
        {
            this.available = newAvailability;
        }
    }
}