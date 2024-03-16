using Newtonsoft.Json;

namespace CloudTranslationAPI.Client;

public class TranslationResponse
{
    [JsonProperty("data")]
    public TranslationResponseData Data { get; set; } = null!;
}

public class TranslationResponseData
{
    [JsonProperty("translations")]
    public List<Translation> Translations { get; set; }= null!;
}

public class Translation
{
    [JsonProperty("detectedSourceLanguage")]
    public string? DetectedSourceLanguage { get; set; }
    [JsonProperty("model")]
    public string Model { get; set; } = null!;
    [JsonProperty("translatedText")]
    public string TranslatedText { get; set; } = null!;
}