using System;
using System.Collections.Generic;
using System.Linq;
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
    public async Task DispatchERRequestAsync_WhenPendingMissing_ReturnsNotSuccess()
    {
        var repo = new Mock<IERDispatchRepository>();
        repo.Setup(r => r.GetPendingRequests()).Returns(Array.Empty<ERRequest>());
        var service = new ERDispatchService(repo.Object);

        var result = await service.DispatchERRequestAsync(42);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DispatchERRequestAsync_WhenDoctorMatches_ReturnsSuccessWithName()
    {
        var request = new ERRequest { Id = 1, Specialization = "Cardiology", Location = "Ward A" };
        var doctorEntry = new DoctorRosterEntry
        {
            DoctorId = 10,
            FullName = "Dr X",
            Specialization = "Cardiology",
            Location = "Ward A",
            StatusRaw = "AVAILABLE",
            ScheduleStart = DateTime.Now.AddHours(-1),
            ScheduleEnd = DateTime.Now.AddHours(2),
        };
        var repo = new Mock<IERDispatchRepository>();
        repo.Setup(r => r.GetPendingRequests()).Returns(new[] { request });
        repo.Setup(r => r.GetDoctorRoster()).Returns(new[] { doctorEntry });
        var service = new ERDispatchService(repo.Object);

        var result = await service.DispatchERRequestAsync(1);

        Assert.Equal("Dr X", result.MatchedDoctorName);
    }

    [Fact]
    public async Task DispatchERRequestAsync_WhenNoMatch_MarksUnmatchedInRepository()
    {
        var request = new ERRequest { Id = 1, Specialization = "Z99", Location = "Nowhere" };
        int? lastStatus = null;
        var repo = new Mock<IERDispatchRepository>();
        repo.Setup(r => r.GetPendingRequests()).Returns(new[] { request });
        repo.Setup(r => r.GetDoctorRoster()).Returns(Array.Empty<DoctorRosterEntry>());
        repo.Setup(r => r.UpdateRequestStatus(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string?>()))
            .Callback<int, string, int?, string?>((id, s, a, b) => lastStatus = s);
        var service = new ERDispatchService(repo.Object);

        await service.DispatchERRequestAsync(1);

        Assert.Equal("UNMATCHED", lastStatus);
    }

    [Fact]
    public async Task GetManualOverrideCandidatesAsync_WhenRequestIdUnknown_ReturnsEmptyList()
    {
        var repo = new Mock<IERDispatchRepository>();
        repo.Setup(r => r.GetRequestById(1)).Returns((ERRequest?)null);
        var service = new ERDispatchService(repo.Object);

        var list = await service.GetManualOverrideCandidatesAsync(1, 30);

        Assert.Empty(list);
    }

    [Fact]
    public async Task ManualOverrideAsync_WhenDoctorIsNotOverrideCandidate_ReturnsNotSuccess()
    {
        var request = new ERRequest { Id = 1, Specialization = "Cardio", Location = "W1" };
        var doctor = new DoctorRosterEntry { DoctorId = 5, FullName = "D" };
        var repo = new Mock<IERDispatchRepository>();
        repo.Setup(r => r.GetRequestById(1)).Returns(request);
        repo.Setup(r => r.GetDoctorById(5)).Returns(doctor);
        repo.Setup(r => r.GetDoctorRoster()).Returns(Array.Empty<DoctorRosterEntry>());
        var service = new ERDispatchService(repo.Object);

        var result = await service.ManualOverrideAsync(1, 5, 10);

        Assert.False(result.IsSuccess);
    }
}
