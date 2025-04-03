using TelegramCards.Models.DTO;

namespace TelegramCards.Managers.Interfaces;

public interface ICardManager
{
    Task<CardOutputDto?> GenerateNewCardToUserAsync(long userTelegramId);
    Task<(ICollection<CardOutputDto> cards, int pageCount)> GetUserCardsAsync(long ownerId, int page, int pageSize);
    Task<CardOutputDto?> SendCardAsync(SendCardDto cardDto);
}