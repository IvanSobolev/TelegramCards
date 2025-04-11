using System.Collections;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.ApiRepository.Interfaces;
using TelegramBot.Entity.Dtos;
using Exception = System.Exception;

namespace TelegramBot;

public class CommandHandler(TelegramBotClient bot, ICardRepository cardRepository, IUserRepository userRepository)
{
    private readonly TelegramBotClient _bot = bot;
    private readonly ICardRepository _cardRepository = cardRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly Dictionary<long, DateTime> _userGeneratorDate = new Dictionary<long, DateTime>(); 
    private readonly Dictionary<long, SendCardDto> _userStarSendCard = new Dictionary<long, SendCardDto>(); 
    
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
                await bot.SendMessage(msg.Chat.Id, "Для использования бота, у вас должен быть username.", replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            user = await userRepository.AddNewUserAsync(new AddNewUserDto
                { TelegramId = msg.Chat.Id, Username = msg.Chat.Username });

            if (user == null)
            {
                await bot.SendMessage(msg.Chat.Id, "Ошибка. Попробуйте написать /start позже.", replyMarkup: new ReplyKeyboardRemove());
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

        string nextCardText = "";
        
        if (DateTime.UtcNow >= nextAvailableTime)
        {
            nextCardText = "Вы можете получить карточку прямо сейчас!";
        }
        else
        {
            var timeLeft = nextAvailableTime - DateTime.UtcNow;
            nextCardText = $"Следующую карточку можно будет получить через {timeLeft:h\\:mm} (час:минута)";
        }
        
        
        await bot.SendMessage(msg.Chat.Id, 
            $"Привет, это бот для коллекционирования карточек.\n{nextCardText}", 
                replyMarkup: replyMarkup);
    }

    public async Task GenerateCardCommand(Message msg)
    {
        if(!_userGeneratorDate.TryGetValue(msg.Chat.Id, out var lastGeneration))
        {
            await bot.SendMessage(msg.Chat.Id, "Что-то пошло не так. Напишите /start для синхронизации данных.", replyMarkup: new ReplyKeyboardRemove());
            return;
        }

        if (DateTime.UtcNow < lastGeneration.AddHours(4))
        {
            var timeLeft = lastGeneration.AddHours(4) - DateTime.UtcNow;
            await bot.SendMessage(msg.Chat.Id, $"Вы можете получить карту только через {timeLeft:h\\:mm} (час:минута)");
            return;
        }

        CardOutputDto? card = await _cardRepository.GenerateCardAsync(msg.Chat.Id);

        if (card == null)
        {
            await bot.SendMessage(msg.Chat.Id, "Что-то пошло не так. Напишите /start для синхронизации данных.", replyMarkup: new ReplyKeyboardRemove());
            return;
        }
        string name = card.Creator == null? card.Name : $"*{card.Creator}* - {card.Name}";
        try
        {
            using var httpClient = new HttpClient();
            byte[] imageBytes = await httpClient.GetByteArrayAsync(card.CardPhotoUrl);
            
            using var stream = new MemoryStream(imageBytes);
            await _bot.SendPhoto(
                chatId: msg.Chat.Id,
                photo: InputFile.FromStream(stream, "card.png"),
                caption: $"Вот ваша новая карточка!\n{name}\nРедкость: {card.RarityLevel.ToString()}\nОчки: {card.Points}",
                parseMode: ParseMode.Markdown
            );
            _userGeneratorDate[msg.Chat.Id] = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending photo: {ex.Message}");
            await _bot.SendMessage(
                chatId: msg.Chat.Id,
                text: $"Вот ваша новая карточка!\n{name}\nОчки: {card.Points}.\n(К сожалению, с картинкой, что-то пошло не так)",
                parseMode: ParseMode.Markdown
            );
        }
    }

    public async Task GetMyCardsCommand(Message msg)
    {
        AllUserCardDto cardDto = await _cardRepository.GetAllUserCardAsync(msg.Chat.Id, 1, 1);
        if (cardDto.Cards.Count < 1 || cardDto.PageCount == 0)
        {
            await bot.SendMessage(msg.Chat.Id, "У вас еще нет карт.");
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
                await bot.SendMessage(msg.Chat.Id,
                    "Пользователь не найден, попробуйте кого-то другого\n(Если пользователя нет в игре, или он менял username попроси написать /start в бота)",
                    replyMarkup: new InlineKeyboardMarkup(new[]{InlineKeyboardButton.WithCallbackData("❌Отмена отправки карты", $"exit")}));
            }

            lastTry.NewOwnerId = user.TelegramId;
            var card = await _cardRepository.SendCardAsync(lastTry);
            await bot.SendMessage(msg.Chat.Id,
                $"Ваша карта отправлена пользователю @{user.Username}");
            string name = card.Creator == null? card.Name : $"*{card.Creator}* - {card.Name}";
            try
            {
                using var httpClient = new HttpClient();
                byte[] imageBytes = await httpClient.GetByteArrayAsync(card.CardPhotoUrl);
            
                using var stream = new MemoryStream(imageBytes);
                await _bot.SendPhoto(
                    chatId: user.TelegramId,
                    photo: InputFile.FromStream(stream, "card.png"),
                    caption: $"Пользователь @{msg.From!.Username} отправил вам карту `#{card.CardBaseId}`\n{name}\nРедкость: {card.RarityLevel.ToString()}\nОчки: {card.Points}\nСоздана {card.GenerationDate:dd/MM/yyyy}",
                    parseMode: ParseMode.Markdown
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending photo: {ex.Message}");
                await _bot.SendMessage(
                    chatId: user.TelegramId,
                    text: $"Пользователь @{msg.From!.Username} отправил вам карту `#{card.CardBaseId}`\n{name}\nРедкость: {card.RarityLevel.ToString()}\nОчки: {card.Points}\nСоздана {card.GenerationDate:dd/MM/yyyy}\n(К сожалению, с картинкой, что-то пошло не так)",
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
            await bot.SendMessage(msg.Chat.Id, "У вас нет этой карты");
            return;
        }

        await SendCardAsync(query.Message!.Chat.Id, cardDto.Cards.First(), newIndex, cardDto.PageCount);
    }
    
    public async Task ExitSliderButtonAsync(CallbackQuery query)
    {
        Message msg = query.Message!;
        _userStarSendCard.Remove(query.From.Id);
        await bot.DeleteMessage(query.From.Id, msg.MessageId);
    }

    public async Task SendCardButtonAsync(CallbackQuery query)
    {
        Message msg = query.Message!;
        await bot.DeleteMessage(msg.Chat.Id, msg.MessageId);
        int cardId = int.Parse(query.Data!.Split('_')[1], CultureInfo.InvariantCulture);
        await bot.SendMessage(query.From.Id,
            "Отправь @username пользователя, которому хочешь отправить эту карту\n(Если пользователя нет в игре, или он менял username попроси написать /start в бота)\nПример: @example",
            replyMarkup: new InlineKeyboardMarkup(new[]{InlineKeyboardButton.WithCallbackData("❌Отмена отправки карты", $"exit")}));
        _userStarSendCard.Add(query.From.Id, new SendCardDto{SenderId = query.From.Id, CardId = cardId, NewOwnerId = 0});
    }
    
    public async Task SendCardAsync(long chatId, CardOutputDto card, int page, int lastPage)
    {
        var buttons = new List<InlineKeyboardButton[]>();
    
        var navButtons = new List<InlineKeyboardButton>();
    
        if (lastPage > 3 && page > 3)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("<< Первая", $"card_1"));
        }
        
        if (page > 1)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("< Назад", $"card_{page - 1}"));
        }

        navButtons.Add(InlineKeyboardButton.WithCallbackData($"{page}/{lastPage}", "zero"));

        if (page < lastPage)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("Вперёд >", $"card_{page + 1}"));
        }
        
        if (page < lastPage - 2 && lastPage > 3)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("Последняя >>", $"card_{lastPage}"));
        }
    
        buttons.Add(navButtons.ToArray());
        
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("📻Отправить карту игроку", $"send_{card.Id}")
        });
        
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🍕 Выйти", "exit")
        });

        var inlineKeyboard = new InlineKeyboardMarkup(buttons);
        string name = card.Creator == null? card.Name : $"*{card.Creator}* - {card.Name}";
        
        try
        {
            using var httpClient = new HttpClient();
            byte[] imageBytes = await httpClient.GetByteArrayAsync(card.CardPhotoUrl);
            
            using var stream = new MemoryStream(imageBytes);
            await _bot.SendPhoto(
                chatId: chatId,
                photo: InputFile.FromStream(stream, "card.png"),
                caption: $"Карта `#{card.CardBaseId}`\n{name}\nРедкость: {card.RarityLevel.ToString()}\nОчки: {card.Points}\nСоздана {card.GenerationDate:dd/MM/yyyy}",
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending photo: {ex.Message}");
            await _bot.SendMessage(
                chatId: chatId,
                text: $"Карта `#{card.CardBaseId}`\n{name}\nРедкость: {card.RarityLevel.ToString()}\nОчки: {card.Points}\nСоздана {card.GenerationDate:dd/MM/yyyy}\n(К сожалению, с картинкой, что-то пошло не так)",
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown
            );
        }
    }

}