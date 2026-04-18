using System.Collections.Generic;
using System.Linq;
using System;
using DevCoreHospital.Data;
using DevCoreHospital.Models;
using Microsoft.IdentityModel.Tokens;

namespace DevCoreHospital.Repositories
{
    public class StaffRepository
    {
        private List<IStaff> staffList;
        private readonly DatabaseManager dbManager;

        public StaffRepository(DatabaseManager dbManager)
        {
            staffList = new List<IStaff>();
            this.dbManager = dbManager;
            LoadStaff();
        }

        public void LoadStaff()
        {
            staffList = this.dbManager.GetStaff();
        }

        public List<IStaff> LoadAllStaff()
        {
            return dbManager.GetStaff();
        }

        public void SaveStaffChanges()
        {
            dbManager.SaveStaff(staffList);
        }

        public IStaff? GetStaffById(int staffId)
        {
            return dbManager.GetStaff().FirstOrDefault(s => s.StaffID == staffId);
        }

        public List<Doctor> GetAvailableDoctors()
        {
            return dbManager.GetStaff().OfType<Doctor>().Where(doctor => doctor.Available).ToList();
        }

        private List<Pharmacyst> GetAvailablePharmacists()
        {
            return dbManager.GetStaff().OfType<Pharmacyst>().Where(ph => ph.Available).ToList();
        }

        public List<Pharmacyst> GetPharmacists()
        {
            return dbManager.GetStaff().OfType<Pharmacyst>().ToList();
        }

        private static string Normalize(string? value)
            => (value ?? string.Empty).Trim().ToLowerInvariant();

        public List<IStaff> GetPotentialSwapColleagues(IStaff requester)
        {
            // Always fresh from DB
            var all = dbManager.GetStaff();
            var req = all.FirstOrDefault(s => s.StaffID == requester.StaffID);
            if (req == null)
            {
                return new List<IStaff>();
            }

            if (req is Doctor reqDoctor)
            {
                var reqSpec = Normalize(reqDoctor.Specialization);

                // IMPORTANT: removed Available==true filter for swap candidates
                return all
                    .OfType<Doctor>()
                    .Where(d =>
                        d.StaffID != reqDoctor.StaffID &&
                        !string.IsNullOrWhiteSpace(d.Specialization) &&
                        Normalize(d.Specialization) == reqSpec)
                    .Cast<IStaff>()
                    .ToList();
            }

            if (req is Pharmacyst reqPharmacyst)
            {
                var reqCert = Normalize(reqPharmacyst.Certification);

                // IMPORTANT: removed Available==true filter for swap candidates
                return all
                    .OfType<Pharmacyst>()
                    .Where(p =>
                        p.StaffID != reqPharmacyst.StaffID &&
                        !string.IsNullOrWhiteSpace(p.Certification) &&
                        Normalize(p.Certification) == reqCert)
                    .Cast<IStaff>()
                    .ToList();
            }

            return new List<IStaff>();
        }

        public List<IStaff> GetAvailableStaff(string doctorSpecialization, string pharmacystCertification)
        {
            var availableDoctors = GetAvailableDoctors();
            var availablePharmacists = GetAvailablePharmacists();
            var availableStaff = new List<IStaff>();

            if (!string.IsNullOrEmpty(doctorSpecialization) && !string.IsNullOrEmpty(pharmacystCertification))
            {
                var filteredDoctors = availableDoctors.Where(doctor => doctor.Specialization.Equals(doctorSpecialization, StringComparison.OrdinalIgnoreCase));
                var filteredPharmacists = availablePharmacists.Where(ph => ph.Certification.Equals(pharmacystCertification, StringComparison.OrdinalIgnoreCase));
                availableStaff.AddRange(filteredDoctors);
                availableStaff.AddRange(filteredPharmacists);
            }
            else if (!doctorSpecialization.IsNullOrEmpty())
            {
                var filteredDoctors = availableDoctors.Where(doctor => doctor.Specialization.Equals(doctorSpecialization, StringComparison.OrdinalIgnoreCase));
                availableStaff.AddRange(filteredDoctors);
            }
            else if (!pharmacystCertification.IsNullOrEmpty())
            {
                var filteredPharmacists = availablePharmacists.Where(ph => ph.Certification.Equals(pharmacystCertification, StringComparison.OrdinalIgnoreCase));
                availableStaff.AddRange(filteredPharmacists);
            }
            else
            {
                availableStaff.AddRange(availableDoctors);
                availableStaff.AddRange(availablePharmacists);
            }

            return availableStaff;
        }

        public List<Doctor> GetDoctorsBySpecialization(string specialization)
        {
            return dbManager.GetStaff().OfType<Doctor>()
                .Where(doctor => doctor.Specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Pharmacyst> GetPharmacystsByCertification(string certification)
        {
            return dbManager.GetStaff().OfType<Pharmacyst>()
                .Where(ph => ph.Certification.Equals(certification, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public void UpdateStaffAvailability(int staffId, bool isAvailable, DoctorStatus status = DoctorStatus.OFF_DUTY)
        {
            var staff = staffList.FirstOrDefault(st => st.StaffID == staffId);
            if (staff != null)
            {
                staff.Available = isAvailable;
                if (staff is Doctor doc)
                {
                    doc.DoctorStatus = status;
                }

                dbManager.UpdateStaff(staff);
            }
        }
    }
}