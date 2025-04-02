using Microsoft.EntityFrameworkCore;
using TelegramCards.Models;
using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;
using TelegramCards.Repositories.Interfaces;
using TelegramCards.Services.interfaces;

namespace TelegramCards.Repositories.implementations;

public class EfCoreCardRepository(DataContext dataContext, ICardBaseGeneratorService generatorService) : ICardRepository
{
    private readonly DataContext _dataContext = dataContext;
    private readonly ICardBaseGeneratorService _cardBaseGenerator = generatorService;
    
    public async Task<Card> GenerateNewCardToUserAsync(long userTelegramId)
    {
        Rarity rarity = await _cardBaseGenerator.GetRandomRarityAsync();
        Card newCard = new Card
            { 
                OwnerId = userTelegramId, 
                RarityLevel = rarity,
                CardIndex = await _cardBaseGenerator.GetRandomCardIndexByRarityAsync(rarity),
                GenerationDate = DateTime.UtcNow,
                ReceivedCard = DateTime.UtcNow
            };

        _dataContext.Cards.Add(newCard);
        await _dataContext.SaveChangesAsync();

        return newCard;
    }

    public async Task<(ICollection<Card> cards, int pageCount)> GetUserCardsAsync(long ownerId, int page, int pageSize)
    {
        if (page < 1 || pageSize < 1)
        {
            return (new List<Card>(), 0);
        }
        var query = _dataContext.Cards.AsQueryable();
        query = query.Where(c => c.OwnerId == ownerId);
        var cards = await query.Skip(pageSize * (page - 1))
                                        .Take(pageSize)
                                        .ToListAsync();

        int cardCount = await query.CountAsync();

        return (cards, (cardCount + pageSize - 1) / pageSize);
    }

    public async Task<Card?> SendCardAsync(long senderId, long newOwnerId, long cardId)
    {
        var card = await _dataContext.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
        if (card == null)
        {
            return null;
        }
        if (card.OwnerId != senderId)
        {
            return card;
        }

        card.OwnerId = newOwnerId;
        await _dataContext.SaveChangesAsync();
        return card;
    }
}