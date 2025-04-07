namespace TelegramBot.Entity.Dtos;

public class UserOutputDto
{
    public long TelegramId { get; set; }
    public string Username { get; set; }
    public int Role { get; set; }
    public DateTime LastTakeCard { get; set; }
}