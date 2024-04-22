using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace DeFiPulse.Project;

public interface ICoinSendContractService : ISingletonDependency
{
    Task<MessageResult> CheckBalanceAsync(ChainType chainType);
    Task<MessageResult> SendTokensAsync(string walletAddress, ChainType chainType);
    Task<MessageResult> SendSeedAsync(string walletAddress, string tokenSymbol);
    Task<List<string>> GetSeedList();
}