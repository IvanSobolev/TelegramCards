namespace TelegramBot.Entity.Dtos;

public class AllUserCardDto
{
    public ICollection<CardOutputDto> Cards { get; set; }
    public int PageCount { get; set; }
}