using TelegramCards.Models.DTO;
using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;

namespace TelegramCards.Services.interfaces;

public interface ICardBaseGeneratorService
{
   Task<CardOutputDto> GetNewCardInRandomCardStackAsync();
   Task<CardOutputDto> GetNewRandomCardInDb();
   Task GenerateNewRamdomCardStackAsync(int lengthStack);
   
}