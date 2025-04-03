using TelegramCards.Models.Entitys;

namespace TelegramCards.Repositories.Interfaces;

public interface IUserRepository
{
    /// <summary>
    /// Создание нового пользователя (если человека не было в системе)
    /// </summary>
    /// <param name="telegramId"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    Task<User> AddNewUserAsync(long telegramId, string username);
    
    /// <summary>
    /// Получение пользователя по username
    /// </summary>
    /// <param name="username">username lkz gjbcrf</param>
    /// <returns>Найденного пользователя</returns>
    Task<User?> GetUserByUsernameAsync(string username);
    
    /// <summary>
    /// Получить пользователя по telegram id
    /// </summary>
    /// <param name="telegramId">telegram id пользователя</param>
    /// <returns>Найденный пользователь</returns>
    Task<User?> GetUserByTelegramIdAsync(long telegramId);
    
    /// <summary>
    /// Изменение username пользователя, если пользователь его изменил
    /// </summary>
    /// <param name="telegramId">telegram id пользователя</param>
    /// <param name="newUsername">новый username</param>
    /// <returns>Измененный пользователь</returns>
    Task<User?> EditUsernameAsync(long telegramId, string newUsername);
}