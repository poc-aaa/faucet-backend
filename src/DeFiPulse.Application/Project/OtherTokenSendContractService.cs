using Microsoft.Extensions.Options;

namespace DeFiPulse.Project;

public class OtherTokenSendContractService : TokenSendContractServiceBase
{
    public OtherTokenSendContractService(IOptionsSnapshot<ApiConfigOptions> apiOptions) 
        : base(string.IsNullOrEmpty(apiOptions.Value.BaseUrlForMainchain) ? apiOptions.Value.BaseUrl : apiOptions.Value.BaseUrlForMainchain, 
            apiOptions.Value.BaseUrlForSidechain, 
            apiOptions.Value.PrivateKey, 
            apiOptions.Value.SendCount)
    {
    }
}