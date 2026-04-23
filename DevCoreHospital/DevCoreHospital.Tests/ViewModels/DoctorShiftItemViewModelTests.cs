using System;
using DevCoreHospital.Models;
using DevCoreHospital.ViewModels.Doctor;

namespace DevCoreHospital.Tests.ViewModels
{
    public class DoctorShiftItemViewModelTests
    {
        private static readonly DateTime DefaultStart = new DateTime(2025, 6, 15, 9, 30, 0);
        private static readonly DateTime DefaultEnd = new DateTime(2025, 6, 15, 17, 45, 0);

        private static DoctorShiftItemViewModel BuildViewModel(
            string location = "Ward A",
            DateTime? start = null,
            DateTime? end = null,
            ShiftStatus status = ShiftStatus.SCHEDULED)
        {
            var staff = new Doctor(1, "First", "Last", string.Empty, string.Empty, true, "General", "LIC-1", DoctorStatus.AVAILABLE, 1);
            var shift = new Shift(1, staff, location, start ?? DefaultStart, end ?? DefaultEnd, status);
            return new DoctorShiftItemViewModel(shift);
        }

        [Fact]
        public void DateText_ReturnsFormattedDate()
        {
            var vm = BuildViewModel(start: new DateTime(2025, 6, 15, 9, 0, 0));

            Assert.Equal("15 Jun 2025", vm.DateText);
        }

        [Fact]
        public void TimeRangeText_ContainsStartAndEndTime()
        {
            var vm = BuildViewModel(
                start: new DateTime(2025, 6, 15, 9, 0, 0),
                end: new DateTime(2025, 6, 15, 17, 0, 0));

            Assert.Contains("09:00", vm.TimeRangeText);
            Assert.Contains("17:00", vm.TimeRangeText);
        }

        [Fact]
        public void LocationText_ReturnsLocation_WhenLocationIsSet()
        {
            var vm = BuildViewModel(location: "ICU");

            Assert.Equal("ICU", vm.LocationText);
        }

        [Fact]
        public void LocationText_ReturnsLocationTbd_WhenLocationIsEmpty()
        {
            var vm = BuildViewModel(location: string.Empty);

            Assert.Equal("Location TBD", vm.LocationText);
        }

        [Fact]
        public void LocationText_ReturnsLocationTbd_WhenLocationIsWhitespace()
        {
            var vm = BuildViewModel(location: "   ");

            Assert.Equal("Location TBD", vm.LocationText);
        }

        [Fact]
        public void Status_ReturnsScheduled_WhenStatusIsScheduled()
        {
            var vm = BuildViewModel(status: ShiftStatus.SCHEDULED);

            Assert.Equal("SCHEDULED", vm.Status);
        }

        [Fact]
        public void Status_ReturnsActive_WhenStatusIsActive()
        {
            var vm = BuildViewModel(status: ShiftStatus.ACTIVE);

            Assert.Equal("ACTIVE", vm.Status);
        }

        [Fact]
        public void Constructor_SetsId_FromShift()
        {
            var staff = new Doctor(1, "F", "L", string.Empty, string.Empty, true, "G", "L", DoctorStatus.AVAILABLE, 1);
            var shift = new Shift(99, staff, "ER", DefaultStart, DefaultEnd, ShiftStatus.SCHEDULED);

            var vm = new DoctorShiftItemViewModel(shift);

            Assert.Equal(99, vm.Id);
        }

        [Fact]
        public void Constructor_SetsStartAndEndTime_FromShift()
        {
            var vm = BuildViewModel(start: DefaultStart, end: DefaultEnd);

            Assert.Equal(DefaultStart, vm.StartTime);
            Assert.Equal(DefaultEnd, vm.EndTime);
        }
    }
}
