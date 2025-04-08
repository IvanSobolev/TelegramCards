using System.Collections;
using Telegram.Bot;
using Telegram.Bot.Types;
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
    private readonly Dictionary<long, int> _userLastPageCard = new Dictionary<long, int>(); 
    
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
        
        try
        {
            using var httpClient = new HttpClient();
            byte[] imageBytes = await httpClient.GetByteArrayAsync(card.CardPhotoUrl);
            
            using var stream = new MemoryStream(imageBytes);
            await _bot.SendPhoto(
                chatId: msg.Chat.Id,
                photo: InputFile.FromStream(stream, "card.png"),
                caption: $"Вот ваша новая карточка!\nОчки: {card.Points}"
            );
            _userGeneratorDate[msg.Chat.Id] = DateTime.Now;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending photo: {ex.Message}");
            await _bot.SendMessage(
                chatId: msg.Chat.Id,
                text: $"Вот ваша новая карточка!\nОчки: {card.Points}.\n(К сожалению, с картинкой, что-то пошло не так)"
            );
        }
    }

    public async Task GetMyCardsCommand(Message msg)
    {
        _userLastPageCard[msg.Chat.Id] = 1;
        AllUserCardDto cardDto = await _cardRepository.GetAllUserCardAsync(msg.Chat.Id, _userLastPageCard[msg.Chat.Id], 1);
        if (cardDto.Cards.Count < 1 || cardDto.PageCount == 0)
        {
            await bot.SendMessage(msg.Chat.Id, "У вас еще нет карт.");
            return;
        }
        
        
        var card = cardDto.Cards.First();

        await SendCardAsync(msg.Chat.Id, card, 1, cardDto.PageCount);
    }
    
    
    public async Task SendCardAsync(long chatId, CardOutputDto card, int page, int lastPage)
    {
        var buttons = new List<InlineKeyboardButton[]>();
    
        var navButtons = new List<InlineKeyboardButton>();
    
        if (lastPage > 3 && page > 3)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("<< Первая", $"prev_1"));
        }
        
        if (page > 1)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("< Назад", $"prev_{page - 1}"));
        }

        navButtons.Add(InlineKeyboardButton.WithCallbackData($"{page}/{lastPage}", "current_page"));

        if (page < lastPage)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("Вперёд >", $"next_{page + 1}"));
        }
        
        if (page < lastPage - 2 && lastPage > 3)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("Последняя >>", $"next_{lastPage}"));
        }
    
        buttons.Add(navButtons.ToArray());
        
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🍕 Выйти", "exit")
        });

        var inlineKeyboard = new InlineKeyboardMarkup(buttons);
        
        try
        {
            using var httpClient = new HttpClient();
            byte[] imageBytes = await httpClient.GetByteArrayAsync(card.CardPhotoUrl);
            
            using var stream = new MemoryStream(imageBytes);
            await _bot.SendPhoto(
                chatId: chatId,
                photo: InputFile.FromStream(stream, "card.png"),
                caption: $"Карта #{card.Id}\nОчки: {card.Points}\nСоздана {card.GenerationDate:dd/MM/yyyy}",
                replyMarkup: inlineKeyboard
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending photo: {ex.Message}");
            await _bot.SendMessage(
                chatId: chatId,
                text: $"Карта #{card.Id}\nОчки: {card.Points}\nСоздана {card.GenerationDate:dd/MM/yyyy}\n(К сожалению, с картинкой, что-то пошло не так)",
                replyMarkup: inlineKeyboard
            );
        }
    }

}