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


        [Fact]
        public async Task BookAppointmentAsync_AddsAppointment_WithGivenAppointmentObject()
        {
            var appointment = new Appointment { Id = 1, DoctorId = 10 };

            await service.BookAppointmentAsync(appointment);

            mockDataSource.Verify(x => x.AddAppointmentAsync(appointment), Times.Once);
        }

        [Fact]
        public async Task BookAppointmentAsync_SetsDoctorStatus_ToInExamination()
        {
            var appointment = new Appointment { Id = 1, DoctorId = 10 };

            await service.BookAppointmentAsync(appointment);

            mockDataSource.Verify(x => x.UpdateDoctorStatusAsync(10, "IN_EXAMINATION"), Times.Once);
        }


        [Fact]
        public async Task FinishAppointmentAsync_SetsAppointmentStatus_ToFinished()
        {
            var appointment = new Appointment { Id = 5, DoctorId = 10 };
            mockDataSource.Setup(x => x.GetActiveAppointmentsCountForDoctorAsync(10)).ReturnsAsync(1);

            await service.FinishAppointmentAsync(appointment);

            mockDataSource.Verify(x => x.UpdateAppointmentStatusAsync(5, "Finished"), Times.Once);
        }

        [Fact]
        public async Task FinishAppointmentAsync_SetsDoctorStatus_ToAvailable_WhenNoActiveAppointmentsRemain()
        {
            var appointment = new Appointment { Id = 5, DoctorId = 10 };
            mockDataSource.Setup(x => x.GetActiveAppointmentsCountForDoctorAsync(10)).ReturnsAsync(0);

            await service.FinishAppointmentAsync(appointment);

            mockDataSource.Verify(x => x.UpdateDoctorStatusAsync(10, "AVAILABLE"), Times.Once);
        }

        [Fact]
        public async Task FinishAppointmentAsync_DoesNotUpdateDoctorStatus_WhenActiveAppointmentsRemain()
        {
            var appointment = new Appointment { Id = 5, DoctorId = 10 };
            mockDataSource.Setup(x => x.GetActiveAppointmentsCountForDoctorAsync(10)).ReturnsAsync(2);

            await service.FinishAppointmentAsync(appointment);

            mockDataSource.Verify(x => x.UpdateDoctorStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }


        [Fact]
        public async Task CancelAppointmentAsync_ThrowsInvalidOperationException_WhenAppointmentIsAlreadyFinished()
        {
            var finishedAppointment = new Appointment { Id = 3, DoctorId = 10, Status = "Finished" };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelAppointmentAsync(finishedAppointment));

            Assert.Equal("Cannot cancel an appointment that is already Finished.", exception.Message);
        }

        [Fact]
        public async Task CancelAppointmentAsync_ThrowsInvalidOperationException_WhenFinishedStatus_IsCaseDifferent()
        {
            var finishedAppointment = new Appointment { Id = 3, DoctorId = 10, Status = "FINISHED" };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelAppointmentAsync(finishedAppointment));
        }

        [Fact]
        public async Task CancelAppointmentAsync_SetsAppointmentStatus_ToCanceled_WhenAppointmentIsScheduled()
        {
            var scheduledAppointment = new Appointment { Id = 3, DoctorId = 10, Status = "Scheduled" };

            await service.CancelAppointmentAsync(scheduledAppointment);

            mockDataSource.Verify(x => x.UpdateAppointmentStatusAsync(3, "Canceled"), Times.Once);
        }

        [Fact]
        public async Task CancelAppointmentAsync_DoesNotUpdateStatus_WhenAppointmentIsAlreadyFinished()
        {
            var finishedAppointment = new Appointment { Id = 3, DoctorId = 10, Status = "Finished" };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelAppointmentAsync(finishedAppointment));

            mockDataSource.Verify(x => x.UpdateAppointmentStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllDoctorsAsync_ReturnsResultFromDataSource()
        {
            IReadOnlyList<(int DoctorId, string DoctorName)> expected = new List<(int, string)>
            {
                (1, "Dr. Smith"),
                (2, "Dr. Jones")
            };
            mockDataSource.Setup(x => x.GetAllDoctorsAsync()).ReturnsAsync(expected);

            var result = await service.GetAllDoctorsAsync();

            Assert.Same(expected, result);
            mockDataSource.Verify(x => x.GetAllDoctorsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAppointmentDetailsAsync_CallsDataSourceWithCorrectId()
        {
            var expected = new Appointment { Id = 42, DoctorId = 5, PatientName = "Jane Doe" };
            mockDataSource.Setup(x => x.GetAppointmentDetailsAsync(42)).ReturnsAsync(expected);

            var result = await service.GetAppointmentDetailsAsync(42);

            Assert.Same(expected, result);
            mockDataSource.Verify(x => x.GetAppointmentDetailsAsync(42), Times.Once);
        }

        [Fact]
        public async Task GetAppointmentDetailsAsync_ReturnsNull_WhenNotFound()
        {
            mockDataSource.Setup(x => x.GetAppointmentDetailsAsync(99)).ReturnsAsync((Appointment?)null);

            var result = await service.GetAppointmentDetailsAsync(99);

            Assert.Null(result);
        }
    }
}
