namespace TelegramCards.Models.Entitys;

public class User
{
    public long TelegramId { get; set; }
    public string Username { get; set; }
    public ICollection<Card> Cards { get; set; }
}