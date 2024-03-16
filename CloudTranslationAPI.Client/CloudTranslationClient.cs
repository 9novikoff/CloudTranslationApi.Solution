using System.Net.Http.Json;
using System.Runtime.Serialization;

namespace CloudTranslationAPI.Client;

public class CloudTranslationClient : ICloudTranslationClient
{
    private readonly string _authorizationToken;
    private readonly string _projectId;
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://translation.googleapis.com/language/translate/v2";

    public CloudTranslationClient(string authorizationToken, string projectId)
    {
        _authorizationToken = authorizationToken;
        _projectId = projectId;
        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("Authorization", authorizationToken);
        _httpClient.DefaultRequestHeaders.Add("x-goog-user-project", projectId);
    }

    public async Task<TranslationResponse> TranslateAsync(TranslationRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiUrl, request);
        response.EnsureSuccessStatusCode();
        
        var translationResponse = await response.Content.ReadFromJsonAsync<TranslationResponse>();

        if (translationResponse == null)
        {
            throw new SerializationException();
        }

        return translationResponse;
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing) {
            _httpClient.Dispose();
        }
    }
    
}