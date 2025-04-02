using TelegramCards.Models.Entitys;

namespace TelegramCards.Repositories.Interfaces;

public interface ICardRepository
{
    Task<Card> GenerateNewCardToUserAsync(long userTelegramId);
    Task<ICollection<Card>> GetUserCardsAsync(long ownerId, int page, int pageSize);
    Task<Card> SendCardAsync(long senderId, long newOwnerId, long cardId);
}