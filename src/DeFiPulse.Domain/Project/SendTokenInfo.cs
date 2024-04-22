using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace DeFiPulse.Project
{
    public class SendTokenInfo: AuditedAggregateRoot<string>
    {
        public string WalletAddress { get; set; }

        public int SendCoinValue { get; set; }

        public bool IsSentToken { get; set; }

        public bool IsSentSeed { get; set; }

        public void SetId(string id)
        {
            Id = id.ToLower();
        }
        public static string FormatId(string id)
        {
            return id.ToLower();
        }
    }
}