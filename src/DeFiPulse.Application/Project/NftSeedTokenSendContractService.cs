using Microsoft.Extensions.Options;

namespace DeFiPulse.Project;

public class NftSeedTokenSendContractService : TokenSendContractServiceBase
{
    public NftSeedTokenSendContractService(IOptionsSnapshot<ApiConfigOptions> apiOptions) 
        : base(string.IsNullOrEmpty(apiOptions.Value.BaseUrlForMainchain) ? apiOptions.Value.BaseUrl : apiOptions.Value.BaseUrlForMainchain, 
            apiOptions.Value.BaseUrlForSidechain, 
            apiOptions.Value.NftSeedPrivateKey, 
            apiOptions.Value.SendCount)
    {
    }
}