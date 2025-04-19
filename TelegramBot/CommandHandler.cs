using System.Collections;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.ApiRepository.Interfaces;
using TelegramBot.Entity.Dtos;
using TelegramCards.Models.Enum;
using Exception = System.Exception;

namespace TelegramBot;

public class CommandHandler(TelegramBotClient bot, ICardRepository cardRepository, IUserRepository userRepository)
{
    private readonly TelegramBotClient _bot = bot;
    private readonly ICardRepository _cardRepository = cardRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly Dictionary<long, DateTime> _userGeneratorDate = new Dictionary<long, DateTime>();
    private readonly Dictionary<long, SendCardDto> _userStarSendCard = new Dictionary<long, SendCardDto>();
    private readonly Dictionary<long, bool> _userActiveNewCard = new Dictionary<long, bool>();

    ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton[] { "💎 Получить карту", "🧊 Посмотреть карты" }
    })
    {
        ResizeKeyboard = true
    };

    public async Task StartCommand(Message msg)
    {
        UserOutputDto? user = await _userRepository.GetUserByTelegramIdAsync(msg.Chat.Id);
        if (user == null)
        {
            if (msg.Chat.Username == null)
            {
                await bot.SendMessage(chatId: msg.Chat.Id,
                    text:
                    "🚫 *Для использования бота у вас должен быть username!*\n\nПожалуйста, установите username в настройках Telegram и попробуйте снова.",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            user = await userRepository.AddNewUserAsync(new AddNewUserDto
                { TelegramId = msg.Chat.Id, Username = msg.Chat.Username });

            if (user == null)
            {
                await bot.SendMessage(chatId: msg.Chat.Id,
                    text: "⚠️ *Ошибка при регистрации!*\n\nПопробуйте написать /start позже.",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: new ReplyKeyboardRemove());
                return;
            }
        }

        if (user.Username != msg.Chat.Username && msg.Chat.Username != null)
        {
            await _userRepository.EditUsernameAsync(msg.Chat.Id, msg.Chat.Username);
        }

        _userGeneratorDate[msg.Chat.Id] = user.LastTakeCard;

        var lastGeneration = _userGeneratorDate[msg.Chat.Id];

        var nextAvailableTime = lastGeneration.AddHours(4);

        string nextCardText = DateTime.UtcNow >= nextAvailableTime
            ? "🎉 *Вы можете получить карточку прямо сейчас!*"
            : $"⏳ Следующую карточку можно будет получить через *{nextAvailableTime - DateTime.UtcNow:h\\:mm}* (час:минута)";


        await bot.SendMessage(chatId: msg.Chat.Id,
            text: $"""
                   🃏 *Добро пожаловать в Mine Cards bot!*

                   Здесь вы можете собирать уникальные карточки, по разному контенту в игре майнкрафт, обмениваться ими с друзьями и соревноваться за топовые места!

                   {nextCardText}

                   *Доступные команды:*
                   /start - Начать игру (синхронизировать данные).
                   `💎 Получить карту` - Получить новую карточку.
                   `🧊 Посмотреть карты` - Посмотреть все свои карточки.
                   """,
            parseMode: ParseMode.Markdown,
            replyMarkup: replyMarkup);
    }

    public async Task GenerateCardCommand(Message msg)
    {
        if (_userActiveNewCard.TryGetValue(msg.Chat.Id, out var value) && value)
        {
            await bot.SendMessage(chatId: msg.Chat.Id,
                text: "🟠 *Вы не завершили прошлое получение карты*\n\nНажмите на кнопку ✅ под сообщением о получении карты.",
                parseMode: ParseMode.Markdown);
            return;
        }
        if (!_userGeneratorDate.TryGetValue(msg.Chat.Id, out var lastGeneration))
        {
            await bot.SendMessage(chatId: msg.Chat.Id,
                text: "🔴 *Ошибка синхронизации!*\n\nНапишите /start для обновления данных.",
                parseMode: ParseMode.Markdown,
                replyMarkup: new ReplyKeyboardRemove());
            return;
        }

        if (DateTime.UtcNow < lastGeneration.AddHours(4))
        {
            var timeLeft = lastGeneration.AddHours(4) - DateTime.UtcNow;
            await bot.SendMessage(chatId: msg.Chat.Id,
                text:
                $"⏳ *Карта еще не готова!*\n\nВы сможете получить следующую карту через *{timeLeft:h\\:mm}* (час:минута)\n\nПопробуйте позже!",
                parseMode: ParseMode.Markdown);
            return;
        }

        CardOutputDto? card = await _cardRepository.GenerateCardAsync(msg.Chat.Id);

        if (card == null)
        {
            await bot.SendMessage(chatId: msg.Chat.Id,
                text: "🔴 *Ошибка генерации карты!*\n\nПопробуйте снова через несколько минут.",
                parseMode: ParseMode.Markdown);
            return;
        }

        string name = card.Creator == null ? $"*{card.Name}*" : $"*{card.Creator}* - {card.Name}";
        
        string rarityEmoji = GetRarityEmoji(card.RarityLevel);

        _userActiveNewCard[msg.Chat.Id] = true;
        try
        {
            using var httpClient = new HttpClient();
            byte[] imageBytes = await httpClient.GetByteArrayAsync(card.CardPhotoUrl);

            using var stream = new MemoryStream(imageBytes);
            await _bot.SendPhoto(
                chatId: msg.Chat.Id,
                photo: InputFile.FromStream(stream, "card.png"),
                caption: $"""
                          🎊 *Ваша новая карта!*

                          {name}
                          {rarityEmoji} Редкость: *{card.RarityLevel}*
                          ⭐ Очки: *{card.Points}*

                          Продолжайте собирать свою коллекцию!
                          """,
                parseMode: ParseMode.Markdown
            );
            _userGeneratorDate[msg.Chat.Id] = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending photo: {ex.Message}");
            await _bot.SendMessage(
                chatId: msg.Chat.Id,
                text: $"""
                       🎊 *Новая карта добавлена в вашу коллекцию!*

                       {name}
                       {rarityEmoji} Редкость: *{card.RarityLevel}*
                       ⭐ Очки: *{card.Points}*

                       🖼 *Изображение временно недоступно*
                       """,
                parseMode: ParseMode.Markdown
            );
        }

        await _bot.SendMessage(
            chatId: msg.Chat.Id,
            text: "Подтвердите принятие карты!",
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("✅", $"getcard_{msg.Chat.Id}")
            }));
    }

    public async Task GetMyCardsCommand(Message msg)
    {
        AllUserCardDto cardDto = await _cardRepository.GetAllUserCardAsync(msg.Chat.Id, 1, 1);
        if (cardDto.Cards.Count < 1 || cardDto.PageCount == 0)
        {
            await bot.SendMessage(chatId: msg.Chat.Id,
                text:
                "📭 *Ваша коллекция пуста!*\n\nИспользуйте кнопку \"💎 Получить карту\" чтобы добавить первую карту в коллекцию.",
                parseMode: ParseMode.Markdown);
            return;
        }


        var card = cardDto.Cards.First();

        await SendCardAsync(msg.Chat.Id, card, 1, cardDto.PageCount);
    }

    public async Task NoCommandMessage(Message msg)
    {
        if (_userStarSendCard.TryGetValue(msg.Chat.Id, out var lastTry) && msg.Text![0] == '@')
        {
            var user = await _userRepository.GetUserByUsernameAsync(msg.Text!.Split('@')[1]);
            if (user == null)
            {
                await bot.SendMessage(chatId: msg.Chat.Id,
                    text: """
                          🔍 *Пользователь не найден!*

                          Проверьте правильность написания username:
                          - Пользователь должен быть зарегистрирован в боте
                          - Username должен быть актуальным

                          Попробуйте снова или отмените отправку.
                          """,
                    replyMarkup: new InlineKeyboardMarkup(new[]
                        { InlineKeyboardButton.WithCallbackData("❌Отмена отправки карты", $"exit") }));
            }

            lastTry.NewOwnerId = user.TelegramId;
            if(lastTry.NewOwnerId == 0 || lastTry.CardId == 0 || lastTry.SenderId == 0)
            {
                await bot.SendMessage(chatId: msg.Chat.Id,
                text: $"Что-то пошло не так. Попробуйте еще раз.",
                parseMode: ParseMode.Markdown);
                return;
            }
            var card = await _cardRepository.SendCardAsync(lastTry);
            _userStarSendCard.Remove(msg.Chat.Id);
            await bot.SendMessage(chatId: msg.Chat.Id,
                text: $"✅ *Карта успешно отправлена!* @{user.Username.Replace("_", "\\_")}",
                parseMode: ParseMode.Markdown);
            string name = card.Creator == null ? $"*{card.Name}*" : $"*{card.Creator}* - {card.Name}";
            string rarityEmoji = GetRarityEmoji(card.RarityLevel);
            try
            {
                using var httpClient = new HttpClient();
                byte[] imageBytes = await httpClient.GetByteArrayAsync(card.CardPhotoUrl);

                using var stream = new MemoryStream(imageBytes);
                await _bot.SendPhoto(
                    chatId: user.TelegramId,
                    photo: InputFile.FromStream(stream, "card.png"),
                    caption: $"""
                              ✉️ *Вы получили новую карту!*

                              От: @{msg.From!.Username.Replace("_", "\\_")}
                              Карта #{card.CardBaseId}

                              {name}
                              {rarityEmoji} Редкость: *{card.RarityLevel}*
                              ⭐ Очки: *{card.Points}*
                              🗓 Создана: *{card.GenerationDate:dd.MM.yyyy}*
                              """,
                    parseMode: ParseMode.Markdown
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending photo: {ex.Message}");
                await _bot.SendMessage(
                    chatId: user.TelegramId,
                    text: $"""
                           ✉️ *Вы получили новую карту!*

                           От: @{msg.From!.Username.Replace("_", "\\_")}
                           Карта #{card.CardBaseId}

                           {name}
                           {rarityEmoji} Редкость: *{card.RarityLevel}*
                           ⭐ Очки: *{card.Points}*
                           🗓 Создана: *{card.GenerationDate:dd.MM.yyyy}*

                           🖼 *Изображение временно недоступно*
                           """,
                    parseMode: ParseMode.Markdown
                );
            }

        }
    }

    public async Task GetAnotherCardButtonAsync(CallbackQuery query)
    {
        Message msg = query.Message!;
        await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);
        int newIndex = int.Parse(query.Data!.Split('_')[1], CultureInfo.InvariantCulture);
        AllUserCardDto cardDto = await _cardRepository.GetAllUserCardAsync(msg.Chat.Id, newIndex, 1);
        if (cardDto.Cards.Count < 1 || cardDto.PageCount == 0)
        {
            await bot.SendMessage(chatId: msg.Chat.Id,
                text: "🔍 *Карта не найдена!*\n\nВозможно, она была удалена или передана другому игроку.",
                parseMode: ParseMode.Markdown);
            return;
        }

        await SendCardAsync(query.Message!.Chat.Id, cardDto.Cards.First(), newIndex, cardDto.PageCount);
    }

    public async Task ExitSliderButtonAsync(CallbackQuery query)
    {
        Message msg = query.Message!;
        _userStarSendCard.Remove(query.From.Id);
        await bot.DeleteMessage(query.From.Id, msg.MessageId);
        await _bot.SendMessage(
            chatId: query.From.Id,
            text: "👋 *Просмотр коллекции завершен!*",
            parseMode: ParseMode.Markdown,
            replyMarkup: replyMarkup);
    }

    public async Task SendCardButtonAsync(CallbackQuery query)
    {
        Message msg = query.Message!;
        await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);
        int cardId = int.Parse(query.Data!.Split('_')[1], CultureInfo.InvariantCulture);
        await bot.SendMessage(chatId: query.From.Id,
            text: """
                  📤 *Отправка карты*

                  Введите @username игрока, которому хотите передать карту\.
                  
                  *Пример*:
                  @username

                  ⚠️ *Учтите*:
                  1\. Отправка необратима
                  2\. Игрок должен быть зарегистрирован в боте
                  3\. Проверьте правильность username
                  """,
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: new InlineKeyboardMarkup(new[]
                { InlineKeyboardButton.WithCallbackData("❌Отмена отправки карты", $"exit") }));
        _userStarSendCard.Add(query.From.Id,
            new SendCardDto { SenderId = query.From.Id, CardId = cardId, NewOwnerId = 0 });
    }
    
    public async Task AcceptCardButtonAsync(CallbackQuery query)
    {
        Message msg = query.Message!;
        await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);
        long userId = int.Parse(query.Data!.Split('_')[1], CultureInfo.InvariantCulture);
        _userActiveNewCard.Remove(userId);
    }

    public async Task SendCardAsync(long chatId, CardOutputDto card, int page, int lastPage)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        var navButtons = new List<InlineKeyboardButton>();

        if (lastPage > 3 && page > 3)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("⏪ Первая", $"card_1"));
        }

        if (page > 1)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("◀️ Назад", $"card_{page - 1}"));
        }

        navButtons.Add(InlineKeyboardButton.WithCallbackData($"📌 {page}/{lastPage}", "zero"));

        if (page < lastPage)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("Вперёд ▶️", $"card_{page + 1}"));
        }

        if (page < lastPage - 2 && lastPage > 3)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("Последняя ⏩", $"card_{lastPage}"));
        }

        buttons.Add(navButtons.ToArray());

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🎁 Отправить карту", $"send_{card.Id}"),
            InlineKeyboardButton.WithCallbackData("📈Табличный просмотр карт", $"tableview_{page}")
        });

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🚪 Выход", "exit")
        });

        var inlineKeyboard = new InlineKeyboardMarkup(buttons);
        string name = card.Creator == null ? $"*{card.Name}*" : $"*{card.Creator}* - {card.Name}";
        string rarityEmoji = GetRarityEmoji(card.RarityLevel);
        try
        {
            using var httpClient = new HttpClient();
            byte[] imageBytes = await httpClient.GetByteArrayAsync(card.CardPhotoUrl);

            using var stream = new MemoryStream(imageBytes);
            await _bot.SendPhoto(
                chatId: chatId,
                photo: InputFile.FromStream(stream, "card.png"),
                caption: $"""
                          🃏 *Карта #{card.CardBaseId}*

                          {name}
                          {rarityEmoji} Редкость: *{card.RarityLevel}*
                          ⭐ Очки: *{card.Points}*
                          🗓 Создана: *{card.GenerationDate:dd.MM.yyyy}*

                          📖 Страница: {page}/{lastPage}
                          """,
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending photo: {ex.Message}");
            await _bot.SendMessage(
                chatId: chatId,
                text: $"""
                       🃏 *Карта #{card.CardBaseId}*

                       {name}
                       {rarityEmoji} Редкость: *{card.RarityLevel}*
                       ⭐ Очки: *{card.Points}*
                       🗓 Создана: *{card.GenerationDate:dd.MM.yyyy}*

                       📖 Страница: {page}/{lastPage}

                       🖼 *Изображение временно недоступно*
                       """,
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown
            );
        }
    }

    public async Task ShowTableFormatButtonAsync(CallbackQuery query)
    {
        Message msg = query.Message!;
        await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);
        int cardId = int.Parse(query.Data!.Split('_')[1], CultureInfo.InvariantCulture);
        await ShowCardsTableAsync(msg.Chat.Id, (cardId / 15) + 1);
        Console.WriteLine(cardId + "  " +  (cardId / 15) + 1);
    }
    
    public async Task ShowCardsTableAsync(long chatId, int page = 1, int pageSize = 15)
{
    // Получаем данные о картах пользователя
    AllUserCardDto cardDto = await _cardRepository.GetAllUserCardAsync(chatId, page, pageSize);
    
    if (cardDto.Cards.Count < 1 || cardDto.PageCount == 0)
    {
        await _bot.SendMessage(
            chatId: chatId,
            text: "📭 *Ваша коллекция пуста!*\n\nИспользуйте кнопку \"💎 Получить карту\" чтобы добавить первую карту в коллекцию.",
            parseMode: ParseMode.Markdown);
        return;
    }

    // Формируем таблицу карт
    var tableHeader = "📋 *Ваша коллекция карт*\n\n";
    var tableFormat = "`{0,2}|{1,-12}|{2,-2}|{3,4}`";
    
    var table = new List<string>
    {
        string.Format(tableFormat, "#", "Имя", "R", "Очки"),
        string.Format(tableFormat, "--", "--------", "--", "----")
    };

    int index = (page - 1) * pageSize + 1;
    foreach (var card in cardDto.Cards)
    {
        string rarity = GetRarityEmoji(card.RarityLevel);
        string name = card.Creator ?? card.Name;
        table.Add(string.Format(tableFormat, 
                              index++, 
                              Truncate(name, 10), 
                              rarity, 
                              card.Points));
    }

    // Создаем кнопки для навигации по картам
    var buttons = new List<InlineKeyboardButton[]>();
    
    // Кнопки для выбора конкретной карты (первые 3 ряда)
    var cardButtons = new List<InlineKeyboardButton>();
    for (int i = 0; i < cardDto.Cards.Count; i++)
    {
        cardButtons.Add(InlineKeyboardButton.WithCallbackData(
            (i + (page - 1) * pageSize + 1).ToString(), 
            $"view_{i + (page - 1) * pageSize  + 1}")); // Сохраняем номер страницы и индекс карты
        
        if (cardButtons.Count % 5 == 0 || i == cardDto.Cards.Count - 1)
        {
            buttons.Add(cardButtons.ToArray());
            cardButtons.Clear();
        }
    }

    // Кнопки для навигации по страницам
    var navButtons = new List<InlineKeyboardButton>();
    if (page > 1)
    {
        navButtons.Add(InlineKeyboardButton.WithCallbackData("◀️ Назад", $"table_{page - 1}"));
    }
    
    navButtons.Add(InlineKeyboardButton.WithCallbackData(
        $"📄 {page}/{cardDto.PageCount}", 
        "table_page"));
    
    if (page < cardDto.PageCount)
    {
        navButtons.Add(InlineKeyboardButton.WithCallbackData("Вперёд ▶️", $"table_{page + 1}"));
    }
    
    buttons.Add(navButtons.ToArray());

    // Кнопка выхода
    buttons.Add(new[]
    {
        InlineKeyboardButton.WithCallbackData("🚪 Выход", "exit")
    });

    var inlineKeyboard = new InlineKeyboardMarkup(buttons);


    // Отправляем сообщение с таблицей
    await _bot.SendMessage(
        chatId: chatId,
        text: tableHeader + string.Join("\n", table),
        parseMode: ParseMode.Markdown,
        replyMarkup: inlineKeyboard);
}

// Обработчик кнопок таблицы
    public async Task HandleTableNavigationAsync(CallbackQuery query)
    {
        if (int.TryParse(query.Data!.Split('_')[1], out int page))
        {
            await _bot.DeleteMessage(query.Message!.Chat.Id, query.Message.MessageId);
            await ShowCardsTableAsync(query.Message.Chat.Id, page);
        }
    }

    public async Task HandleCardViewAsync(CallbackQuery query)
    {
        var parts = query.Data!.Split('_');
        if (parts.Length == 2 && int.TryParse(parts[1], out int cardIndex))
        {
            await _bot.DeleteMessage(query.Message!.Chat.Id, query.Message.MessageId);
        
            var cards = await _cardRepository.GetAllUserCardAsync(query.Message.Chat.Id, cardIndex, 1);
        
            await SendCardAsync(
                query.Message.Chat.Id, 
                cards.Cards.First(), 
                cardIndex, 
                cards.PageCount);
        }
    }

    private static string GetRarityEmoji(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Wood => "🪵",
            Rarity.Iron => "🪨",
            Rarity.Gold => "🥇",
            Rarity.Diamonds => "💎"
        };
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) 
            return value;
    
        return value.Length <= maxLength ? 
            value : 
            value.Substring(0, maxLength - 3) + "...";
    }

}