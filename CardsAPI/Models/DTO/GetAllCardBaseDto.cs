namespace TelegramCards.Models.DTO;

public class GetAllCardBaseDto
{
    public ICollection<CardBaseOutputDto> CardBases {get;set;} 
    public int PageCount {get;set;}
}