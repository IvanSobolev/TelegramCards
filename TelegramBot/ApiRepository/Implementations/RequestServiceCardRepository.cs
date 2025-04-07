using TelegramBot.ApiRepository.Interfaces;
using TelegramBot.Entity.Dtos;
using TelegramBot.Services.Interfaces;

namespace TelegramBot.ApiRepository.Implementations;

public class RequestServiceCardRepository(IRequestService requestService, string baseApiUrl) : ICardRepository
{
    private readonly IRequestService _requestService = requestService;
    private readonly string _baseApiUrl = baseApiUrl;
    
    public async Task<CardOutputDto?> GenerateCardAsync(long id)
    {
        ApiResult<CardOutputDto> card = await _requestService.GetRequestAsync<CardOutputDto>($"{_baseApiUrl}/card/generate/{id}");
        if (!card.Success)
        {
            Console.WriteLine(card.StatusCode + " " + card.ErrorMessage);
            return null;
        }

        return card.Data;
    }

    public async Task<AllUserCardDto> GetAllUserCardAsync(long id, int page, int pageSize)
    {
        ApiResult<AllUserCardDto> card = await _requestService.GetRequestAsync<AllUserCardDto>($"{_baseApiUrl}/card/get/user/{id}/{page}/{pageSize}");
        if (!card.Success || card.Data == null)
        {
            Console.WriteLine(card.StatusCode + " " + card.ErrorMessage);
            return new AllUserCardDto
            {
                Cards = new List<CardOutputDto>(),
                PageCount = 0
            };
        }

        return card.Data;
    }

    public async Task<CardOutputDto?> SendCardAsync(SendCardDto sendCard)
    {
        ApiResult<CardOutputDto> card =
            await _requestService.PutRequestAsync<CardOutputDto, SendCardDto>($"{_baseApiUrl}/card/send", sendCard);

        if (!card.Success)
        {
            Console.WriteLine(card.StatusCode + " " + card.ErrorMessage);
            return null;
        }

        return card.Data;
    }
}