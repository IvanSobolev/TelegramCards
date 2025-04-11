using System.Collections.Concurrent;
using TelegramCards.Models.DTO;
using TelegramCards.Models.Enum;
using TelegramCards.Repositories.Interfaces;
using TelegramCards.Services.interfaces;

namespace TelegramCards.Services.Implementations;

public class CardBaseGeneratorService(IServiceScopeFactory scopeFactory, int generateLengthStack) : ICardBaseGeneratorService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
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
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICardBaseRepository>();
        Rarity rarity = (Rarity)_random.Next(0, 3);
        var card = await repository.GetRandomCardInRarity(rarity);
        return new CardOutputDto
        {
            CardBaseId = card.Id,
            Name = card.Name,
            Creator = card.Creator,
            RarityLevel = card.RarityLevel,
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