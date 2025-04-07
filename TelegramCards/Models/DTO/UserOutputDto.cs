using TelegramCards.Models.Enum;

namespace TelegramCards.Models.DTO;

public class UserOutputDto
{
    public long TelegramId { get; set; }
    public string Username { get; set; }
    public Roles Role { get; set; }
    public DateTime LastTakeCard { get; set; }
}