using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;

namespace TelegramCards.Repositories.Interfaces;

public interface ICardBaseRepository
{
    Task<CardBase?> AddNewCardBaseAsync(long adminId, Rarity rarity, string photoUrl, int pointsNumber);
    Task<(ICollection<CardBase> cardBases, int PageCount)> GetCardBasesAsync(long adminId, int page, int pageSize);
    Task<int> GetLastIndexInRarity(Rarity rarity);
    
}