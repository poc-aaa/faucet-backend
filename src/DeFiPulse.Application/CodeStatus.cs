namespace DeFiPulse
{
    public enum CodeStatus
    {
        Success = 1, //成功
        HadReceived = 2, //已领取过
        InvalidAddress = 3, //无效地址
        BalanceNotAdequate = 4, //余额不足
        SystemError = 5 //系统错误
    }
}