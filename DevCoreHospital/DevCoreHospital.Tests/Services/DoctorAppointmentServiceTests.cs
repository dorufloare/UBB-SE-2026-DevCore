using System;
using System.Threading.Tasks;
using DevCoreHospital.Data;
using DevCoreHospital.Models;
using DevCoreHospital.Services;
using Moq;

namespace DevCoreHospital.Tests.Services
{
    public class DoctorAppointmentServiceTests
    {
        private readonly Mock<IDoctorAppointmentDataSource> mockDataSource;
        private readonly DoctorAppointmentService service;

        public DoctorAppointmentServiceTests()
        {
            mockDataSource = new Mock<IDoctorAppointmentDataSource>();
            service = new DoctorAppointmentService(mockDataSource.Object);
        }

        // ── BookAppointmentAsync ────────────────────────────────────────────────

        [Fact]
        public async Task BookAppointmentAsync_AddsAppointment_WithGivenAppointmentObject()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, DoctorId = 10 };

            // Act
            await service.BookAppointmentAsync(appointment);

            // Assert
            mockDataSource.Verify(x => x.AddAppointmentAsync(appointment), Times.Once);
        }

        [Fact]
        public async Task BookAppointmentAsync_SetsDoctorStatus_ToInExamination()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, DoctorId = 10 };

            // Act
            await service.BookAppointmentAsync(appointment);

            // Assert
            mockDataSource.Verify(x => x.UpdateDoctorStatusAsync(10, "IN_EXAMINATION"), Times.Once);
        }

        // ── FinishAppointmentAsync ──────────────────────────────────────────────

        [Fact]
        public async Task FinishAppointmentAsync_SetsAppointmentStatus_ToFinished()
        {
            // Arrange
            var appointment = new Appointment { Id = 5, DoctorId = 10 };
            mockDataSource.Setup(x => x.GetActiveAppointmentsCountForDoctorAsync(10)).ReturnsAsync(1);

            // Act
            await service.FinishAppointmentAsync(appointment);

            // Assert
            mockDataSource.Verify(x => x.UpdateAppointmentStatusAsync(5, "Finished"), Times.Once);
        }

        [Fact]
        public async Task FinishAppointmentAsync_SetsDoctorStatus_ToAvailable_WhenNoActiveAppointmentsRemain()
        {
            // Arrange
            var appointment = new Appointment { Id = 5, DoctorId = 10 };
            mockDataSource.Setup(x => x.GetActiveAppointmentsCountForDoctorAsync(10)).ReturnsAsync(0);

            // Act
            await service.FinishAppointmentAsync(appointment);

            // Assert
            mockDataSource.Verify(x => x.UpdateDoctorStatusAsync(10, "AVAILABLE"), Times.Once);
        }

        [Fact]
        public async Task FinishAppointmentAsync_DoesNotUpdateDoctorStatus_WhenActiveAppointmentsRemain()
        {
            // Arrange
            var appointment = new Appointment { Id = 5, DoctorId = 10 };
            mockDataSource.Setup(x => x.GetActiveAppointmentsCountForDoctorAsync(10)).ReturnsAsync(2);

            // Act
            await service.FinishAppointmentAsync(appointment);

            // Assert
            mockDataSource.Verify(x => x.UpdateDoctorStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        // ── CancelAppointmentAsync ──────────────────────────────────────────────

        [Fact]
        public async Task CancelAppointmentAsync_ThrowsInvalidOperationException_WhenAppointmentIsAlreadyFinished()
        {
            // Arrange
            var finishedAppointment = new Appointment { Id = 3, DoctorId = 10, Status = "Finished" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelAppointmentAsync(finishedAppointment));

            Assert.Equal("Cannot cancel an appointment that is already Finished.", exception.Message);
        }

        [Fact]
        public async Task CancelAppointmentAsync_ThrowsInvalidOperationException_WhenFinishedStatus_IsCaseDifferent()
        {
            // Arrange
            var finishedAppointment = new Appointment { Id = 3, DoctorId = 10, Status = "FINISHED" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelAppointmentAsync(finishedAppointment));
        }

        [Fact]
        public async Task CancelAppointmentAsync_SetsAppointmentStatus_ToCanceled_WhenAppointmentIsScheduled()
        {
            // Arrange
            var scheduledAppointment = new Appointment { Id = 3, DoctorId = 10, Status = "Scheduled" };

            // Act
            await service.CancelAppointmentAsync(scheduledAppointment);

            // Assert
            mockDataSource.Verify(x => x.UpdateAppointmentStatusAsync(3, "Canceled"), Times.Once);
        }

        [Fact]
        public async Task CancelAppointmentAsync_DoesNotUpdateStatus_WhenAppointmentIsAlreadyFinished()
        {
            // Arrange
            var finishedAppointment = new Appointment { Id = 3, DoctorId = 10, Status = "Finished" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelAppointmentAsync(finishedAppointment));

            mockDataSource.Verify(x => x.UpdateAppointmentStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }
    }
}
