namespace TelegramBot.Services.Interfaces;

public interface IRequestService
{
    Task<T?> GetRequestAsync<T>(string url);
    Task<T?> PostRequestAsync<T, TB>(string url, TB body);
    Task<T?> PutRequestAsync<T, TB>(string url, TB body);
    Task<T?> DeleteRequestAsync<T>(string url);
}