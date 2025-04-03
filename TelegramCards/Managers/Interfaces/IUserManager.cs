using TelegramCards.Models.DTO;

namespace TelegramCards.Managers.Interfaces;

public interface IUserManager
{
    Task<UserOutputDto> AddNewUserAsync(AddUserDto user);
    Task<UserOutputDto?> GetUserByUsernameAsync(string username);
    Task<UserOutputDto?> GetUserByTelegramIdAsync(long telegramId);
    Task<UserOutputDto?> EditUsernameAsync(long telegramId, string newUsername);
}