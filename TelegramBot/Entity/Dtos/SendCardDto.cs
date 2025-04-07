namespace TelegramBot.Entity.Dtos;

public class SendCardDto
{
    public long SenderId { get; set; }
    public long NewOwnerId { get; set; }
    public long CardId { get; set; }
}