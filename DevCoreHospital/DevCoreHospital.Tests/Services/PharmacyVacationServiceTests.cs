using System;
using System.Collections.Generic;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using Moq;

namespace DevCoreHospital.Tests.Services
{
    public class PharmacyVacationServiceTests
    {
        private readonly Mock<IPharmacyStaffRepository> mockStaffRepository;
        private readonly Mock<IPharmacyShiftRepository> mockShiftRepository;
        private readonly PharmacyVacationService service;

        private readonly Pharmacyst pharmacist = new Pharmacyst(1, "Ana", "Pop", string.Empty, true, "General", 3);

        public PharmacyVacationServiceTests()
        {
            mockStaffRepository = new Mock<IPharmacyStaffRepository>();
            mockShiftRepository = new Mock<IPharmacyShiftRepository>();
            service = new PharmacyVacationService(mockStaffRepository.Object, mockShiftRepository.Object);
        }

        // ── RegisterVacation ───────────────────────────────────────────────────

        [Fact]
        public void RegisterVacation_ThrowsArgumentException_WhenEndDateIsBeforeStartDate()
        {
            // Arrange
            var startDate = new DateTime(2025, 6, 10);
            var endDate = new DateTime(2025, 6, 5);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                service.RegisterVacation(pharmacist.StaffID, startDate, endDate));

            Assert.Equal("End date must be on or after start date.", exception.Message);
        }

        [Fact]
        public void RegisterVacation_ThrowsArgumentException_WhenPharmacistNotFound()
        {
            // Arrange
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst>());

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                service.RegisterVacation(99, new DateTime(2025, 6, 1), new DateTime(2025, 6, 3)));

            Assert.Equal("Pharmacist not found.", exception.Message);
        }

        [Fact]
        public void RegisterVacation_ThrowsInvalidOperationException_WhenVacationOverlapsExistingShift()
        {
            // Arrange
            var existingShift = new Shift(10, pharmacist, "Ward A",
                new DateTime(2025, 6, 8), new DateTime(2025, 6, 12), ShiftStatus.SCHEDULED);

            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetShiftsByStaffID(pharmacist.StaffID)).Returns(new List<Shift> { existingShift });

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                service.RegisterVacation(pharmacist.StaffID, new DateTime(2025, 6, 10), new DateTime(2025, 6, 15)));

            Assert.Equal("Cannot add vacation: this period overlaps an existing shift.", exception.Message);
        }

        [Fact]
        public void RegisterVacation_ThrowsInvalidOperationException_WhenVacationWouldExceedMonthlyLimit()
        {
            // Arrange — 3 existing vacation days in June, adding 2 more would make 5 (> 4 limit)
            var existingVacation = new Shift(10, pharmacist, "Vacation",
                new DateTime(2025, 6, 1), new DateTime(2025, 6, 4), ShiftStatus.VACATION);

            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetShiftsByStaffID(pharmacist.StaffID)).Returns(new List<Shift> { existingVacation });

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                service.RegisterVacation(pharmacist.StaffID, new DateTime(2025, 6, 20), new DateTime(2025, 6, 21)));

            Assert.Equal("Cannot add vacation: pharmacist would exceed 4 vacation days in a month.", exception.Message);
        }

        [Fact]
        public void RegisterVacation_AddsVacationShift_WhenAllConditionsAreMet()
        {
            // Arrange
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetShiftsByStaffID(pharmacist.StaffID)).Returns(new List<Shift>());
            mockShiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift>());

            // Act
            service.RegisterVacation(pharmacist.StaffID, new DateTime(2025, 7, 1), new DateTime(2025, 7, 3));

            // Assert
            mockShiftRepository.Verify(r => r.AddShift(It.IsAny<Shift>()), Times.Once);
        }

        [Fact]
        public void RegisterVacation_AddsShiftWithVacationStatus_WhenAllConditionsAreMet()
        {
            // Arrange
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetShiftsByStaffID(pharmacist.StaffID)).Returns(new List<Shift>());
            mockShiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift>());

            // Act
            service.RegisterVacation(pharmacist.StaffID, new DateTime(2025, 7, 1), new DateTime(2025, 7, 3));

            // Assert
            mockShiftRepository.Verify(r => r.AddShift(
                It.Is<Shift>(shift => shift.Status == ShiftStatus.VACATION)), Times.Once);
        }

        [Fact]
        public void RegisterVacation_AllowsVacation_WhenExactlyAtMonthlyLimit()
        {
            // Arrange — adding exactly 4 days in a month with no prior vacation should succeed
            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetShiftsByStaffID(pharmacist.StaffID)).Returns(new List<Shift>());
            mockShiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift>());

            // Act — June 1–4 inclusive = 4 days
            service.RegisterVacation(pharmacist.StaffID, new DateTime(2025, 6, 1), new DateTime(2025, 6, 4));

            // Assert
            mockShiftRepository.Verify(r => r.AddShift(It.IsAny<Shift>()), Times.Once);
        }

        [Fact]
        public void RegisterVacation_ThrowsInvalidOperationException_WhenNewVacationExceedsLimitAcrossMonths()
        {
            // Arrange — 3 existing vacation days in July, adding 2 more crosses the limit in July
            var existingVacation = new Shift(10, pharmacist, "Vacation",
                new DateTime(2025, 7, 1), new DateTime(2025, 7, 4), ShiftStatus.VACATION);

            mockStaffRepository.Setup(r => r.GetPharmacists()).Returns(new List<Pharmacyst> { pharmacist });
            mockShiftRepository.Setup(r => r.GetShiftsByStaffID(pharmacist.StaffID)).Returns(new List<Shift> { existingVacation });

            // Act & Assert — July 28–31 adds 4 days to July: total July = 7 days > 4
            Assert.Throws<InvalidOperationException>(() =>
                service.RegisterVacation(pharmacist.StaffID, new DateTime(2025, 7, 28), new DateTime(2025, 7, 31)));
        }
    }
}
