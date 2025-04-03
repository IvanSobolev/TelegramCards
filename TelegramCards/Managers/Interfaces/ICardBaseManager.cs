using TelegramCards.Models.DTO;
using TelegramCards.Models.Enum;

namespace TelegramCards.Managers.Interfaces;

public interface ICardBaseManager
{
    Task<CardBaseOutputDto?> AddNewCardBaseAsync(AddCardBaseDto cardBaseDto);
    Task<(ICollection<CardBaseOutputDto> cardBases, int PageCount)> GetCardBasesAsync(long adminId, int page, int pageSize);
}