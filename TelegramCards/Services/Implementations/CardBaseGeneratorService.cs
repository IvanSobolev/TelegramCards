using System.Collections.Concurrent;
using TelegramCards.Models.DTO;
using TelegramCards.Models.Enum;
using TelegramCards.Repositories.Interfaces;
using TelegramCards.Services.interfaces;

namespace TelegramCards.Services.Implementations;

public class CardBaseGeneratorService(IServiceScopeFactory scopeFactory, int generateLengthStack, IConfiguration configuration) : ICardBaseGeneratorService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly Dictionary<Rarity, double> _rarityDistribution= new Dictionary<Rarity, double>
    {
        [Rarity.Base] = configuration.GetValue<double>("RarityDistribution:Base"),
        [Rarity.Rare] = configuration.GetValue<double>("RarityDistribution:Rare"),
        [Rarity.Epic] = configuration.GetValue<double>("RarityDistribution:Epic"),
        [Rarity.Legendary] = configuration.GetValue<double>("RarityDistribution:Legendary")
    };
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
    

        var rarity = GetRandomRarity();
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
    
    private Rarity GetRandomRarity()
    {
        double randomValue = _random.NextDouble() * 100.0;
        double cumulative = 0.0;

        foreach (var rarity in Enum.GetValues<Rarity>())
        {
            var probability = _rarityDistribution[rarity];
            cumulative += probability;
            if (randomValue <= cumulative)
                return rarity;
        }

        return Rarity.Base;
    }
}