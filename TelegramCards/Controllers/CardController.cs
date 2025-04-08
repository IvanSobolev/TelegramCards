using Microsoft.AspNetCore.Mvc;
using TelegramCards.Managers.Interfaces;
using TelegramCards.Models.DTO;

namespace TelegramCards.Controllers;

[ApiController]
[Route("card")]
public class CardController (ICardManager cardManager) : ControllerBase
{
    private readonly ICardManager _cardManager = cardManager;

    [HttpGet]
    [Route("generate/{id}")]
    public async Task<IActionResult> GenerateCardAsync(long id)
    {
        var card = await _cardManager.GenerateNewCardToUserAsync(id);
        if (card == null)
        {
            return NotFound("User not found");
        }

        return Ok(card);
    }
    
    [HttpGet]
    [Route("get/user/{id}/{page}/{pageSize}")]
    public async Task<IActionResult> GetAllUserCard(long id, int page, int pageSize)
    {
        var cards = await _cardManager.GetUserCardsAsync(id, page, pageSize);
        return Ok(cards);
    }
    
    [HttpPut]
    [Route("send")]
    public async Task<IActionResult> SendCard([FromBody] SendCardDto cardDto)
    {
        var card = await _cardManager.SendCardAsync(cardDto);
        if (card == null)
        {
            return NotFound("Card not found");
        }
        if (card.OwnerId != cardDto.NewOwnerId)
        {
            return BadRequest("The card doesn't belong to the sender");
        }

        return Ok(card);
    }
}