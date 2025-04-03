using TelegramCards.Models.Enum;

namespace TelegramCards.Models.Entitys;

public class CardBase
{
    public Rarity RarityLevel { get; set; }
    public int CardIndex { get; set; }
    public string CardPhotoUrl { get; set; }
    public int Points { get; set; }
    
    public ICollection<Card> Cards { get; set; }
}