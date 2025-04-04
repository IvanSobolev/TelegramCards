using TelegramCards.Models.DTO;
using TelegramCards.Models.Enum;

namespace TelegramCards.Managers.Interfaces;

public interface ICardBaseManager
{
    /// <summary>
    /// Добавление новой индивидуальной карты
    /// </summary>
    /// <param name="cardBaseDto">Add card base DTO для добавления новой индивидуальной карты</param>
    /// <returns>Созданная индивидуальная карта</returns>
    Task<CardBaseOutputDto?> AddNewCardBaseAsync(AddCardBaseDto cardBaseDto);
    
    /// <summary>
    /// Получить все индивидуальные карты с разделением на страницы
    /// </summary>
    /// <param name="adminId">id администратора, который добавляет карту (проверка администратора)</param>
    /// <param name="page">Страница данных</param>
    /// <param name="pageSize">Число данных на странице</param>
    /// <returns>(все индивидуальные карты на указанной старице, общее количество страниц)</returns>
    Task<(ICollection<CardBaseOutputDto> cardBases, int PageCount)> GetCardBasesAsync(long adminId, int page, int pageSize);
}