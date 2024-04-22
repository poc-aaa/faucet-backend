using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf;
using AElf.Client;
using AElf.Client.Dto;
using AElf.Client.MultiToken;
using Google.Protobuf;
using Address = AElf.Types.Address;
using Microsoft.Extensions.Options;
using RestSharp;

namespace DeFiPulse.Project;

public class CoinSendContractService : ICoinSendContractService
{
    private string BaseUrlForMainchain;
    private string BaseUrlForSidechain;
    private string PrivateKey;
    private string _address;
    private AElfClient ClientForMainchain { get; }
    private AElfClient ClientForSidechain { get; }
    private readonly ApiConfigOptions _apiConfig;

    public CoinSendContractService(IOptionsSnapshot<ApiConfigOptions> apiOptions)
    {
        _apiConfig = apiOptions.Value;
        BaseUrlForMainchain = string.IsNullOrEmpty(_apiConfig.BaseUrlForMainchain)
            ? _apiConfig.BaseUrl
            : _apiConfig.BaseUrlForMainchain;
        BaseUrlForSidechain = _apiConfig.BaseUrlForSidechain;
        PrivateKey = _apiConfig.PrivateKey;
        ClientForMainchain = new AElfClient(BaseUrlForMainchain);
        ClientForSidechain = new AElfClient(BaseUrlForSidechain);
        _address = ClientForMainchain.GetAddressFromPrivateKey(PrivateKey);
    }

    public async Task<MessageResult> CheckBalanceAsync(ChainType chainType)
    {
        var messageResult = new MessageResult();
        try
        {
            // Now we check both MainChain and SideChain.
            messageResult = await CheckFaucetAccountBalanceAsync(ClientForMainchain);
            if (messageResult.IsSuccess)
            {
                messageResult = await CheckFaucetAccountBalanceAsync(ClientForSidechain);
            }
        }
        catch (Exception ex)
        {
            messageResult.IsSuccess = false;
            messageResult.Code = Convert.ToInt32(CodeStatus.SystemError);
            messageResult.Message = $"Insufficient balance on faucet account: {ex.Message}";
        }

        return messageResult;
    }

    private async Task<MessageResult> CheckFaucetAccountBalanceAsync(AElfClient client)
    {
        var messageResult = new MessageResult();
        var tokenContractAddress =
            await client.GetContractAddressByNameAsync(
                HashHelper.ComputeFrom("AElf.ContractNames.Token"));
        var getBalanceInput = new GetBalanceInput
        {
            Symbol = "ELF",
            Owner = new AElf.Client.Proto.Address { Value = Address.FromBase58(_address).Value }
        };

        var transactionGetBalance =
            await client.GenerateTransactionAsync(_address, tokenContractAddress.ToBase58(), "GetBalance",
                getBalanceInput);
        var txWithSignGetBalance = client.SignTransaction(PrivateKey, transactionGetBalance);
        var transactionGetBalanceResult = await client.ExecuteTransactionAsync(new ExecuteTransactionDto
        {
            RawTransaction = txWithSignGetBalance.ToByteArray().ToHex()
        });
        var balance = GetBalanceOutput.Parser.ParseFrom(
            ByteArrayHelper.HexStringToByteArray(transactionGetBalanceResult));

        if (balance.Balance < 100_00000000)
        {
            messageResult.IsSuccess = false;
            messageResult.Code = Convert.ToInt32(CodeStatus.BalanceNotAdequate);
            messageResult.Message = "Faucet account balance is not enough.";
        }

        return messageResult;
    }

    public async Task<MessageResult> SendTokensAsync(string walletAddress, ChainType chainType)
    {
        var messageResult = new MessageResult();
        try
        {
            // messageResult = await SendTokensToUserAsync(chainType == ChainType.Mainchain
            //     ? ClientForMainchain
            //     : ClientForSidechain, walletAddress);

            // Now we send tokens to user on both MainChain and SideChain
            messageResult = await SendTokensToUserAsync(ClientForSidechain, walletAddress);
            await SendTokensToUserAsync(ClientForMainchain, walletAddress);
        }
        catch (Exception ex)
        {
            messageResult.IsSuccess = false;
            messageResult.Code = Convert.ToInt32(CodeStatus.SystemError);
            messageResult.Message = $"Failed to send tokens to user: {ex.Message}";
        }

        return messageResult;
    }

    public async Task<MessageResult> SendSeedAsync(string walletAddress, string tokenSymbol)
    {
        var messageResult = new MessageResult();
        try
        {
            messageResult = await SendSeedTokenToUserAsync(ClientForMainchain, walletAddress, tokenSymbol);
        }
        catch (Exception ex)
        {
            messageResult.IsSuccess = false;
            messageResult.Code = Convert.ToInt32(CodeStatus.SystemError);
            messageResult.Message = $"Failed to send seed token to user: {ex.Message}";
        }

        return messageResult;
    }

    public async Task<List<string>> GetSeedList()
    {
        var options = new RestClientOptions("https://explorer-test.aelf.io/api/viewer");
        var client = new RestClient(options);
        var request = new RestRequest($"balances?address={_address}");
        var balance = await client.GetAsync<BalanceDto>(request);
        return balance.Data.Select(d => d.Symbol).ToList();
    }

    private async Task<MessageResult> SendTokensToUserAsync(AElfClient client, string walletAddress)
    {
        var messageResult = new MessageResult();

        var toAddress =
            await client.GetContractAddressByNameAsync(
                HashHelper.ComputeFrom("AElf.ContractNames.Token"));

        var param = new TransferInput
        {
            To = new AElf.Client.Proto.Address { Value = Address.FromBase58(walletAddress).Value },
            Symbol = "ELF",
            Amount = _apiConfig.SendCount * 100000000L
        };

        var transaction =
            await client.GenerateTransactionAsync(_address, toAddress.ToBase58(), "Transfer", param);
        var txWithSign = client.SignTransaction(PrivateKey, transaction);

        var result = await client.SendTransactionAsync(new SendTransactionInput
        {
            RawTransaction = txWithSign.ToByteArray().ToHex()
        });
        messageResult.Message = result.TransactionId;
        return messageResult;
    }

    private async Task<MessageResult> SendSeedTokenToUserAsync(AElfClient client, string walletAddress,
        string tokenSymbol)
    {
        var messageResult = new MessageResult();

        var toAddress =
            await client.GetContractAddressByNameAsync(
                HashHelper.ComputeFrom("AElf.ContractNames.Token"));

        var param = new TransferInput
        {
            To = new AElf.Client.Proto.Address { Value = Address.FromBase58(walletAddress).Value },
            Symbol = tokenSymbol,
            Amount = 1
        };

        var transaction =
            await client.GenerateTransactionAsync(_address, toAddress.ToBase58(), "Transfer", param);
        var txWithSign = client.SignTransaction(PrivateKey, transaction);

        var result = await client.SendTransactionAsync(new SendTransactionInput
        {
            RawTransaction = txWithSign.ToByteArray().ToHex()
        });
        messageResult.Message = result.TransactionId;
        return messageResult;
    }
}