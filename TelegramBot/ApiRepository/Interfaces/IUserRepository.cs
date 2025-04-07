using TelegramBot.Entity.Dtos;

namespace TelegramBot.ApiRepository.Interfaces;

public interface IUserRepository
{
    Task<UserOutputDto> AddNewUserAsync(AddNewUserDto user);
    Task<UserOutputDto> GetUserByUsernameAsync(string username);
    Task<UserOutputDto> GetUserByTelegramIdAsync(long telegramId);
    Task<UserOutputDto> EditUsernameAsync(long telegramId, string newUsername);
}