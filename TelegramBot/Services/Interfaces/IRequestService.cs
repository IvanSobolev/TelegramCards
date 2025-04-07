using TelegramBot.Entity.Dtos;

namespace TelegramBot.Services.Interfaces;

public interface IRequestService
{
    Task<ApiResult<T>> GetRequestAsync<T>(string url);
    Task<ApiResult<T>> PostRequestAsync<T, TB>(string url, TB body);
    Task<ApiResult<T>> PutRequestAsync<T, TB>(string url, TB body);
    Task<ApiResult<T>> DeleteRequestAsync<T>(string url);
}