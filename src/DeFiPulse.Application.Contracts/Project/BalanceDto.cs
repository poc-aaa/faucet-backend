using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeFiPulse.Project;

public class BalanceDto
{
    [JsonPropertyName("msg")]
    public string Message { get; set; }
    
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("data")]
    public List<TokenBalanceDto> Data { get; set; }
}

public class TokenBalanceDto
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
    
    [JsonPropertyName("balance")]
    public string Balance { get; set; }
}