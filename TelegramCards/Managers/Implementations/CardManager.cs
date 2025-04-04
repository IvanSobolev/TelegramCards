using TelegramCards.Managers.Interfaces;
using TelegramCards.Models.DTO;
using TelegramCards.Repositories.Interfaces;

namespace TelegramCards.Managers.Implementations;

public class CardManager(ICardRepository cardRepository) : ICardManager
{
    private readonly ICardRepository _cardRepository = cardRepository;
    
    /// <inheritdoc/>
    public async Task<CardOutputDto?> GenerateNewCardToUserAsync(long userTelegramId)
    {
        return await _cardRepository.GenerateNewCardToUserAsync(userTelegramId);
    }

    /// <inheritdoc/>
    public async Task<(ICollection<CardOutputDto> cards, int pageCount)> GetUserCardsAsync(long ownerId, int page, int pageSize)
    {
        return await _cardRepository.GetUserCardsAsync(ownerId, page, pageSize);
    }

    /// <inheritdoc/>
    public async Task<CardOutputDto?> SendCardAsync(SendCardDto cardDto)
    {
        return await _cardRepository.SendCardAsync(cardDto.SenderId, cardDto.NewOwnerId, cardDto.CardId);
    }
}