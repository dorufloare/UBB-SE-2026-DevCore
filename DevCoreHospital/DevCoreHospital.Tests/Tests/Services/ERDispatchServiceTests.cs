using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using Moq;
using Xunit;

namespace DevCoreHospital.Tests.Services;

public class ERDispatchServiceTests
{
    [Fact]
    public async Task DispatchERRequestAsync_WhenPendingListDoesNotContainRequestId_ReturnsFailureResult()
    {
        var repository = new Mock<IERDispatchRepository>();
        repository
            .Setup(dispatcherRepository => dispatcherRepository.GetPendingRequests())
            .Returns(Array.Empty<ERRequest>());
        var service = new ERDispatchService(repository.Object);

        var dispatchResult = await service.DispatchERRequestAsync(42);

        Assert.False(dispatchResult.IsSuccess);
    }

    [Fact]
    public async Task DispatchERRequestAsync_WhenRosterHasAvailableSpecialistInLocation_ReturnsMatchedDoctorName()
    {
        var pendingRequest = new ERRequest { Id = 1, Specialization = "Cardiology", Location = "Ward A" };
        var availableDoctorRosterEntry = new DoctorRosterEntry
        {
            DoctorId = 10,
            FullName = "Dr X",
            Specialization = "Cardiology",
            Location = "Ward A",
            StatusRaw = "AVAILABLE",
            ScheduleStart = DateTime.Now.AddHours(-1),
            ScheduleEnd = DateTime.Now.AddHours(2),
        };
        var repository = new Mock<IERDispatchRepository>();
        repository
            .Setup(dispatcherRepository => dispatcherRepository.GetPendingRequests())
            .Returns(new[] { pendingRequest });
        repository
            .Setup(dispatcherRepository => dispatcherRepository.GetDoctorRoster())
            .Returns(new[] { availableDoctorRosterEntry });
        var service = new ERDispatchService(repository.Object);

        var dispatchResult = await service.DispatchERRequestAsync(1);

        Assert.Equal("Dr X", dispatchResult.MatchedDoctorName);
    }

    [Fact]
    public async Task DispatchERRequestAsync_WhenNoRosterMatchExists_PersistsUnmatchedStatusInRepository()
    {
        var pendingRequest = new ERRequest { Id = 1, Specialization = "Z99", Location = "Nowhere" };
        string? lastPersistedRequestStatus = null;
        var repository = new Mock<IERDispatchRepository>();
        repository
            .Setup(dispatcherRepository => dispatcherRepository.GetPendingRequests())
            .Returns(new[] { pendingRequest });
        repository
            .Setup(dispatcherRepository => dispatcherRepository.GetDoctorRoster())
            .Returns(Array.Empty<DoctorRosterEntry>());
        repository
            .Setup(dispatcherRepository => dispatcherRepository.UpdateRequestStatus(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>()))
            .Callback(
                (int requestId, string requestStatus, int? assignedDoctorId, string? assignedDoctorName) => lastPersistedRequestStatus = requestStatus);
        var service = new ERDispatchService(repository.Object);

        await service.DispatchERRequestAsync(1);

        Assert.Equal("UNMATCHED", lastPersistedRequestStatus);
    }

    [Fact]
    public async Task GetManualOverrideCandidatesAsync_WhenGetRequestByIdReturnsNull_ReturnsNoCandidates()
    {
        var repository = new Mock<IERDispatchRepository>();
        repository
            .Setup(dispatcherRepository => dispatcherRepository.GetRequestById(1))
            .Returns((ERRequest?)null);
        var service = new ERDispatchService(repository.Object);

        var manualOverrideCandidateProfiles = await service.GetManualOverrideCandidatesAsync(1, 30);

        Assert.Empty(manualOverrideCandidateProfiles);
    }

    [Fact]
    public async Task ManualOverrideAsync_WhenNoNearEndRosterEntryMatchesDoctor_ReturnsUnsuccessfulResult()
    {
        var erRequest = new ERRequest { Id = 1, Specialization = "Cardio", Location = "W1" };
        var overrideDoctorRosterEntry = new DoctorRosterEntry { DoctorId = 5, FullName = "D" };
        var repository = new Mock<IERDispatchRepository>();
        repository
            .Setup(dispatcherRepository => dispatcherRepository.GetRequestById(1))
            .Returns(erRequest);
        repository
            .Setup(dispatcherRepository => dispatcherRepository.GetDoctorById(5))
            .Returns(overrideDoctorRosterEntry);
        repository
            .Setup(dispatcherRepository => dispatcherRepository.GetDoctorRoster())
            .Returns(Array.Empty<DoctorRosterEntry>());
        var service = new ERDispatchService(repository.Object);

        var manualOverrideResult = await service.ManualOverrideAsync(1, 5, 10);

        Assert.False(manualOverrideResult.IsSuccess);
    }
}
