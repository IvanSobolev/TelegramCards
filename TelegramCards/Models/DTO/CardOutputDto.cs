using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;

namespace TelegramCards.Models.DTO;

public class CardOutputDto
{
    public long Id { get; set; }
    public long OwnerId { get; set; }

    public int CardBaseId { get; set; }
    public string CardPhotoUrl { get; set; }
    public int Points { get; set; }

    public DateTime GenerationDate { get; set; }
    public DateTime ReceivedCard { get; set; }
}