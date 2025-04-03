using TelegramCards.Models.Enum;

namespace TelegramCards.Models.DTO;

public class AddCardBaseDto
{
    public long AdminId { get; set; } 
    public Rarity Rarity{ get; set; } 
    public IFormFile File { get; set; } 
    public int PointsNumber{ get; set; }
}