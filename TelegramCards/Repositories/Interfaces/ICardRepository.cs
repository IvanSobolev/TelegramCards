namespace TelegramCards.Repositories.Interfaces;

public interface ICardRepository
{
    Task GenerateNewCardToUserAsync(long userTelegramId);
    Task GetUserCardsAsync(long ownerId);
    Task SendCardsAsync(long cardId, long newOwnerId);
}