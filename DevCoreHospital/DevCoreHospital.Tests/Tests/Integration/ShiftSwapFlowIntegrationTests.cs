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
        var d1 = new MDoctor(1, "A", "A", string.Empty, string.Empty, true, "Sp", "L", DoctorStatus.AVAILABLE, 1);
        var d2 = new MDoctor(2, "B", "B", string.Empty, string.Empty, true, "Sp", "L", DoctorStatus.AVAILABLE, 1);
        var t = DateTime.UtcNow.AddDays(7);
        var sh = new Shift(50, d1, "ER", t, t.AddHours(6), ShiftStatus.SCHEDULED);
        var staff = new Mock<IStaffRepository>();
        staff.Setup(r => r.LoadAllStaff()).Returns(new List<IStaff> { d1, d2 });
        staff.Setup(r => r.GetStaffById(1)).Returns(d1);
        var shift = new Mock<IShiftRepository>();
        shift.Setup(r => r.GetShiftsByStaffID(1)).Returns(new List<Shift> { sh });
        shift.Setup(r => r.GetShiftById(50)).Returns(sh);
        shift.Setup(r => r.IsStaffWorkingDuring(2, t, t.AddHours(6))).Returns(false);
        var swap = new Mock<IShiftSwapRepository>();
        swap.Setup(r => r.CreateShiftSwapRequest(It.IsAny<ShiftSwapRequest>())).Returns(1);
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
        swap.Setup(r => r.GetPendingSwapRequestsForColleague(1))
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
