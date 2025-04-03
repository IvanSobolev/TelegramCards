using TelegramCards.Models.DTO;
using TelegramCards.Models.Entitys;
using TelegramCards.Models.Enum;

namespace TelegramCards.Repositories.Interfaces;

public interface ICardBaseRepository
{
    /// <summary>
    /// Добавление новой индивидуальной карты
    /// </summary>
    /// <param name="adminId">id администратора, который добавляет карту (проверка администратора)</param>
    /// <param name="rarity">Редкость новой карты</param>
    /// <param name="photoUrl">Ссылка на фотографию карты</param>
    /// <param name="pointsNumber">Число очков, которое дает карта</param>
    /// <returns>Созданная индивидуальная карта</returns>
    Task<CardBase?> AddNewCardBaseAsync(long adminId, Rarity rarity, string photoUrl, int pointsNumber);
    
    /// <summary>
    /// Получить все индивидуальные карты с разделением на страницы
    /// </summary>
    /// <param name="adminId">id администратора, который добавляет карту (проверка администратора)</param>
    /// <param name="page">Страница данных</param>
    /// <param name="pageSize">Число данных на странице</param>
    /// <returns>(все индивидуальные карты на указанной старице, общее количество страниц)</returns>
    Task<(ICollection<CardBaseOutputDto> cardBases, int PageCount)> GetCardBasesAsync(long adminId, int page, int pageSize);
    
    /// <summary>
    /// Получение индекса последней карты в определенной редкости
    /// </summary>
    /// <param name="rarity">Редкость</param>
    /// <returns>Последний индекс карты в редкости</returns>
    Task<int> GetLastIndexInRarity(Rarity rarity);
    
}