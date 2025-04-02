using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;

namespace TelegramCards.Services.interfaces;

public interface ICardBaseGeneratorService
{
   Task<Rarity> GetRandomRarityAsync();
   Task<int> GetRandomCardIndexByRarityAsync(Rarity rarity);
}