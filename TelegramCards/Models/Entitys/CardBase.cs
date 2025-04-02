using TelegramCards.Models.Enum;

namespace TelegramCards.Models.Entitys;

public class CardBase
{
    public long Id { get; set; }
    public ICollection<Card> Cards { get; set; }
    public Rarity RarityLevel { get; set; }
    public string CardPhotoUrl { get; set; }
    public int PointsNumber { get; set; }
}