using TelegramCards.Models.DTO;

namespace TelegramCards.Managers.Interfaces;

public interface ICardManager
{
    /// <summary>
    /// Генерация новой карты для пользователя
    /// </summary>
    /// <param name="userTelegramId">telegram id пользователя</param>
    /// <returns>Сгенерированная карта в общем формате</returns>
    Task<CardOutputDto?> GenerateNewCardToUserAsync(long userTelegramId);
    
    /// <summary>
    /// Получение всех карту у пользователя с выборкой по страницам
    /// </summary>
    /// <param name="ownerId">Пользователь, чьи карты нам нужны</param>
    /// <param name="page">Страница данных</param>
    /// <param name="pageSize">Количество данных на странице</param>
    /// <returns>(все карты пользователя на указанной старице, общее количество страниц)</returns>
    Task<GetAllUserCardDto> GetUserCardsAsync(long ownerId, int page, int pageSize);
    
    /// <summary>
    /// Отправить карту другому пользователя
    /// </summary>
    /// <param name="cardDto">send card DTO с данным для отправки карты</param>
    /// <returns>Отправленная карта (если все успешно, то с новым пользователем)</returns>
    Task<CardOutputDto?> SendCardAsync(SendCardDto cardDto);
}