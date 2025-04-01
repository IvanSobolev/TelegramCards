namespace TelegramCards.Repositories.Interfaces;

public interface IUserRepository
{
    Task AddNewUserAsync(long telegramId, string username);
    Task EditUsernameAsync(long telegramId, string newUsername);
    Task GetUserByUsernameAsync(string username);
}