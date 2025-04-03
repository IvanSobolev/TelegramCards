using Microsoft.EntityFrameworkCore;
using TelegramCards.Models;
using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;
using TelegramCards.Repositories.Interfaces;

namespace TelegramCards.Repositories.implementations;

public class EfCoreUserRepository (DataContext dataContext) : IUserRepository
{
    private readonly DataContext _dataContext = dataContext;
    
    /// <inheritdoc/>
    public async Task<User> AddNewUserAsync(long telegramId, string username)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        if (user != null)
        {
            return user;
        }

        user = new User { TelegramId = telegramId, Username = username, Role = Roles.User, Cards = new List<Card>() };

        await _dataContext.Users.AddAsync(user);
        await _dataContext.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc/>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        return user;
    }

    /// <inheritdoc/>
    public async Task<User?> GetUserByTelegramIdAsync(long telegramId)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        return user;
    }

    /// <inheritdoc/>
    public async Task<User?> EditUsernameAsync(long telegramId, string newUsername)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        if (user == null)
        {
            return null;
        }
        user.Username = newUsername;
        await _dataContext.SaveChangesAsync();
        return user;
    }
}