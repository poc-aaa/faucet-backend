namespace DeFiPulse;

public class ApiConfigOptions
{
    public string BaseUrl { get; set; } = "13.211.28.67:8000";
    public string BaseUrlForMainchain { get; set; }
    public string BaseUrlForSidechain { get; set; }
    public string PrivateKey { get; set; }
    public int SendCount { get; set; }
    public string NftSeedPrivateKey { get; set; }
}