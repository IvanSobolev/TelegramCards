namespace TelegramCards.Models.DTO;

public class GetAllUserCardDto
{
    public ICollection<CardOutputDto> Cards { get; set; }
    public int PageCount { get; set; }
}