namespace TelegramBot.Entity.Dtos;

public class ApiResult<T>
{ 
    public bool Success { get; set; } 
    public T? Data { get; set; } 
    public string? ErrorMessage { get; set; } 
    public int StatusCode { get; set; }
    
    public static ApiResult<T> FromSuccess(T data, int statusCode) => new()
    {
        Success = true,
        Data = data,
        StatusCode = statusCode
    };

    public static ApiResult<T> FromError(string errorMessage, int statusCode) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        StatusCode = statusCode
    };
}