using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevCoreHospital.Models;
using DevCoreHospital.Data;
using Microsoft.IdentityModel.Tokens;

namespace DevCoreHospital.Repositories
{
    public class StaffRepository
    {
        private List<IStaff> _staffList;
        private DatabaseManager _dbManager;

        public StaffRepository(DatabaseManager dbManager)
        {
            this._staffList = new List<IStaff>();
            this._dbManager = dbManager;
        }
        public void LoadStaff()
        {
            _staffList = _dbManager.GetStaff();
        }

        public void SaveStaffChanges()
        {
            // Here you would add code to save the staff list to the database
            // For now, we will just assume it's saved
        }

        public List<Doctor> GetAvailableDoctors()
        {
            var availableDoctors = _dbManager.GetStaff().OfType<Doctor>().Where(doctor => doctor.Available).ToList();
            return availableDoctors;
        }
        private List<Pharmacyst> GetAvailablePharmacists()
        {
            var availablePharmacists = _dbManager.GetStaff().OfType<Pharmacyst>().Where(ph => ph.Available).ToList();
            return availablePharmacists;
        }
        public List<IStaff> GetAvailableStaff(string doctorSpecialization, string pharmacystCertification)
        {
            var availableDoctors = GetAvailableDoctors();
            var availablePharmacists = GetAvailablePharmacists();
            var availableStaff = new List<IStaff>();

            if (!string.IsNullOrEmpty(doctorSpecialization) && !string.IsNullOrEmpty(pharmacystCertification)) // in this case, we need doctors & pharmacysts
            {
                var filteredDoctors = availableDoctors.Where(doctor => doctor.Specialization.Equals(doctorSpecialization, StringComparison.OrdinalIgnoreCase));
                var filteredPharmacists = availablePharmacists.Where(ph => ph.Certification.Equals(pharmacystCertification, StringComparison.OrdinalIgnoreCase));
                availableStaff.AddRange(filteredDoctors);
                availableStaff.AddRange(filteredPharmacists);
            } else if (!doctorSpecialization.IsNullOrEmpty()) // in this case, we need only doctors
            {
                var filteredDoctors = availableDoctors.Where(doctor => doctor.Specialization.Equals(doctorSpecialization, StringComparison.OrdinalIgnoreCase));
                availableStaff.AddRange(filteredDoctors);
            } else if (!pharmacystCertification.IsNullOrEmpty()) // in this case, we need only pharmacysts
            {
                var filteredPharmacists = availablePharmacists.Where(ph => ph.Certification.Equals(pharmacystCertification, StringComparison.OrdinalIgnoreCase));
                availableStaff.AddRange(filteredPharmacists);
            }
            else // in this case, retrieve all the available doctors & pharmacysts
            {
                availableStaff.AddRange(availableDoctors);
                availableStaff.AddRange(availablePharmacists);
            }
            return availableStaff;
        }

        public void RegisterStaff(IStaff newStaff)
        {
            // Here you would add code to save the new staff member to the database
            // For now, we will just add it to the local list
            _staffList.Add(newStaff);
        }
        public void RemoveStaff(int staffId)
        {
            var staffToRemove = _staffList.FirstOrDefault(staff => staff.StaffID == staffId);
            if (staffToRemove != null)
            {
                // Here you would add code to remove the staff member from the database
                // For now, we will just remove it from the local list
                _staffList.Remove(staffToRemove);
            }
        }
        public List<Doctor> GetDoctorsBySpecialization(string specialization)
        {
            var doctors = _dbManager.GetStaff().OfType<Doctor>().Where(doctor => doctor.Specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase)).ToList();
            return doctors;
        }
        public List<Pharmacyst> GetPharmacystsByCertification(string certification)
        {
            var pharmacysts = _dbManager.GetStaff().OfType<Pharmacyst>().Where(ph => ph.Certification.Equals(certification, StringComparison.OrdinalIgnoreCase)).ToList();
            return pharmacysts;
        }
        public void UpdateStaffAvailability(int staffId, bool isAvailable, DoctorStatus status = DoctorStatus.OFF_DUTY)
        {
            var staff = _staffList.FirstOrDefault(staff => staff.StaffID == staffId);
            if (staff != null)
            {
                staff.Available = isAvailable;
                if (staff is Doctor doc) doc.DoctorStatus = status;
            }
        }
    }
}
