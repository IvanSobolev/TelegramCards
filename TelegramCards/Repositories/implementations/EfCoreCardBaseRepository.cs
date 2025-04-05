using Microsoft.EntityFrameworkCore;
using TelegramCards.Models;
using TelegramCards.Models.DTO;
using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;
using TelegramCards.Repositories.Interfaces;

namespace TelegramCards.Repositories.implementations;

public class EfCoreCardBaseRepository (DataContext dataContext) : ICardBaseRepository
{
    private readonly DataContext _dataContext = dataContext;
    
    /// <inheritdoc/>
    public async Task<CardBase?> AddNewCardBaseAsync(long adminId, Rarity rarity, string photoUrl, int pointsNumber)
    {
        User? user = await _dataContext.Users.FirstOrDefaultAsync(u => u.TelegramId == adminId);
        if (user == null || user.Role != Roles.Admin)
        {
            return null;
        }
        CardBase newCardBase = new CardBase { RarityLevel = rarity, CardPhotoUrl = photoUrl, Points = pointsNumber };

        _dataContext.CardBases.Add(newCardBase);
        await _dataContext.SaveChangesAsync();

        return newCardBase;
    }

    /// <inheritdoc/>
    public async Task<(ICollection<CardBaseOutputDto> cardBases, int PageCount)> GetCardBasesAsync(long adminId, int page, int pageSize)
    {
        if (page < 1 || pageSize < 1)
        {
            return (new List<CardBaseOutputDto>(), 0);
        }
        User? user = await _dataContext.Users.FirstOrDefaultAsync(u => u.TelegramId == adminId);
        if (user == null || user.Role != Roles.Admin)
        {
            return (new List<CardBaseOutputDto>(), 0);
        }

        var query = _dataContext.CardBases.Select(cb => new CardBaseOutputDto()
                                                                {
                                                                    Id = cb.Id,
                                                                    RarityLevel = cb.RarityLevel,
                                                                    CardPhotoUrl = cb.CardPhotoUrl,
                                                                    Points = cb.Points
                                                                })
                                                                .AsQueryable();

        List<CardBaseOutputDto> cardBases = await query.Skip(pageSize * (page - 1))
                                                    .Take(pageSize)
                                                    .ToListAsync();

        int cardCount = await query.CountAsync();

        return (cardBases, (cardCount + pageSize - 1) / pageSize);
    }

    /// <inheritdoc/>
    public async Task<CardBase?> GetRandomCardInRarity(Rarity rarity)
    {
        return await _dataContext.CardBases
            .Where(cb => cb.RarityLevel == rarity)
            .OrderBy(c => Guid.NewGuid())
            .FirstOrDefaultAsync();
    }
}