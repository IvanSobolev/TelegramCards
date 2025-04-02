using TelegramCards.Models.Entitys;

namespace TelegramCards.Services.interfaces;

public interface ICardBaseGeneratorService
{
   Task<long> GetRandomBaseCardIdAsync();
}