using System.Text;
using System.Text.Json;
using TelegramBot.Entity.Dtos;
using TelegramBot.Services.Interfaces;

namespace TelegramBot.Services.Implementations;

public class HttpClientRequestService(HttpClient? httpClient = null) : IRequestService
{
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();
    
    public async Task<ApiResult<T>> GetRequestAsync<T>(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResult<T>.FromError($"GET error: {ex.Message}", 0);
        }
    }

    public async Task<ApiResult<T>> PostRequestAsync<T, TB>(string url, TB body)
    {
        try
        {
            var content = SerializeRequest(body);
            var response = await _httpClient.PostAsync(url, content);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResult<T>.FromError($"POST error: {ex.Message}", 0);
        }
    }

    public async Task<ApiResult<T>> PutRequestAsync<T, TB>(string url, TB body)
    {
        try
        {
            var content = SerializeRequest(body);
            var response = await _httpClient.PutAsync(url, content);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResult<T>.FromError($"PUT error: {ex.Message}", 0);
        }
    }

    public async Task<ApiResult<T>> DeleteRequestAsync<T>(string url)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(url);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResult<T>.FromError($"DELETE error: {ex.Message}", 0);
        }
    }
    
    
    private static StringContent SerializeRequest<T>(T body)
    {
        var json = JsonSerializer.Serialize(body);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
    
    private static async Task<ApiResult<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var data = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return ApiResult<T>.FromSuccess(data!, (int)response.StatusCode);
            }
            catch (JsonException)
            {
                return ApiResult<T>.FromError("Ошибка десериализации ответа", (int)response.StatusCode);
            }
        }

        return ApiResult<T>.FromError($"{content}", (int)response.StatusCode);
    }
}