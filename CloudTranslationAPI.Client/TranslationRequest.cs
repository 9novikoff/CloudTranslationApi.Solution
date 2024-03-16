using Newtonsoft.Json;

namespace CloudTranslationAPI.Client;

public class TranslationRequest(string q, string target, string? format = null, string? source = null)
{
    [JsonProperty("q")]
    public string Q { get; set; } = q;
    [JsonProperty("target")]
    public string Target { get; set; } = target;
    [JsonProperty("format")]
    public string? Format { get; set; } = format;
    [JsonProperty("source")]
    public string? Source { get; set; } = source;
}