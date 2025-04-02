using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;

namespace TelegramCards.Repositories.Interfaces;

public interface ICardBaseRepository
{
    Task<CardBase> AddNewCardBaseAsync(long adminId, Rarity rarity, string photoUrl, int pointsNumber);
    Task<ICollection<CardBase>> GetCardBasesAsync(long adminId, int page, int pageSize);
    Task<CardBase> GetCardBasesAsync(Rarity rarityLevel, int cardIndex);
    Task<int> GetCardsRarityCount(Rarity rarity);
    Task<CardBase> UpdateCardBaseAsync(long adminId, long cardId, Rarity? rarity = null, string? photoUrl = null, int? pointsNumber = null);
    Task DeleteCardBaseAsync(long adminId, long id);
    
}