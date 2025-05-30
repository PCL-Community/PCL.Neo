using PCL.Neo.Core.Service.Accounts.Storage;

namespace PCL.Neo.Core.Service.Accounts.MicrosoftAuth
{
    public class DeviceFlowState;

    public class DeviceFlowAwaitUser(string userCode, string verificationUri) : DeviceFlowState
    {
        public string UserCode { get; } = userCode;
        public string VerificationUri { get; } = verificationUri;
    }

    public class DeviceFlowPolling : DeviceFlowState;

    public class DeviceFlowDeclined : DeviceFlowState;

    public class DeviceFlowExpired : DeviceFlowState;

    public class DeviceFlowBadVerificationCode : DeviceFlowState;

    public class DeviceFlowGetAccountInfo : DeviceFlowState;

    public class DeviceFlowSucceeded(MsaAccount account) : DeviceFlowState

    {
        public MsaAccount Account { get; } = account;
    }

    public class DeviceFlowUnkonw : DeviceFlowState;

    public class DeviceFlowInternetError : DeviceFlowState;

    public class DeviceFlowJsonError : DeviceFlowState;
}