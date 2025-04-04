using TelegramCards.Models.DTO;

namespace TelegramCards.Managers.Interfaces;

public interface IUserManager
{
    /// <summary>
    /// Создание нового пользователя (если человека не было в системе)
    /// </summary>
    /// <param name="user">add user DTO с информацией для добавления пользователя</param>
    /// <returns>Добавленный пользователь</returns>
    Task<UserOutputDto> AddNewUserAsync(AddUserDto user);
    
    /// <summary>
    /// Получение пользователя по username
    /// </summary>
    /// <param name="username">username для поиска</param>
    /// <returns>Найденный пользователь</returns>
    Task<UserOutputDto?> GetUserByUsernameAsync(string username);
    
    /// <summary>
    /// Получить пользователя по telegram id
    /// </summary>
    /// <param name="telegramId">telegram id пользователя</param>
    /// <returns>Найденный пользователь</returns>
    Task<UserOutputDto?> GetUserByTelegramIdAsync(long telegramId);
    
    /// <summary>
    /// Изменение username пользователя, если пользователь его изменил
    /// </summary>
    /// <param name="telegramId">telegram id пользователя</param>
    /// <param name="newUsername">новый username</param>
    /// <returns>Измененный пользователь</returns>
    Task<UserOutputDto?> EditUsernameAsync(long telegramId, string newUsername);
}