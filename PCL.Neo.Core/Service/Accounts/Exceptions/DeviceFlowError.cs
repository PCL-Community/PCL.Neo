using PCL.Neo.Core.Service.Accounts.MicrosoftAuth;

namespace PCL.Neo.Core.Service.Accounts.Exceptions;

public class DeviceFlowError(
    DeviceFlowState state,
    Exception? exc) : DeviceFlowState
{
    public DeviceFlowState State { get; init; } = state;
    public Exception? Exc { get; init; } = exc;
}
