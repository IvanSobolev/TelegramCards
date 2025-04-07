using TelegramBot.ApiRepository.Interfaces;
using TelegramBot.Entity.Dtos;
using TelegramBot.Services.Interfaces;

namespace TelegramBot.ApiRepository.Implementations;

public class RequestServiceUserRepository(IRequestService requestService, string baseApiUrl) : IUserRepository
{
    private readonly IRequestService _requestService = requestService;
    private readonly string _baseApiUrl = baseApiUrl;
    
    public async Task<UserOutputDto?> AddNewUserAsync(AddNewUserDto user)
    {
        ApiResult<UserOutputDto> card = 
            await _requestService.PostRequestAsync<UserOutputDto, AddNewUserDto>($"{_baseApiUrl}/user/add", user);
        if (!card.Success)
        {
            Console.WriteLine(card.StatusCode + " " + card.ErrorMessage);
            return null;
        }

        return card.Data;
    }

    public async Task<UserOutputDto?> GetUserByUsernameAsync(string username)
    {
        ApiResult<UserOutputDto> card = 
            await _requestService.GetRequestAsync<UserOutputDto>($"{_baseApiUrl}/user/get/username/{username}");
        if (!card.Success)
        {
            Console.WriteLine(card.StatusCode + " " + card.ErrorMessage);
            return null;
        }

        return card.Data;
    }

    public async Task<UserOutputDto?> GetUserByTelegramIdAsync(long telegramId)
    {
        ApiResult<UserOutputDto> card = 
            await _requestService.GetRequestAsync<UserOutputDto>($"{_baseApiUrl}/user/get/id/{telegramId}");
        if (!card.Success)
        {
            Console.WriteLine(card.StatusCode + " " + card.ErrorMessage);
            return null;
        }

        return card.Data;
    }

    public async Task<UserOutputDto?> EditUsernameAsync(long telegramId, string newUsername)
    {
        ApiResult<UserOutputDto> card = 
            await _requestService.GetRequestAsync<UserOutputDto>($"{_baseApiUrl}/user/edit/username/{telegramId}/{newUsername}");
        if (!card.Success)
        {
            Console.WriteLine(card.StatusCode + " " + card.ErrorMessage);
            return null;
        }

        return card.Data;
    }
}