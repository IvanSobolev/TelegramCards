using TelegramCards.Models.Entitys;

namespace TelegramCards.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User> AddNewUserAsync(long telegramId, string username);
    Task<User> GetUserByUsernameAsync(string username);
    Task<User> GetUserByTelegramIdAsync(long telegramId);
    Task<User> EditUsernameAsync(long telegramId, string newUsername);
}