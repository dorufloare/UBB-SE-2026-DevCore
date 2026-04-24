using System;
using System.Collections.Generic;
using System.Linq;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using Moq;
using Xunit;

namespace DevCoreHospital.Tests.Services
{
    public class ShiftManagementServiceTests
    {
        private readonly Mock<IShiftManagementStaffRepository> staffRepository;
        private readonly Mock<IShiftManagementShiftRepository> shiftRepository;
        private readonly ShiftManagementService service;

        public ShiftManagementServiceTests()
        {
            staffRepository = new Mock<IShiftManagementStaffRepository>();
            shiftRepository = new Mock<IShiftManagementShiftRepository>();
            service = new ShiftManagementService(staffRepository.Object, shiftRepository.Object);
        }

        [Fact]
        public void SetShiftActive_WhenShiftExists_UpdatesShiftStatusToActive()
        {
            // Arrange
            var shiftId = 100;
            var doctor = BuildDoctor(10, "Cardiology");
            shiftRepository
                .Setup(repo => repo.GetShifts())
                .Returns(new List<Shift>
                {
                    BuildShift(shiftId, doctor, new DateTime(2026, 4, 21, 8, 0, 0), new DateTime(2026, 4, 21, 16, 0, 0))
                });

            int updateCount = 0;
            int updatedShiftId = -1;
            ShiftStatus updatedStatus = ShiftStatus.CANCELLED;
            shiftRepository
                .Setup(repo => repo.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()))
                .Callback<int, ShiftStatus>((id, status) =>
                {
                    updateCount++;
                    updatedShiftId = id;
                    updatedStatus = status;
                });

            // Act
            service.SetShiftActive(shiftId);

            // Assert
            Assert.Equal(1, updateCount);
            Assert.Equal(shiftId, updatedShiftId);
            Assert.Equal(ShiftStatus.ACTIVE, updatedStatus);
        }

        [Fact]
        public void SetShiftActive_WhenShiftExists_UpdatesStaffAvailabilityToAvailable()
        {
            // Arrange
            var shiftId = 101;
            var doctor = BuildDoctor(11, "Neurology");
            shiftRepository
                .Setup(repo => repo.GetShifts())
                .Returns(new List<Shift>
                {
                    BuildShift(shiftId, doctor, new DateTime(2026, 4, 21, 9, 0, 0), new DateTime(2026, 4, 21, 17, 0, 0))
                });

            int updateCount = 0;
            int updatedStaffId = -1;
            bool updatedAvailability = false;
            DoctorStatus updatedDoctorStatus = DoctorStatus.IN_EXAMINATION;
            staffRepository
                .Setup(repo => repo.UpdateStaffAvailability(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DoctorStatus>()))
                .Callback<int, bool, DoctorStatus>((staffId, isAvailable, status) =>
                {
                    updateCount++;
                    updatedStaffId = staffId;
                    updatedAvailability = isAvailable;
                    updatedDoctorStatus = status;
                });

            // Act
            service.SetShiftActive(shiftId);

            // Assert
            Assert.Equal(1, updateCount);
            Assert.Equal(doctor.StaffID, updatedStaffId);
            Assert.Equal(true, updatedAvailability);
            Assert.Equal(DoctorStatus.AVAILABLE, updatedDoctorStatus);
        }

        [Fact]
        public void SetShiftActive_WhenShiftDoesNotExist_DoesNotUpdateShiftStatus()
        {
            // Arrange
            shiftRepository.Setup(repo => repo.GetShifts()).Returns(new List<Shift>());

            int updateCount = 0;
            shiftRepository
                .Setup(repo => repo.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()))
                .Callback(() => updateCount++);

            // Act
            service.SetShiftActive(999);

            // Assert
            Assert.Equal(0, updateCount);
        }

        [Fact]
        public void SetShiftActive_WhenShiftDoesNotExist_DoesNotUpdateStaffAvailability()
        {
            // Arrange
            shiftRepository.Setup(repo => repo.GetShifts()).Returns(new List<Shift>());

            int updateCount = 0;
            staffRepository
                .Setup(repo => repo.UpdateStaffAvailability(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DoctorStatus>()))
                .Callback(() => updateCount++);

            // Act
            service.SetShiftActive(999);

            // Assert
            Assert.Equal(0, updateCount);
        }

        [Fact]
        public void CancelShift_WhenShiftExists_UpdatesStaffAvailabilityToOffDuty()
        {
            // Arrange
            var shiftId = 200;
            var doctor = BuildDoctor(20, "Oncology");
            shiftRepository
                .Setup(repo => repo.GetShifts())
                .Returns(new List<Shift>
                {
                    BuildShift(shiftId, doctor, new DateTime(2026, 4, 21, 8, 0, 0), new DateTime(2026, 4, 21, 16, 0, 0))
                });

            int updateCount = 0;
            int updatedStaffId = -1;
            bool updatedAvailability = true;
            DoctorStatus updatedDoctorStatus = DoctorStatus.AVAILABLE;
            staffRepository
                .Setup(repo => repo.UpdateStaffAvailability(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DoctorStatus>()))
                .Callback<int, bool, DoctorStatus>((staffId, isAvailable, status) =>
                {
                    updateCount++;
                    updatedStaffId = staffId;
                    updatedAvailability = isAvailable;
                    updatedDoctorStatus = status;
                });

            // Act
            service.CancelShift(shiftId);

            // Assert
            Assert.Equal(1, updateCount);
            Assert.Equal(doctor.StaffID, updatedStaffId);
            Assert.Equal(false, updatedAvailability);
            Assert.Equal(DoctorStatus.OFF_DUTY, updatedDoctorStatus);
        }

        [Fact]
        public void CancelShift_WhenShiftExists_UpdatesShiftStatusToCompleted()
        {
            // Arrange
            var shiftId = 201;
            var doctor = BuildDoctor(21, "Cardiology");
            shiftRepository
                .Setup(repo => repo.GetShifts())
                .Returns(new List<Shift>
                {
                    BuildShift(shiftId, doctor, new DateTime(2026, 4, 21, 10, 0, 0), new DateTime(2026, 4, 21, 18, 0, 0))
                });

            int updateCount = 0;
            int updatedShiftId = -1;
            ShiftStatus updatedStatus = ShiftStatus.SCHEDULED;
            shiftRepository
                .Setup(repo => repo.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()))
                .Callback<int, ShiftStatus>((id, status) =>
                {
                    updateCount++;
                    updatedShiftId = id;
                    updatedStatus = status;
                });

            // Act
            service.CancelShift(shiftId);

            // Assert
            Assert.Equal(1, updateCount);
            Assert.Equal(shiftId, updatedShiftId);
            Assert.Equal(ShiftStatus.COMPLETED, updatedStatus);
        }

        [Fact]
        public void CancelShift_WhenShiftDoesNotExist_DoesNotUpdateStaffAvailability()
        {
            // Arrange
            shiftRepository.Setup(repo => repo.GetShifts()).Returns(new List<Shift>());

            int updateCount = 0;
            staffRepository
                .Setup(repo => repo.UpdateStaffAvailability(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DoctorStatus>()))
                .Callback(() => updateCount++);

            // Act
            service.CancelShift(999);

            // Assert
            Assert.Equal(0, updateCount);
        }

        [Fact]
        public void CancelShift_WhenShiftDoesNotExist_DoesNotUpdateShiftStatus()
        {
            // Arrange
            shiftRepository.Setup(repo => repo.GetShifts()).Returns(new List<Shift>());

            int updateCount = 0;
            shiftRepository
                .Setup(repo => repo.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()))
                .Callback(() => updateCount++);

            // Act
            service.CancelShift(999);

            // Assert
            Assert.Equal(0, updateCount);
        }

        [Theory]
        [InlineData(8, 10, true)]
        [InlineData(9, 11, false)]
        [InlineData(10, 12, false)]
        [InlineData(11, 13, false)]
        [InlineData(12, 14, true)]
        public void ValidateNoOverlap_WhenBoundaryAndOverlapCasesAreChecked_ReturnsExpectedResult(
            int candidateStartHour,
            int candidateEndHour,
            bool expected)
        {
            // Arrange
            var day = new DateTime(2026, 4, 21);
            var existingDoctor = BuildDoctor(25, "Cardiology");
            var existingShift = BuildShift(1, existingDoctor, day.AddHours(10), day.AddHours(12));
            shiftRepository.Setup(repo => repo.GetShifts()).Returns(new List<Shift> { existingShift });

            // Act
            var result = service.ValidateNoOverlap(existingDoctor.StaffID, day.AddHours(candidateStartHour), day.AddHours(candidateEndHour));

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ValidateNoOverlap_WhenShiftsBelongToOtherStaff_ReturnsTrue()
        {
            // Arrange
            var day = new DateTime(2026, 4, 21);
            var otherDoctor = BuildDoctor(200, "Neurology");
            var shiftForOtherStaff = BuildShift(10, otherDoctor, day.AddHours(10), day.AddHours(12));
            shiftRepository.Setup(repo => repo.GetShifts()).Returns(new List<Shift> { shiftForOtherStaff });

            // Act
            var result = service.ValidateNoOverlap(201, day.AddHours(10), day.AddHours(12));

            // Assert
            Assert.Equal(true, result);
        }

        [Theory]
        [InlineData("Pharmacy")]
        [InlineData("pharmacy")]
        [InlineData("PHARMACY")]
        public void GetFilteredStaff_WhenLocationIsPharmacy_ReturnsMatchingPharmacistStaffIds(string location)
        {
            // Arrange
            var matchingPharmacist = BuildPharmacyst(1, "Sterile Compounding");
            var nonMatchingPharmacist = BuildPharmacyst(2, "Oncology");
            var doctor = BuildDoctor(3, "Cardiology");
            staffRepository.Setup(repo => repo.LoadAllStaff()).Returns(new List<IStaff>
            {
                matchingPharmacist,
                nonMatchingPharmacist,
                doctor,
            });

            // Act
            var result = service.GetFilteredStaff(location, "sterile");

            // Assert
            Assert.Equal(new[] { matchingPharmacist.StaffID }, result.Select(staff => staff.StaffID).ToArray());
        }

        [Fact]
        public void GetFilteredStaff_WhenLocationIsNotPharmacy_ReturnsMatchingDoctorStaffIds()
        {
            // Arrange
            var matchingDoctor = BuildDoctor(10, "Cardiology");
            var nonMatchingDoctor = BuildDoctor(11, "Neurology");
            var pharmacist = BuildPharmacyst(12, "Sterile Compounding");
            staffRepository.Setup(repo => repo.LoadAllStaff()).Returns(new List<IStaff>
            {
                matchingDoctor,
                nonMatchingDoctor,
                pharmacist,
            });

            // Act
            var result = service.GetFilteredStaff("ER", "cardio");

            // Assert
            Assert.Equal(new[] { matchingDoctor.StaffID }, result.Select(staff => staff.StaffID).ToArray());
        }

        [Theory]
        [InlineData("Pharmacy")]
        [InlineData("pharmacy")]
        [InlineData("PHARMACY")]
        public void GetSpecializationsAndCertificationsForLocation_WhenLocationIsPharmacy_ReturnsDistinctSortedNonEmptyCertifications(string location)
        {
            // Arrange
            var compounding = BuildPharmacyst(30, "Compounding");
            var toxicology = BuildPharmacyst(31, "Toxicology");
            var duplicateCompounding = BuildPharmacyst(32, "compounding");
            var emptyCertification = BuildPharmacyst(33, string.Empty);
            var nullCertification = BuildPharmacyst(34, "Unused");
            nullCertification.Certification = null!;
            var doctor = BuildDoctor(35, "Cardiology");

            staffRepository.Setup(repo => repo.LoadAllStaff()).Returns(new List<IStaff>
            {
                compounding,
                toxicology,
                duplicateCompounding,
                emptyCertification,
                nullCertification,
                doctor,
            });

            // Act
            var result = service.GetSpecializationsAndCertificationsForLocation(location);

            // Assert
            Assert.Equal(new[] { "Compounding", "Toxicology" }, result);
        }

        [Fact]
        public void GetSpecializationsAndCertificationsForLocation_WhenLocationIsNotPharmacy_ReturnsDistinctSortedNonEmptySpecializations()
        {
            // Arrange
            var cardiology = BuildDoctor(40, "Cardiology");
            var neurology = BuildDoctor(41, "Neurology");
            var duplicateCardiology = BuildDoctor(42, "cardiology");
            var emptySpecialization = BuildDoctor(43, string.Empty);
            var nullSpecialization = BuildDoctor(44, "Unused");
            nullSpecialization.Specialization = null!;
            var pharmacist = BuildPharmacyst(45, "Compounding");

            staffRepository.Setup(repo => repo.LoadAllStaff()).Returns(new List<IStaff>
            {
                cardiology,
                neurology,
                duplicateCardiology,
                emptySpecialization,
                nullSpecialization,
                pharmacist,
            });

            // Act
            var result = service.GetSpecializationsAndCertificationsForLocation("ER");

            // Assert
            Assert.Equal(new[] { "Cardiology", "Neurology" }, result);
        }

        [Fact]
        public void GetSpecializationsAndCertificationsForLocation_WhenNoMatchingNonEmptyValuesExist_ReturnsEmptyList()
        {
            // Arrange
            var doctor = BuildDoctor(50, string.Empty);
            var pharmacist = BuildPharmacyst(51, string.Empty);
            staffRepository.Setup(repo => repo.LoadAllStaff()).Returns(new List<IStaff>
            {
                doctor,
                pharmacist,
            });

            // Act
            var pharmacyResult = service.GetSpecializationsAndCertificationsForLocation("Pharmacy");
            var erResult = service.GetSpecializationsAndCertificationsForLocation("ER");

            // Assert
            Assert.Empty(pharmacyResult);
            Assert.Empty(erResult);
        }

        [Fact]
        public void FindStaffReplacements_WhenShiftIsNull_ReturnsEmptyList()
        {
            // Act
            var result = service.FindStaffReplacements(null!);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindStaffReplacements_WhenAppointedStaffIsNull_ReturnsEmptyList()
        {
            // Arrange
            var shift = BuildShift(90, BuildDoctor(900, "Cardiology"), new DateTime(2026, 4, 21, 8, 0, 0), new DateTime(2026, 4, 21, 16, 0, 0));
            shift.AppointedStaff = null!;

            // Act
            var result = service.FindStaffReplacements(shift);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindStaffReplacements_WhenCandidatesIncludeOverlapAndDifferentType_ReturnsOnlyCompatibleNonOverlappingStaff()
        {
            // Arrange
            var day = new DateTime(2026, 4, 21);
            var currentDoctor = BuildDoctor(1, "Cardiology");
            var candidateNoOverlap = BuildDoctor(2, "Cardiology");
            var candidateOverlap = BuildDoctor(3, "Cardiology");
            var candidateDifferentType = BuildPharmacyst(4, "Sterile Compounding");
            var anotherNoOverlapDoctor = BuildDoctor(5, "Cardiology");

            var targetShift = BuildShift(1000, currentDoctor, day.AddHours(10), day.AddHours(12));
            var conflictingShift = BuildShift(2000, candidateOverlap, day.AddHours(11), day.AddHours(13));

            staffRepository.Setup(repo => repo.LoadAllStaff()).Returns(new List<IStaff>
            {
                currentDoctor,
                candidateNoOverlap,
                candidateOverlap,
                candidateDifferentType,
                anotherNoOverlapDoctor,
            });

            shiftRepository.Setup(repo => repo.GetShifts()).Returns(new List<Shift>
            {
                targetShift,
                conflictingShift,
            });

            // Act
            var replacements = service.FindStaffReplacements(targetShift);

            // Assert
            Assert.Equal(
                new[]
                {
                    candidateNoOverlap.StaffID,
                    anotherNoOverlapDoctor.StaffID,
                },
                replacements.Select(staff => staff.StaffID).ToArray());
        }

        [Fact]
        public void SetShiftActive_WhenExecutingActivationFlow_InvokesExpectedRepositoryCallOrder()
        {
            // Arrange
            var shiftId = 500;
            var doctor = BuildDoctor(50, "Emergency Medicine");
            var shift = BuildShift(shiftId, doctor, new DateTime(2026, 4, 21, 8, 0, 0), new DateTime(2026, 4, 21, 16, 0, 0));
            var callOrder = new List<string>();

            shiftRepository
                .Setup(repo => repo.GetShifts())
                .Returns(() =>
                {
                    callOrder.Add("GetShifts");
                    return new List<Shift> { shift };
                });

            shiftRepository
                .Setup(repo => repo.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()))
                .Callback(() => callOrder.Add("UpdateShiftStatus"));

            staffRepository
                .Setup(repo => repo.UpdateStaffAvailability(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DoctorStatus>()))
                .Callback(() => callOrder.Add("UpdateStaffAvailability"));

            // Act
            service.SetShiftActive(shiftId);

            // Assert
            Assert.Equal(
                new[]
                {
                    "GetShifts",
                    "UpdateShiftStatus",
                    "UpdateStaffAvailability",
                },
                callOrder);
        }

        [Fact]
        public void CancelShift_WhenExecutingCancellationFlow_InvokesExpectedRepositoryCallOrder()
        {
            // Arrange
            var shiftId = 501;
            var doctor = BuildDoctor(51, "Emergency Medicine");
            var shift = BuildShift(shiftId, doctor, new DateTime(2026, 4, 21, 8, 0, 0), new DateTime(2026, 4, 21, 16, 0, 0));
            var callOrder = new List<string>();

            shiftRepository
                .Setup(repo => repo.GetShifts())
                .Returns(() =>
                {
                    callOrder.Add("GetShifts");
                    return new List<Shift> { shift };
                });

            staffRepository
                .Setup(repo => repo.UpdateStaffAvailability(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DoctorStatus>()))
                .Callback(() => callOrder.Add("UpdateStaffAvailability"));

            shiftRepository
                .Setup(repo => repo.UpdateShiftStatus(It.IsAny<int>(), It.IsAny<ShiftStatus>()))
                .Callback(() => callOrder.Add("UpdateShiftStatus"));

            // Act
            service.CancelShift(shiftId);

            // Assert
            Assert.Equal(
                new[]
                {
                    "GetShifts",
                    "UpdateStaffAvailability",
                    "UpdateShiftStatus",
                },
                callOrder);
        }

        [Fact]
        public void ReassignShift_ReturnsFalse_WhenShiftIsNull()
        {
            var result = service.ReassignShift(null!, BuildDoctor(60, "Cardiology"));

            Assert.False(result);
        }

        [Fact]
        public void ReassignShift_ReturnsFalse_WhenNewStaffIsNull()
        {
            var shift = BuildShift(600, BuildDoctor(61, "Cardiology"), new DateTime(2026, 4, 21, 8, 0, 0), new DateTime(2026, 4, 21, 16, 0, 0));

            var result = service.ReassignShift(shift, null!);

            Assert.False(result);
        }

        [Fact]
        public void ReassignShift_ReturnsTrue_WhenBothAreValid()
        {
            var shift = BuildShift(601, BuildDoctor(62, "Cardiology"), new DateTime(2026, 4, 21, 8, 0, 0), new DateTime(2026, 4, 21, 16, 0, 0));
            var newStaff = BuildDoctor(63, "Neurology");

            var result = service.ReassignShift(shift, newStaff);

            Assert.True(result);
        }

        [Fact]
        public void ReassignShift_UpdatesAppointedStaff_WhenBothAreValid()
        {
            var originalDoctor = BuildDoctor(64, "Cardiology");
            var shift = BuildShift(602, originalDoctor, new DateTime(2026, 4, 21, 8, 0, 0), new DateTime(2026, 4, 21, 16, 0, 0));
            var newStaff = BuildDoctor(65, "Neurology");

            service.ReassignShift(shift, newStaff);

            Assert.Equal(newStaff.StaffID, shift.AppointedStaff.StaffID);
        }

        [Fact]
        public void AddShift_DelegatesToRepository()
        {
            var doctor = BuildDoctor(66, "Cardiology");
            var shift = BuildShift(603, doctor, new DateTime(2026, 4, 21, 8, 0, 0), new DateTime(2026, 4, 21, 16, 0, 0));

            service.AddShift(shift);

            shiftRepository.Verify(r => r.AddShift(shift), Times.Once);
        }

        [Fact]
        public void GetDailyShifts_ReturnsOnlyShiftsOnGivenDate()
        {
            var day = new DateTime(2026, 4, 21);
            var doctor = BuildDoctor(67, "Cardiology");
            var matchingShift = BuildShift(700, doctor, day.AddHours(8), day.AddHours(16));
            var differentDayShift = BuildShift(701, doctor, day.AddDays(1).AddHours(8), day.AddDays(1).AddHours(16));
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { matchingShift, differentDayShift });

            var result = service.GetDailyShifts(day);

            Assert.Single(result);
            Assert.Equal(700, result[0].Id);
        }

        [Fact]
        public void GetDailyShifts_ReturnsEmptyList_WhenNoShiftsOnDate()
        {
            var day = new DateTime(2026, 4, 21);
            var doctor = BuildDoctor(68, "Cardiology");
            var otherDayShift = BuildShift(702, doctor, day.AddDays(1).AddHours(8), day.AddDays(1).AddHours(16));
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { otherDayShift });

            var result = service.GetDailyShifts(day);

            Assert.Empty(result);
        }

        private static Shift BuildShift(int id, IStaff appointedStaff, DateTime start, DateTime end, ShiftStatus status = ShiftStatus.SCHEDULED)
            => new Shift(id, appointedStaff, "ER", start, end, status);

        private static Doctor BuildDoctor(int staffId, string specialization)
            => new Doctor(staffId, "John", "Doe", "john.doe@example.com", string.Empty, false, specialization, "LIC-1", DoctorStatus.OFF_DUTY, 5);

        [Fact]
        public void GetActiveShifts_ReturnsOnlyActiveShifts()
        {
            var day = new DateTime(2026, 4, 21);
            var doctor = BuildDoctor(70, "Cardiology");
            var activeShift = BuildShift(800, doctor, day.AddHours(8), day.AddHours(16), ShiftStatus.ACTIVE);
            var scheduledShift = BuildShift(801, doctor, day.AddHours(16), day.AddHours(20), ShiftStatus.SCHEDULED);
            var cancelledShift = BuildShift(802, doctor, day.AddDays(1).AddHours(8), day.AddDays(1).AddHours(16), ShiftStatus.CANCELLED);
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { activeShift, scheduledShift, cancelledShift });

            var result = service.GetActiveShifts();

            Assert.Single(result);
            Assert.Equal(800, result[0].Id);
        }

        [Fact]
        public void GetActiveShifts_WhenNoActiveShifts_ReturnsEmptyList()
        {
            var day = new DateTime(2026, 4, 21);
            var doctor = BuildDoctor(71, "Cardiology");
            var scheduledShift = BuildShift(803, doctor, day.AddHours(8), day.AddHours(16), ShiftStatus.SCHEDULED);
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { scheduledShift });

            var result = service.GetActiveShifts();

            Assert.Empty(result);
        }

        [Fact]
        public void GetWeeklyHours_WhenShiftsAreInCurrentWeek_ReturnsTotalHours()
        {
            var now = DateTime.Now;
            int daysFromMonday = (7 + (int)(now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekMonday = now.Date.AddDays(-daysFromMonday);
            var doctor = BuildDoctor(72, "Cardiology");
            var shiftOne = BuildShift(810, doctor, weekMonday.AddHours(8), weekMonday.AddHours(16));
            var shiftTwo = BuildShift(811, doctor, weekMonday.AddDays(1).AddHours(8), weekMonday.AddDays(1).AddHours(12));
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { shiftOne, shiftTwo });

            var result = service.GetWeeklyHours(doctor.StaffID);

            Assert.Equal(12f, result);
        }

        [Fact]
        public void GetWeeklyHours_WhenShiftsAreOutsideCurrentWeek_ReturnsZero()
        {
            var doctor = BuildDoctor(73, "Cardiology");
            var pastShift = BuildShift(812, doctor, DateTime.Now.AddDays(-14).AddHours(8), DateTime.Now.AddDays(-14).AddHours(16));
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { pastShift });

            var result = service.GetWeeklyHours(doctor.StaffID);

            Assert.Equal(0f, result);
        }

        [Fact]
        public void IsStaffWorkingDuring_WhenScheduledShiftOverlaps_ReturnsTrue()
        {
            var day = DateTime.Now.AddDays(1);
            var doctor = BuildDoctor(74, "Cardiology");
            var shift = BuildShift(820, doctor, day.AddHours(8), day.AddHours(16), ShiftStatus.SCHEDULED);
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { shift });

            var result = service.IsStaffWorkingDuring(doctor.StaffID, day.AddHours(10), day.AddHours(12));

            Assert.True(result);
        }

        [Fact]
        public void IsStaffWorkingDuring_WhenNoOverlap_ReturnsFalse()
        {
            var day = DateTime.Now.AddDays(1);
            var doctor = BuildDoctor(75, "Cardiology");
            var shift = BuildShift(821, doctor, day.AddHours(8), day.AddHours(16), ShiftStatus.SCHEDULED);
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { shift });

            var result = service.IsStaffWorkingDuring(doctor.StaffID, day.AddHours(17), day.AddHours(19));

            Assert.False(result);
        }

        [Fact]
        public void IsStaffWorkingDuring_WhenShiftIsFinished_ReturnsFalse()
        {
            var day = DateTime.Now.AddDays(1);
            var doctor = BuildDoctor(76, "Cardiology");
            var shift = BuildShift(822, doctor, day.AddHours(8), day.AddHours(16), ShiftStatus.COMPLETED);
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { shift });

            var result = service.IsStaffWorkingDuring(doctor.StaffID, day.AddHours(10), day.AddHours(12));

            Assert.False(result);
        }

        private static Pharmacyst BuildPharmacyst(int staffId, string certification)
            => new Pharmacyst(staffId, "Pharma", "Cist", "pharma@example.com", true, certification, 4);


        [Fact]
        public void TryAddShift_WhenNoOverlap_AddsShiftAndReturnsTrue()
        {
            var doctor = BuildDoctor(90, "Cardiology");
            var start = new DateTime(2030, 7, 1, 8, 0, 0);
            var end = new DateTime(2030, 7, 1, 16, 0, 0);
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift>());
            Shift? added = null;
            shiftRepository.Setup(r => r.AddShift(It.IsAny<Shift>())).Callback<Shift>(s => added = s);

            bool result = service.TryAddShift(doctor, start, end, "ER");

            Assert.True(result);
            Assert.NotNull(added);
            Assert.Equal(doctor.StaffID, added!.AppointedStaff.StaffID);
            Assert.Equal("ER", added.Location);
            Assert.Equal(ShiftStatus.SCHEDULED, added.Status);
        }

        [Fact]
        public void TryAddShift_WhenOverlapExists_DoesNotAddShiftAndReturnsFalse()
        {
            var doctor = BuildDoctor(91, "Cardiology");
            var existing = BuildShift(900, doctor, new DateTime(2030, 7, 2, 8, 0, 0), new DateTime(2030, 7, 2, 16, 0, 0));
            shiftRepository.Setup(r => r.GetShifts()).Returns(new List<Shift> { existing });
            int addCalls = 0;
            shiftRepository.Setup(r => r.AddShift(It.IsAny<Shift>())).Callback(() => addCalls++);

            bool result = service.TryAddShift(doctor, new DateTime(2030, 7, 2, 10, 0, 0), new DateTime(2030, 7, 2, 14, 0, 0), "ER");

            Assert.False(result);
            Assert.Equal(0, addCalls);
        }

        [Fact]
        public void ValidateShiftTimes_ReturnsTrue_WhenEndIsAfterStart()
        {
            var result = service.ValidateShiftTimes(new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0));

            Assert.True(result);
        }

        [Fact]
        public void ValidateShiftTimes_ReturnsFalse_WhenEndEqualsStart()
        {
            var result = service.ValidateShiftTimes(new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0));

            Assert.False(result);
        }

        [Fact]
        public void ValidateShiftTimes_ReturnsFalse_WhenEndIsBeforeStart()
        {
            var result = service.ValidateShiftTimes(new TimeSpan(16, 0, 0), new TimeSpan(8, 0, 0));

            Assert.False(result);
        }
    }
}
