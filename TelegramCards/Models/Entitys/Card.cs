using TelegramCards.Models.Enum;

namespace TelegramCards.Models.Entitys;

public class Card
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public Rarity RarityLevel { get; set; }
    public long CardIndex { get; set; }
    public DateTime GenerationDate { get; set; }
    public DateTime ReceivedCard { get; set; }
    
    public CardBase BaseCard { get; set; }
    public User Owner { get; set; }
}