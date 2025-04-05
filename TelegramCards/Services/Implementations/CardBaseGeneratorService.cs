using System.Collections.Concurrent;
using TelegramCards.Models.DTO;
using TelegramCards.Models.Enum;
using TelegramCards.Repositories.Interfaces;
using TelegramCards.Services.interfaces;

namespace TelegramCards.Services.Implementations;

public class CardBaseGeneratorService(ICardBaseRepository cardBaseRepository, int generateLengthStack) : ICardBaseGeneratorService
{
    private readonly ICardBaseRepository _cardBaseRepository = cardBaseRepository;
    private readonly int _generateLengthStack = generateLengthStack;
    private readonly ConcurrentQueue<CardOutputDto> _cacheCardBaseStack = new();
    private DateTime _lastCacheUpdate = DateTime.UtcNow;
    private bool _isGenerated = false;
    private readonly object _generationLock = new();
    private static readonly Random _random = new();
    public async Task<CardOutputDto> GetNewCardInRandomCardStackAsync()
    {
        lock (_generationLock)
        {
            if ((_cacheCardBaseStack.Count <= _generateLengthStack / 10 || 
                 (DateTime.UtcNow - _lastCacheUpdate).Hours > 24) && 
                !_isGenerated)
            {
                _isGenerated = true;
                _ = GenerateNewRamdomCardStackAsync(_generateLengthStack);
            }
        }

        if (_cacheCardBaseStack.TryDequeue(out var card))
            return card;

        return await GetNewRandomCardInDb();
    }

    public async Task<CardOutputDto> GetNewRandomCardInDb()
    {
        Rarity rarity = (Rarity)_random.Next(0, 3);
        var card = await _cardBaseRepository.GetRandomCardInRarity(rarity);
        return new CardOutputDto
        {
            CardBaseId = card.Id,
            CardPhotoUrl = card.CardPhotoUrl,
            Points = card.Points
        };
    }

    public async Task GenerateNewRamdomCardStackAsync(int lengthStack)
    {
        try
        {
            for (int i = 0; i < lengthStack && _cacheCardBaseStack.Count < _generateLengthStack * 2; i++)
            {
                _cacheCardBaseStack.Enqueue(await GetNewRandomCardInDb());
            }
            _lastCacheUpdate = DateTime.UtcNow;
        }
        finally
        {
            _isGenerated = false;
        }
    }
}