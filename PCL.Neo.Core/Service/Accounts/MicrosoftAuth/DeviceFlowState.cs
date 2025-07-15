using PCL.Neo.Core.Service.Accounts.Storage;

namespace PCL.Neo.Core.Service.Accounts.MicrosoftAuth;

public class DeviceFlowState;

public class DeviceFlowAwaitUser(string userCode, string verificationUri) : DeviceFlowState
{
    public string UserCode { get; } = userCode;
    public string VerificationUri { get; } = verificationUri;
}

public class DeviceFlowPolling : DeviceFlowState;

public class DeviceFlowDeclined : Exception;

public class DeviceFlowExpired : Exception;

public class DeviceFlowBadVerificationCode : Exception;

public class DeviceFlowGetAccountInfo : DeviceFlowState;

public class DeviceFlowSucceeded(MsaAccount account) : DeviceFlowState

{
    public MsaAccount Account { get; } = account;
}

public class DeviceFlowUnkonw : Exception;

public class DeviceFlowInternetError : DeviceFlowState;

public class DeviceFlowJsonError : DeviceFlowState;
