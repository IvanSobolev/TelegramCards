using TelegramCards.Models.Enum;

namespace TelegramCards.Models.DTO;

public class CardBaseOutputDto
{
    public Rarity RarityLevel { get; set; }
    public int CardIndex { get; set; }
    public string CardPhotoUrl { get; set; }
    public int Points { get; set; }
}