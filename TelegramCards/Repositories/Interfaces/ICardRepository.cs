using TelegramCards.Models.DTO;
using TelegramCards.Models.Entitys;

namespace TelegramCards.Repositories.Interfaces;

public interface ICardRepository
{
    Task<CardOutputDto?> GenerateNewCardToUserAsync(long userTelegramId);
    Task<(ICollection<CardOutputDto> cards, int pageCount)> GetUserCardsAsync(long ownerId, int page, int pageSize);
    Task<CardOutputDto?> SendCardAsync(long senderId, long newOwnerId, long cardId);
}