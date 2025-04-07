using System.Text;
using System.Text.Json;
using TelegramBot.Services.Interfaces;

namespace TelegramBot.Services.Implementations;

public class HttpClientRequestService(HttpClient? httpClient = null) : IRequestService
{
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();
    
    public async Task<T?> GetRequestAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await DeserializeResponse<T>(response);
    }

    public async Task<T?> PostRequestAsync<T, TB>(string url, TB body)
    {
        var content = SerializeRequest(body);
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await DeserializeResponse<T>(response);
    }

    public async Task<T?> PutRequestAsync<T, TB>(string url, TB body)
    {
        var content = SerializeRequest(body);
        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await DeserializeResponse<T>(response);
    }

    public async Task<T?> DeleteRequestAsync<T>(string url)
    {
        var response = await _httpClient.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
        return await DeserializeResponse<T>(response);
    }
    
    private static StringContent SerializeRequest<T>(T body)
    {
        var json = JsonSerializer.Serialize(body);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}