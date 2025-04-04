using TelegramCards.Managers.Interfaces;
using TelegramCards.Models.DTO;
using TelegramCards.Models.Entitys;
using TelegramCards.Repositories.Interfaces;

namespace TelegramCards.Managers.Implementations;

public class UserManager (IUserRepository userRepository) : IUserManager
{
    private readonly IUserRepository _userRepository = userRepository;
    
    /// <inheritdoc/>
    public async Task<UserOutputDto> AddNewUserAsync(AddUserDto user)
    {
        User newUser = await _userRepository.AddNewUserAsync(user.TelegramId, user.Username);
        return new UserOutputDto
        {
            TelegramId = newUser.TelegramId,
            Username = newUser.Username,
            Role = newUser.Role,
            LastTakeCard = newUser.LastTakeCard
        };
    }

    /// <inheritdoc/>
    public async Task<UserOutputDto?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetUserByUsernameAsync(username);
    }

    public async Task<UserOutputDto?> GetUserByTelegramIdAsync(long telegramId)
    {
        return await _userRepository.GetUserByTelegramIdAsync(telegramId);
    }

    /// <inheritdoc/>
    public async Task<UserOutputDto?> EditUsernameAsync(long telegramId, string newUsername)
    {
        User? user = await _userRepository.EditUsernameAsync(telegramId, newUsername);
        if (user == null)
        {
            return null;
        }
        return new UserOutputDto
        {
            TelegramId = user.TelegramId,
            Username = user.Username,
            Role = user.Role,
            LastTakeCard = user.LastTakeCard
        };
    }
}