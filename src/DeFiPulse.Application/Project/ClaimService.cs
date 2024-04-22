using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DeFiPulse.Project;

public class ClaimService : DeFiPulseAppService, IClaimService
{
    private readonly ISendTokenInfoRepository _sendTokenInfoRepository;
    private readonly ICoinSendContractService _coinSendContractService;

    public ClaimService(ISendTokenInfoRepository sendTokenInfoRepository,
        ICoinSendContractService contractService)
    {
        _sendTokenInfoRepository = sendTokenInfoRepository;
        _coinSendContractService = contractService;
    }

    [Route("api/claim")]
    public async Task<MessageResult> ClaimTokenAsync(string walletAddress)
    {
        var messageResult = new MessageResult();

        if (string.IsNullOrEmpty(walletAddress))
        {
            messageResult.IsSuccess = false;
            messageResult.Code = Convert.ToInt32(CodeStatus.InvalidAddress);
            messageResult.Message = "Incorrect address formant.";
            return messageResult;
        }

        var chainType = ChainType.Mainchain;
        if (walletAddress.Contains("ELF_"))
        {
            if (walletAddress.Split('_')[2] != "AELF")
            {
                chainType = ChainType.Sidechain;
            }

            walletAddress = walletAddress.Split('_')[1];
        }

        var gotTokenBefore = await _sendTokenInfoRepository.GetAsync(walletAddress);
        if (gotTokenBefore is { IsSentToken: true })
        {
            messageResult.IsSuccess = false;
            messageResult.Message = $"You have received the test tokens and cannot receive it again";
            messageResult.Code = Convert.ToInt32(CodeStatus.HadReceived);
            return messageResult;
        }

        var checkBalanceMessageResult = await _coinSendContractService.CheckBalanceAsync(chainType);
        if (!checkBalanceMessageResult.IsSuccess)
        {
            return checkBalanceMessageResult;
        }

        var sendTokenMessageResult = await _coinSendContractService.SendTokensAsync(walletAddress, chainType);
        if (!sendTokenMessageResult.IsSuccess)
        {
            return sendTokenMessageResult;
        }

        messageResult.Message = sendTokenMessageResult.Message;

        try
        {
            var sendTokenInfo = await _sendTokenInfoRepository.GetAsync(walletAddress.ToLower());
            if (sendTokenInfo == null)
            {
                sendTokenInfo = new SendTokenInfo
                {
                    WalletAddress = walletAddress,
                    SendCoinValue = 2000
                };
                sendTokenInfo.SetId(walletAddress);
            }

            sendTokenInfo.IsSentToken = true;

            var result = await _sendTokenInfoRepository.InsertAsync(sendTokenInfo);
            if (result == null)
            {
                messageResult.IsSuccess = false;
                messageResult.Message = $"System error: Failed to insert send token info.";
                messageResult.Code = Convert.ToInt32(CodeStatus.SystemError);
            }
        }
        catch (Exception ex)
        {
            messageResult.IsSuccess = false;
            messageResult.Message = $"System error: {ex.Message}";
            messageResult.Code = Convert.ToInt32(CodeStatus.SystemError);
        }

        return messageResult;
    }

    [HttpPost("api/claim-seed")]
    public async Task<MessageResult> ClaimSeedAsync(string walletAddress)
    {
        var messageResult = new MessageResult();

        var seedList = await _coinSendContractService.GetSeedList();
        var seedTokenSymbol = seedList.FirstOrDefault(s => s.Contains("SEED"));
        if (seedTokenSymbol == null)
        {
            messageResult.IsSuccess = false;
            messageResult.Code = Convert.ToInt32(CodeStatus.SystemError);
            messageResult.Message = "Failed to get seed token.";
            return messageResult;
        }

        var gotTokenBefore = await _sendTokenInfoRepository.GetAsync(walletAddress);
        if (gotTokenBefore is { IsSentSeed: true })
        {
            messageResult.IsSuccess = false;
            messageResult.Message = $"You have received the seed and cannot receive it again";
            messageResult.Code = Convert.ToInt32(CodeStatus.HadReceived);
            return messageResult;
        }

        var sendTokenMessageResult = await _coinSendContractService.SendSeedAsync(walletAddress, seedTokenSymbol);
        if (!sendTokenMessageResult.IsSuccess)
        {
            return sendTokenMessageResult;
        }

        try
        {
            var sendTokenInfo = await _sendTokenInfoRepository.GetAsync(walletAddress.ToLower());
            if (sendTokenInfo == null)
            {
                sendTokenInfo = new SendTokenInfo
                {
                    WalletAddress = walletAddress,
                    SendCoinValue = 2000
                };
                sendTokenInfo.SetId(walletAddress);
            }

            sendTokenInfo.IsSentSeed = true;

            var result = await _sendTokenInfoRepository.InsertAsync(sendTokenInfo);
            if (result == null)
            {
                messageResult.IsSuccess = false;
                messageResult.Message = $"System error: Failed to insert send token info.";
                messageResult.Code = Convert.ToInt32(CodeStatus.SystemError);
            }
        }
        catch (Exception ex)
        {
            messageResult.IsSuccess = false;
            messageResult.Message = $"System error: {ex.Message}";
            messageResult.Code = Convert.ToInt32(CodeStatus.SystemError);
        }

        messageResult.Message = sendTokenMessageResult.Message;

        return messageResult;
    }
}