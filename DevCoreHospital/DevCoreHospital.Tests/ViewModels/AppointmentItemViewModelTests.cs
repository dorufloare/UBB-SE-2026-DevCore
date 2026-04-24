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
            var vm = new AppointmentItemViewModel(BuildAppointment(date: new DateTime(2025, 6, 15)));

            Assert.Equal("15 Jun 2025", vm.DateText);
        }

        [Fact]
        public void TimeRangeText_ContainsStartAndEndTime()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(
                startTime: new TimeSpan(9, 30, 0),
                endTime: new TimeSpan(10, 15, 0)));

            Assert.Contains("09:30", vm.TimeRangeText);
            Assert.Contains("10:15", vm.TimeRangeText);
        }

        [Fact]
        public void LocationSafe_ReturnsLocation_WhenLocationIsSet()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(location: "Room 5"));

            Assert.Equal("Room 5", vm.LocationSafe);
        }

        [Fact]
        public void LocationSafe_ReturnsLocationTbd_WhenLocationIsEmpty()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(location: string.Empty));

            Assert.Equal("Location TBD", vm.LocationSafe);
        }

        [Fact]
        public void LocationSafe_ReturnsLocationTbd_WhenLocationIsWhitespace()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(location: "   "));

            Assert.Equal("Location TBD", vm.LocationSafe);
        }

        [Fact]
        public void ToAppointment_ReturnsAppointmentWithSameId()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(id: 42));

            var result = vm.ToAppointment();

            Assert.Equal(42, result.Id);
        }

        [Fact]
        public void ToAppointment_PreservesPatientName()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(patientName: "Alice"));

            Assert.Equal("Alice", vm.ToAppointment().PatientName);
        }

        [Fact]
        public void ToAppointment_PreservesDoctorName()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(doctorName: "Dr. Brown"));

            Assert.Equal("Dr. Brown", vm.ToAppointment().DoctorName);
        }

        [Fact]
        public void ToAppointment_PreservesLocation()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(location: "Ward B"));

            Assert.Equal("Ward B", vm.ToAppointment().Location);
        }

        [Fact]
        public void ToAppointment_PreservesStatus()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(status: "Scheduled"));

            Assert.Equal("Scheduled", vm.ToAppointment().Status);
        }

        [Fact]
        public void ToAppointment_PreservesType()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(type: "Follow-up"));

            Assert.Equal("Follow-up", vm.ToAppointment().Type);
        }

        [Fact]
        public void ToAppointment_PreservesNotes()
        {
            var vm = new AppointmentItemViewModel(BuildAppointment(notes: "Bring X-ray"));

            Assert.Equal("Bring X-ray", vm.ToAppointment().Notes);
        }

        [Fact]
        public void ToAppointment_PreservesDate()
        {
            var date = new DateTime(2025, 7, 10);
            var vm = new AppointmentItemViewModel(BuildAppointment(date: date));

            Assert.Equal(date, vm.ToAppointment().Date);
        }

        [Fact]
        public void ToAppointment_PreservesStartTime()
        {
            var start = new TimeSpan(9, 0, 0);
            var vm = new AppointmentItemViewModel(BuildAppointment(startTime: start));

            Assert.Equal(start, vm.ToAppointment().StartTime);
        }

        [Fact]
        public void ToAppointment_PreservesEndTime()
        {
            var end = new TimeSpan(11, 30, 0);
            var vm = new AppointmentItemViewModel(BuildAppointment(endTime: end));

            Assert.Equal(end, vm.ToAppointment().EndTime);
        }

        [Fact]
        public void Constructor_ReplacesNullPatientName_WithEmptyString()
        {
            var vm = new AppointmentItemViewModel(BuildAppointmentWithNulls());

            Assert.Equal(string.Empty, vm.PatientName);
        }

        [Fact]
        public void Constructor_ReplacesNullDoctorName_WithEmptyString()
        {
            var vm = new AppointmentItemViewModel(BuildAppointmentWithNulls());

            Assert.Equal(string.Empty, vm.DoctorName);
        }

        [Fact]
        public void Constructor_ReplacesNullNotes_WithEmptyString()
        {
            var vm = new AppointmentItemViewModel(BuildAppointmentWithNulls());

            Assert.Equal(string.Empty, vm.Notes);
        }

        private static Appointment BuildAppointmentWithNulls()
            => new Appointment
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
    }
}
