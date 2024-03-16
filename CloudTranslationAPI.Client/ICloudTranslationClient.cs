namespace CloudTranslationAPI.Client;

public interface ICloudTranslationClient : IDisposable
{
    public Task<TranslationResponse> TranslateAsync(TranslationRequest request);
}