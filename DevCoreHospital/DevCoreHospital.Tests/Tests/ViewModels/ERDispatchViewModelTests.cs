using System.Collections.Generic;
using System.Threading.Tasks;
using DevCoreHospital.Models;
using DevCoreHospital.Services;
using DevCoreHospital.ViewModels;
using Moq;
using Xunit;

namespace DevCoreHospital.Tests.ViewModels;

public class ERDispatchViewModelTests
{
    [Fact]
    public void Refresh_ClearsUnmatchedAndSuccessfulCollections()
    {
        var service = new Mock<IERDispatchService>(MockBehavior.Loose);
        var vm = new ERDispatchViewModel(service.Object);
        vm.UnmatchedRequests.Add(new ERDispatchViewModel.UnmatchedRequestRow { RequestId = 1 });
        vm.SuccessfulMatches.Add(new ERDispatchViewModel.SuccessfulMatchRow { RequestId = 2 });
        vm.OverrideCandidates.Add(new ERDispatchViewModel.OverrideCandidateRow { DoctorId = 3 });

        vm.Refresh();

        Assert.Equal(0, vm.UnmatchedRequests.Count + vm.SuccessfulMatches.Count + vm.OverrideCandidates.Count);
    }

    [Fact]
    public async Task SimulateIncomingAsync_SetsStatusMessageMentioningSimulated()
    {
        var service = new Mock<IERDispatchService>();
        service.Setup(s => s.SimulateIncomingRequestsAsync(2)).ReturnsAsync((IReadOnlyList<int>)new[] { 1, 2 });
        var vm = new ERDispatchViewModel(service.Object);

        await vm.SimulateIncomingAsync(2);

        Assert.Contains("Simulated", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunDispatchAsync_AddsRowWhenServiceMatches()
    {
        var service = new Mock<IERDispatchService>();
        var req = new ERRequest { Id = 1, Specialization = "A", Location = "L" };
        service.Setup(s => s.GetPendingRequestIdsAsync()).ReturnsAsync((IReadOnlyList<int>)new[] { 1 });
        service.Setup(s => s.DispatchERRequestAsync(1))
            .ReturnsAsync(new ERDispatchResult { IsSuccess = true, Request = req, MatchedDoctorName = "Doc" });
        var vm = new ERDispatchViewModel(service.Object);

        await vm.RunDispatchAsync();

        Assert.Equal(1, vm.SuccessfulMatches.Count);
    }

    [Fact]
    public async Task RunDispatchAsync_AddsUnmatchedWhenServiceDoesNotMatch()
    {
        var service = new Mock<IERDispatchService>();
        var req = new ERRequest { Id = 1, Specialization = "A", Location = "L" };
        service.Setup(s => s.GetPendingRequestIdsAsync()).ReturnsAsync((IReadOnlyList<int>)new[] { 1 });
        service.Setup(s => s.DispatchERRequestAsync(1))
            .ReturnsAsync(new ERDispatchResult { IsSuccess = false, Request = req, Message = "no one" });
        service.Setup(s => s.GetManualOverrideCandidatesAsync(1, 30))
            .ReturnsAsync((IReadOnlyList<DoctorProfile>)new List<DoctorProfile>());
        var vm = new ERDispatchViewModel(service.Object);

        await vm.RunDispatchAsync();

        Assert.Equal(1, vm.UnmatchedRequests.Count);
    }

    [Fact]
    public async Task LoadOverrideCandidatesAsync_UsesNoEligibleHintWhenListEmpty()
    {
        var service = new Mock<IERDispatchService>();
        service.Setup(s => s.GetManualOverrideCandidatesAsync(1, 30))
            .ReturnsAsync((IReadOnlyList<DoctorProfile>)new List<DoctorProfile>());
        var vm = new ERDispatchViewModel(service.Object);

        await vm.LoadOverrideCandidatesAsync(1);

        Assert.Contains("No eligible", vm.ManualInterventionHint, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyOverrideAsync_FailsWhenUnmatchedIdMissingInCollection()
    {
        var service = new Mock<IERDispatchService>(MockBehavior.Strict);
        var vm = new ERDispatchViewModel(service.Object);
        vm.OverrideCandidates.Add(new ERDispatchViewModel.OverrideCandidateRow { DoctorId = 1 });

        var result = await vm.ApplyOverrideAsync(5, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task ApplyOverrideAsync_FailsWhenOverrideDoctorNotInCandidatesList()
    {
        var service = new Mock<IERDispatchService>(MockBehavior.Strict);
        var vm = new ERDispatchViewModel(service.Object);
        vm.UnmatchedRequests.Add(new ERDispatchViewModel.UnmatchedRequestRow { RequestId = 1 });
        vm.OverrideCandidates.Clear();

        var ok = await vm.ApplyOverrideAsync(1, 1);

        Assert.False(ok);
    }

    [Fact]
    public async Task ApplyOverrideAsync_WhenServiceSucceeds_ReturnsTrue()
    {
        var service = new Mock<IERDispatchService>();
        var request = new ERRequest { Id = 1, Specialization = "S", Location = "L" };
        service.Setup(s => s.ManualOverrideAsync(1, 2, 30))
            .ReturnsAsync(
                new ERDispatchResult
                {
                    IsSuccess = true,
                    Request = request,
                    MatchedDoctorName = "Dr Z",
                    MatchReason = "override"
                });
        var vm = new ERDispatchViewModel(service.Object);
        vm.UnmatchedRequests.Add(new ERDispatchViewModel.UnmatchedRequestRow { RequestId = 1, RequestSpecialization = "S", RequestLocation = "L" });
        vm.OverrideCandidates.Add(new ERDispatchViewModel.OverrideCandidateRow { DoctorId = 2, FullName = "Dr Z" });

        var ok = await vm.ApplyOverrideAsync(1, 2);

        Assert.True(ok);
    }
}
