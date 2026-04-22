using System;
using System.Collections.Generic;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using Moq;
using Xunit;

namespace DevCoreHospital.Tests.Services;

public class ShiftSwapServiceTests
{
    [Fact]
    public void GetEligibleSwapColleaguesForShift_WhenShiftMissing_ReturnsShiftNotFoundError()
    {
        var staff = new Mock<IStaffRepository>();
        var shift = new Mock<IShiftRepository>();
        var swap = new Mock<IShiftSwapRepository>();
        shift.Setup(shiftRepository => shiftRepository.GetShiftById(1)).Returns((Shift?)null);
        var service = new ShiftSwapService(staff.Object, shift.Object, swap.Object);

        _ = service.GetEligibleSwapColleaguesForShift(1, 1, out var error);

        Assert.Equal("Shift not found.", error);
    }

    [Fact]
    public void GetEligibleSwapColleaguesForShift_WhenRequesterIsNotAppointed_LimitsToOwnShiftMessage()
    {
        var requesterDoctor = BuildDoctor(1, "Cardio");
        var appointedDoctor = BuildDoctor(2, "Cardio");
        var targetShift = new Shift(10, appointedDoctor, "ER", DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(8), ShiftStatus.SCHEDULED);
        var staff = new Mock<IStaffRepository>();
        var shift = new Mock<IShiftRepository>();
        var swap = new Mock<IShiftSwapRepository>();
        shift.Setup(shiftRepository => shiftRepository.GetShiftById(10)).Returns(targetShift);

        var service = new ShiftSwapService(staff.Object, shift.Object, swap.Object);
        _ = service.GetEligibleSwapColleaguesForShift(1, 10, out var error);

        Assert.Equal("You can only request swap for your own shift.", error);
    }

    [Fact]
    public void GetIncomingSwapRequests_UsesRepositoryList()
    {
        var list = new List<ShiftSwapRequest> { new() { SwapId = 5, ColleagueId = 9, ShiftId = 1, RequesterId = 1, RequestedAt = DateTime.UtcNow, Status = ShiftSwapRequestStatus.PENDING } };
        var staff = new Mock<IStaffRepository>();
        var shift = new Mock<IShiftRepository>();
        var swap = new Mock<IShiftSwapRepository>();
        swap.Setup(shiftSwapRepository => shiftSwapRepository.GetPendingSwapRequestsForColleague(9)).Returns(list);
        var service = new ShiftSwapService(staff.Object, shift.Object, swap.Object);

        var result = service.GetIncomingSwapRequests(9);

        Assert.Same(list, result);
    }

    [Fact]
    public void AcceptSwapRequest_WhenSwapIdUnknown_ReturnsNotFoundMessage()
    {
        var staff = new Mock<IStaffRepository>();
        var shift = new Mock<IShiftRepository>();
        var swap = new Mock<IShiftSwapRepository>();
        swap.Setup(shiftSwapRepository => shiftSwapRepository.GetShiftSwapRequestById(1)).Returns((ShiftSwapRequest?)null);
        var service = new ShiftSwapService(staff.Object, shift.Object, swap.Object);

        _ = service.AcceptSwapRequest(1, 1, out var message);

        Assert.Equal("Swap request not found.", message);
    }

    [Fact]
    public void RejectSwapRequest_WhenValidPending_UpdatesStatusInRepository()
    {
        var pendingSwapRequest = new ShiftSwapRequest { SwapId = 1, ColleagueId = 2, RequesterId = 3, ShiftId = 4, Status = ShiftSwapRequestStatus.PENDING };
        var staff = new Mock<IStaffRepository>();
        var shift = new Mock<IShiftRepository>();
        var swap = new Mock<IShiftSwapRepository>();
        swap.Setup(shiftSwapRepository => shiftSwapRepository.GetShiftSwapRequestById(1)).Returns(pendingSwapRequest);
        string? updatedStatus = null;
        swap.Setup(shiftSwapRepository => shiftSwapRepository.UpdateShiftSwapRequestStatus(It.IsAny<int>(), It.IsAny<string>()))
            .Callback<int, string>((_, st) => updatedStatus = st)
            .Returns(true);
        var service = new ShiftSwapService(staff.Object, shift.Object, swap.Object);

        _ = service.RejectSwapRequest(1, 2, out _);

        Assert.Equal("REJECTED", updatedStatus);
    }

    [Fact]
    public void RequestShiftSwap_WhenIneligibleColleague_ReturnsSelectionMessage()
    {
        var requesterDoctor = BuildDoctor(1, "Oncology");
        var futureShift = new Shift(100, requesterDoctor, "Ward", DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(4), ShiftStatus.SCHEDULED);
        var staff = new Mock<IStaffRepository>();
        var shift = new Mock<IShiftRepository>();
        var swap = new Mock<IShiftSwapRepository>();
        staff.Setup(staffRepository => staffRepository.LoadAllStaff()).Returns(new List<IStaff> { requesterDoctor });
        shift.Setup(shiftRepository => shiftRepository.GetShiftById(100)).Returns(futureShift);
        shift.Setup(shiftRepository => shiftRepository.IsStaffWorkingDuring(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(false);
        var service = new ShiftSwapService(staff.Object, shift.Object, swap.Object);

        _ = service.RequestShiftSwap(1, 100, 9, out var message);

        Assert.Equal("Selected colleague is not eligible (must be same profile and free in interval).", message);
    }

    private static Doctor BuildDoctor(int id, string spec)
        => new(id, "A", "B", string.Empty, string.Empty, true, spec, "L-1", DoctorStatus.AVAILABLE, 1);
}
