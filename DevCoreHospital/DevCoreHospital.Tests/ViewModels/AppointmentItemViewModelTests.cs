using System;
using DevCoreHospital.Models;
using DevCoreHospital.ViewModels.Doctor;

namespace DevCoreHospital.Tests.ViewModels
{
    public class AppointmentItemViewModelTests
    {
        private static Appointment BuildAppointment(
            int id = 1,
            string patientName = "Jane Doe",
            string doctorName = "Dr. Smith",
            string location = "Room 5",
            string status = "Scheduled",
            string type = "Checkup",
            string? notes = null,
            DateTime? date = null,
            TimeSpan? startTime = null,
            TimeSpan? endTime = null)
            => new Appointment
            {
                Id = id,
                PatientName = patientName,
                DoctorId = 10,
                DoctorName = doctorName,
                Location = location,
                Status = status,
                Type = type,
                Notes = notes ?? string.Empty,
                Date = date ?? new DateTime(2025, 6, 15),
                StartTime = startTime ?? new TimeSpan(9, 0, 0),
                EndTime = endTime ?? new TimeSpan(10, 0, 0),
            };

        [Fact]
        public void DateText_ReturnsFormattedDate()
        {
            var date = new DateTime(2025, 6, 15);
            var viewModel = new AppointmentItemViewModel(BuildAppointment(date: date));

            Assert.Equal(date.ToString("dd MMM yyyy"), viewModel.DateText);
        }

        [Fact]
        public void TimeRangeText_ContainsStartAndEndTime()
        {
            var viewModel = new AppointmentItemViewModel(BuildAppointment(
                startTime: new TimeSpan(9, 30, 0),
                endTime: new TimeSpan(10, 15, 0)));

            Assert.Contains("09:30", viewModel.TimeRangeText);
            Assert.Contains("10:15", viewModel.TimeRangeText);
        }

        [Fact]
        public void LocationSafe_ReturnsLocation_WhenLocationIsSet()
        {
            var viewModel = new AppointmentItemViewModel(BuildAppointment(location: "Room 5"));

            Assert.Equal("Room 5", viewModel.LocationSafe);
        }

        [Fact]
        public void LocationSafe_ReturnsLocationTbd_WhenLocationIsEmpty()
        {
            var viewModel = new AppointmentItemViewModel(BuildAppointment(location: string.Empty));

            Assert.Equal("Location TBD", viewModel.LocationSafe);
        }

        [Fact]
        public void LocationSafe_ReturnsLocationTbd_WhenLocationIsWhitespace()
        {
            var viewModel = new AppointmentItemViewModel(BuildAppointment(location: "   "));

            Assert.Equal("Location TBD", viewModel.LocationSafe);
        }

        [Fact]
        public void ToAppointment_ReturnsAppointmentWithSameId()
        {
            var viewModel = new AppointmentItemViewModel(BuildAppointment(id: 42));

            var result = viewModel.ToAppointment();

            Assert.Equal(42, result.Id);
        }

        private static Appointment BuildFullAppointment() => BuildAppointment(
            id: 5, patientName: "Alice", doctorName: "Dr. Brown",
            location: "Ward B", status: "Scheduled", type: "Follow-up",
            notes: "Bring X-ray",
            date: new DateTime(2025, 7, 10),
            startTime: new TimeSpan(9, 0, 0),
            endTime: new TimeSpan(11, 30, 0));

        [Fact]
        public void ToAppointment_WhenBuilt_PreservesPatientName()
        {
            var result = new AppointmentItemViewModel(BuildFullAppointment()).ToAppointment();
            Assert.Equal("Alice", result.PatientName);
        }

        [Fact]
        public void ToAppointment_WhenBuilt_PreservesDoctorName()
        {
            var result = new AppointmentItemViewModel(BuildFullAppointment()).ToAppointment();
            Assert.Equal("Dr. Brown", result.DoctorName);
        }

        [Fact]
        public void ToAppointment_WhenBuilt_PreservesLocation()
        {
            var result = new AppointmentItemViewModel(BuildFullAppointment()).ToAppointment();
            Assert.Equal("Ward B", result.Location);
        }

        [Fact]
        public void ToAppointment_WhenBuilt_PreservesStatus()
        {
            var result = new AppointmentItemViewModel(BuildFullAppointment()).ToAppointment();
            Assert.Equal("Scheduled", result.Status);
        }

        [Fact]
        public void ToAppointment_WhenBuilt_PreservesType()
        {
            var result = new AppointmentItemViewModel(BuildFullAppointment()).ToAppointment();
            Assert.Equal("Follow-up", result.Type);
        }

        [Fact]
        public void ToAppointment_WhenBuilt_PreservesNotes()
        {
            var result = new AppointmentItemViewModel(BuildFullAppointment()).ToAppointment();
            Assert.Equal("Bring X-ray", result.Notes);
        }

        [Fact]
        public void ToAppointment_WhenBuilt_PreservesDate()
        {
            var result = new AppointmentItemViewModel(BuildFullAppointment()).ToAppointment();
            Assert.Equal(new DateTime(2025, 7, 10), result.Date);
        }

        [Fact]
        public void ToAppointment_WhenBuilt_PreservesStartTime()
        {
            var result = new AppointmentItemViewModel(BuildFullAppointment()).ToAppointment();
            Assert.Equal(new TimeSpan(9, 0, 0), result.StartTime);
        }

        [Fact]
        public void ToAppointment_WhenBuilt_PreservesEndTime()
        {
            var result = new AppointmentItemViewModel(BuildFullAppointment()).ToAppointment();
            Assert.Equal(new TimeSpan(11, 30, 0), result.EndTime);
        }

        private static Appointment BuildAllNullStringsAppointment() => new Appointment
        {
            Id = 1,
            PatientName = null!,
            DoctorName = null!,
            Location = null!,
            Notes = null!,
            Type = null!,
            Status = null!,
            Date = DateTime.Now,
        };

        [Fact]
        public void Constructor_WhenPatientNameIsNull_NormalizesToEmptyString()
        {
            var viewModel = new AppointmentItemViewModel(BuildAllNullStringsAppointment());
            Assert.Equal(string.Empty, viewModel.PatientName);
        }

        [Fact]
        public void Constructor_WhenDoctorNameIsNull_NormalizesToEmptyString()
        {
            var viewModel = new AppointmentItemViewModel(BuildAllNullStringsAppointment());
            Assert.Equal(string.Empty, viewModel.DoctorName);
        }

        [Fact]
        public void Constructor_WhenNotesIsNull_NormalizesToEmptyString()
        {
            var viewModel = new AppointmentItemViewModel(BuildAllNullStringsAppointment());
            Assert.Equal(string.Empty, viewModel.Notes);
        }
    }
}
