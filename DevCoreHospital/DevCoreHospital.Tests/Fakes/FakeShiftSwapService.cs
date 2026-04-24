using System.Collections.Generic;
using DevCoreHospital.Models;
using DevCoreHospital.Services;

namespace DevCoreHospital.Tests.Fakes;

public sealed class FakeShiftSwapService : IShiftSwapService
{
    public List<IStaff> EligibleColleagues { get; } = new();

    public string EligibleError { get; set; } = string.Empty;

    public bool RequestResult { get; set; }

    public string RequestMessage { get; set; } = string.Empty;

    public List<ShiftSwapRequest> PendingInbox { get; } = new();

    public bool AcceptResult { get; set; }

    public string AcceptMessage { get; set; } = string.Empty;

    public bool RejectResult { get; set; }

    public string RejectMessage { get; set; } = string.Empty;

    public List<IStaff> GetEligibleSwapColleaguesForShift(int requesterId, int shiftId, out string error)
    {
        error = EligibleError;
        return EligibleColleagues;
    }

    public bool RequestShiftSwap(int requesterId, int shiftId, int colleagueId, out string message)
    {
        message = RequestMessage;
        return RequestResult;
    }

    public List<ShiftSwapRequest> GetIncomingSwapRequests(int colleagueId)
        => new List<ShiftSwapRequest>(PendingInbox);

    public bool AcceptSwapRequest(int swapId, int colleagueId, out string message)
    {
        message = AcceptMessage;
        PendingInbox.RemoveAll(request => AcceptResult && request.SwapId == swapId);
        return AcceptResult;
    }

    public bool RejectSwapRequest(int swapId, int colleagueId, out string message)
    {
        message = RejectMessage;
        PendingInbox.RemoveAll(request => RejectResult && request.SwapId == swapId);
        return RejectResult;
    }
}
