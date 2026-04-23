using System;
using DevCoreHospital.Models;
using DevCoreHospital.ViewModels.Pharmacy;

namespace DevCoreHospital.Tests.ViewModels
{
    public class PharmacyShiftItemViewModelTests
    {
        private static readonly DateTime DefaultStart = new DateTime(2025, 6, 15, 8, 0, 0);
        private static readonly DateTime DefaultEnd = new DateTime(2025, 6, 15, 16, 30, 0);

        private static PharmacyShiftItemViewModel BuildViewModel(
            string location = "Ward A",
            DateTime? start = null,
            DateTime? end = null,
            ShiftStatus status = ShiftStatus.SCHEDULED)
        {
            var staff = new Doctor(1, "First", "Last", string.Empty, string.Empty, true, "General", "LIC-1", DoctorStatus.AVAILABLE, 1);
            var shift = new Shift(1, staff, location, start ?? DefaultStart, end ?? DefaultEnd, status);
            return new PharmacyShiftItemViewModel(shift);
        }

        [Fact]
        public void Constructor_SetsRotationAssignment_FromShiftLocation()
        {
            var vm = BuildViewModel(location: "ICU");

            Assert.Equal("ICU", vm.RotationAssignment);
        }

        [Fact]
        public void Constructor_SetsShiftStartTime_FromShiftStartTime()
        {
            var vm = BuildViewModel();

            Assert.Equal(DefaultStart, vm.ShiftStartTime);
        }

        [Fact]
        public void Constructor_SetsShiftEndTime_FromShiftEndTime()
        {
            var vm = BuildViewModel();

            Assert.Equal(DefaultEnd, vm.ShiftEndTime);
        }

        [Fact]
        public void ShiftStartTimeText_ReturnsHourMinuteFormat()
        {
            var vm = BuildViewModel(start: new DateTime(2025, 6, 15, 9, 30, 0));

            Assert.Equal("09:30", vm.ShiftStartTimeText);
        }

        [Fact]
        public void ShiftEndTimeText_ReturnsHourMinuteFormat_WhenEndTimeIsSet()
        {
            var vm = BuildViewModel(end: new DateTime(2025, 6, 15, 17, 45, 0));

            Assert.Equal("17:45", vm.ShiftEndTimeText);
        }

        [Fact]
        public void DayLabel_ReturnsEnglishFormattedDate()
        {
            var vm = BuildViewModel(start: new DateTime(2025, 6, 15, 8, 0, 0));

            Assert.Equal("Sun, 15 Jun 2025", vm.DayLabel);
        }

        [Fact]
        public void DurationText_ReturnsCorrectHoursAndMinutes()
        {
            var vm = BuildViewModel(
                start: new DateTime(2025, 6, 15, 8, 0, 0),
                end: new DateTime(2025, 6, 15, 10, 30, 0));

            Assert.Equal("2h 30m", vm.DurationText);
        }

        [Fact]
        public void DurationText_ReturnsZeroHours_WhenShiftIsUnderOneHour()
        {
            var vm = BuildViewModel(
                start: new DateTime(2025, 6, 15, 8, 0, 0),
                end: new DateTime(2025, 6, 15, 8, 45, 0));

            Assert.Equal("0h 45m", vm.DurationText);
        }

        [Fact]
        public void DurationText_ReturnsEightHours_ForStandardShift()
        {
            var vm = BuildViewModel(
                start: new DateTime(2025, 6, 15, 8, 0, 0),
                end: new DateTime(2025, 6, 15, 16, 0, 0));

            Assert.Equal("8h 0m", vm.DurationText);
        }

        [Fact]
        public void StatusDisplay_ReturnsScheduled_WhenStatusIsScheduled()
        {
            var vm = BuildViewModel(status: ShiftStatus.SCHEDULED);

            Assert.Equal("Scheduled", vm.StatusDisplay);
        }

        [Fact]
        public void StatusDisplay_ReturnsActive_WhenStatusIsActive()
        {
            var vm = BuildViewModel(status: ShiftStatus.ACTIVE);

            Assert.Equal("Active", vm.StatusDisplay);
        }

        [Fact]
        public void StatusDisplay_ReturnsCompleted_WhenStatusIsCompleted()
        {
            var vm = BuildViewModel(status: ShiftStatus.COMPLETED);

            Assert.Equal("Completed", vm.StatusDisplay);
        }

        [Fact]
        public void StatusDisplay_ReturnsCancelled_WhenStatusIsCancelled()
        {
            var vm = BuildViewModel(status: ShiftStatus.CANCELLED);

            Assert.Equal("Cancelled", vm.StatusDisplay);
        }

        [Fact]
        public void TimeRangeDetail_ContainsStartTimeText()
        {
            var vm = BuildViewModel(
                start: new DateTime(2025, 6, 15, 8, 0, 0),
                end: new DateTime(2025, 6, 15, 16, 0, 0));

            Assert.Contains("08:00", vm.TimeRangeDetail);
        }

        [Fact]
        public void TimeRangeDetail_ContainsEndTimeText()
        {
            var vm = BuildViewModel(
                start: new DateTime(2025, 6, 15, 8, 0, 0),
                end: new DateTime(2025, 6, 15, 16, 0, 0));

            Assert.Contains("16:00", vm.TimeRangeDetail);
        }

        [Fact]
        public void TimeRangeDetail_ContainsDurationText()
        {
            var vm = BuildViewModel(
                start: new DateTime(2025, 6, 15, 8, 0, 0),
                end: new DateTime(2025, 6, 15, 16, 0, 0));

            Assert.Contains("8h 0m", vm.TimeRangeDetail);
        }
    }
}
