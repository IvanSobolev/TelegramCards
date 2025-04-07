using TelegramBot.Entity.Dtos;

namespace TelegramBot.ApiRepository.Interfaces;

public interface ICardRepository
{
    Task<CardOutputDto> GenerateCardAsync(long id);
    Task<AllUserCardDto> GetAllUserCardAsync(long id, int page, int pageSize);
    Task<CardOutputDto> SendCardAsync(SendCardDto sendCard);
}