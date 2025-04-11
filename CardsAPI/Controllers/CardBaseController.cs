using Microsoft.AspNetCore.Mvc;
using TelegramCards.Managers.Interfaces;
using TelegramCards.Models.DTO;

namespace TelegramCards.Controllers;

[ApiController]
[Route("cardbase")]
public class CardBaseController (ICardBaseManager cardBaseManager ) : ControllerBase
{
    private readonly ICardBaseManager _cardBaseManager = cardBaseManager;
    
    [HttpPost]
    [Route("add")]
    public async Task<IActionResult> AddNewCardBaseAsync([FromForm] AddCardBaseDto cardBaseDto)
    {
        var cardBase = await _cardBaseManager.AddNewCardBaseAsync(cardBaseDto);
        if (cardBase == null)
        {
            return NotFound("Admin is not found");
        }

        return Ok(cardBase);
    }

    [HttpGet]
    [Route("get/{adminId}/{page}/{pageSize}")]
    public async Task<IActionResult> GetCardBasesAsync(long adminId, int page, int pageSize)
    {
        var cardBases = await _cardBaseManager.GetCardBasesAsync(adminId, page, pageSize);
        return Ok(cardBases);
    }
}