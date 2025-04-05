using Microsoft.EntityFrameworkCore;
using TelegramCards.Models;
using TelegramCards.Models.DTO;
using TelegramCards.Models.Entitys;
using TelegramCards.Repositories.Interfaces;
using TelegramCards.Services.interfaces;

namespace TelegramCards.Repositories.implementations;

public class EfCoreCardRepository(DataContext dataContext, ICardBaseGeneratorService generatorService) : ICardRepository
{
    private readonly DataContext _dataContext = dataContext;
    private readonly ICardBaseGeneratorService _cardBaseGenerator = generatorService;
    
    /// <inheritdoc/>
    public async Task<CardOutputDto?> GenerateNewCardToUserAsync(long userTelegramId)
    {
        User? user = await _dataContext.Users.FirstOrDefaultAsync(u => u.TelegramId == userTelegramId);
        if (user == null || (DateTime.UtcNow - user.LastTakeCard) >= TimeSpan.FromHours(4))
        {
            return null;
        }
        
        CardOutputDto newCardOuntput = await _cardBaseGenerator.GetNewCardInRandomCardStackAsync();
        newCardOuntput.OwnerId = userTelegramId;
        
        Card newCard = new Card
            { 
                CardBaseId = newCardOuntput.CardBaseId,
                OwnerId = userTelegramId, 
                GenerationDate = DateTime.UtcNow,
                ReceivedCard = DateTime.UtcNow
            };

        user.LastTakeCard = DateTime.UtcNow;
        _dataContext.Cards.Add(newCard);
        await _dataContext.SaveChangesAsync();

        return newCardOuntput;
    }

    /// <inheritdoc/>
    public async Task<(ICollection<CardOutputDto> cards, int pageCount)> GetUserCardsAsync(long ownerId, int page, int pageSize)
    {
        if (page < 1 || pageSize < 1)
        {
            return (new List<CardOutputDto>(), 0);
        }
        var query = _dataContext.Cards.AsQueryable()
                                                            .Select(c => new CardOutputDto
                                                            {
                                                                Id = c.Id,
                                                                OwnerId = c.OwnerId,
                                                                CardBaseId = c.CardBaseId,
                                                                CardPhotoUrl = c.BaseCard.CardPhotoUrl,
                                                                Points = c.BaseCard.Points,
                                                                GenerationDate = c.GenerationDate,
                                                                ReceivedCard = c.ReceivedCard
                                                            });
        
        query = query.Where(c => c.OwnerId == ownerId);
        var cards = await query.Skip(pageSize * (page - 1))
                                        .Take(pageSize)
                                        .ToListAsync();

        int cardCount = await query.CountAsync();

        return (cards, (cardCount + pageSize - 1) / pageSize);
    }

    /// <inheritdoc/>
    public async Task<CardOutputDto?> SendCardAsync(long senderId, long newOwnerId, long cardId)
    {
        var card = await _dataContext.Cards.Select(c => new CardOutputDto
            {
                Id = c.Id,
                OwnerId = c.OwnerId,
                CardBaseId = c.CardBaseId,
                CardPhotoUrl = c.BaseCard.CardPhotoUrl,
                Points = c.BaseCard.Points,
                GenerationDate = c.GenerationDate,
                ReceivedCard = c.ReceivedCard
            })
            .FirstOrDefaultAsync(c => c.Id == cardId);
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