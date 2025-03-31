namespace TelegramCards.Models.Entitys;

public class Card
{
    public long Id { get; set; }
    public long? OwnerId { get; set; }
    public long? BaseCardId { get; set; }
    public DateTime GenerateCard { get; set; }
    public DateTime UserReceivedCard { get; set; }
    
    public CardBase? BaseCard { get; set; }
    public User? Owner { get; set; }
}