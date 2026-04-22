using System;
using System.Collections.Generic;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using DevCoreHospital.ViewModels;
using DevCoreHospital.ViewModels.Doctor;
using Moq;
using Xunit;
using MDoctor = DevCoreHospital.Models.Doctor;

namespace DevCoreHospital.Tests.Integration;

public class ShiftSwapFlowIntegrationTests
{
    [Fact]
    public void Pipeline_FromRepositoryThroughService_MyScheduleShowsSuccessMessage()
    {
        var requesterDoctor = new MDoctor(1, "A", "A", string.Empty, string.Empty, true, "Sp", "L", DoctorStatus.AVAILABLE, 1);
        var colleagueDoctor = new MDoctor(2, "B", "B", string.Empty, string.Empty, true, "Sp", "L", DoctorStatus.AVAILABLE, 1);
        var shiftStart = DateTime.UtcNow.AddDays(7);
        var requesterShift = new Shift(50, requesterDoctor, "ER", shiftStart, shiftStart.AddHours(6), ShiftStatus.SCHEDULED);
        var staff = new Mock<IStaffRepository>();
        staff.Setup(staffRepository => staffRepository.LoadAllStaff()).Returns(new List<IStaff> { requesterDoctor, colleagueDoctor });
        staff.Setup(staffRepository => staffRepository.GetStaffById(1)).Returns(requesterDoctor);
        var shift = new Mock<IShiftRepository>();
        shift.Setup(shiftRepository => shiftRepository.GetShiftsByStaffID(1)).Returns(new List<Shift> { requesterShift });
        shift.Setup(shiftRepository => shiftRepository.GetShiftById(50)).Returns(requesterShift);
        shift.Setup(shiftRepository => shiftRepository.IsStaffWorkingDuring(2, shiftStart, shiftStart.AddHours(6))).Returns(false);
        var swap = new Mock<IShiftSwapRepository>();
        swap.Setup(shiftSwapRepository => shiftSwapRepository.CreateShiftSwapRequest(It.IsAny<ShiftSwapRequest>())).Returns(1);
        var service = new ShiftSwapService(staff.Object, shift.Object, swap.Object);
        var vm = new MyScheduleViewModel(service, shift.Object, staff.Object);
        vm.SelectedColleague = new StaffOptionViewModel { StaffId = 2, DisplayName = "B" };
        vm.SelectedShift = vm.FutureShifts[0];

        ((RelayCommand)vm.RequestSwapCommand).Execute(null!);

        Assert.Contains("successfully", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pipeline_SwapServiceExposesRepositoryRequests_OnIncomingViewModel()
    {
        var swap = new Mock<IShiftSwapRepository>();
        swap.Setup(shiftSwapRepository => shiftSwapRepository.GetPendingSwapRequestsForColleague(1))
            .Returns(
                new List<ShiftSwapRequest>
                {
                    new() { SwapId = 1, ShiftId = 2, ColleagueId = 1, RequesterId = 3, RequestedAt = DateTime.UtcNow, Status = ShiftSwapRequestStatus.PENDING }
                });
        var staff = new Mock<IStaffRepository>();
        var shift = new Mock<IShiftRepository>();
        IShiftSwapService service = new ShiftSwapService(staff.Object, shift.Object, swap.Object);
        var vm = new IncomingSwapRequestsViewModel(service, new[] { new DoctorOptionViewModel { StaffId = 1, DisplayName = "A" } });

        Assert.Equal(1, vm.Requests.Count);
    }
}
