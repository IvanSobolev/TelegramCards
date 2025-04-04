using Microsoft.AspNetCore.Mvc;
using TelegramCards.Managers.Interfaces;
using TelegramCards.Models.DTO;

namespace TelegramCards.Controllers;

[ApiController]
[Route("user")]
public class UserController(IUserManager userManager) : ControllerBase
{
    private readonly IUserManager _userManager = userManager;

    [HttpPost]
    [Route("/add")]
    public async Task<IActionResult> AddNewUserAsync([FromBody] AddUserDto userDto)
    {
        var addUser = await _userManager.AddNewUserAsync(userDto);
        return Ok(addUser);
    }

    [HttpGet]
    [Route("/get/username/{username}")]
    public async Task<IActionResult> GetUserByUsernameAsync(string username)
    {
        var user = await _userManager.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(user);
    }
    
    [HttpGet]
    [Route("/get/id/{telegramId}")]
    public async Task<IActionResult> GetUserByTelegramIdAsync(long telegramId)
    {
        var user = await _userManager.GetUserByTelegramIdAsync(telegramId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(user);
    }

    [HttpPut]
    [Route("/edit/username/{telegramId}/{newUsername}")]
    public async Task<IActionResult> EditUsernameAsync(long telegramId, string newUsername)
    {
        var user = await _userManager.EditUsernameAsync(telegramId, newUsername);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(user);
    }
    
}