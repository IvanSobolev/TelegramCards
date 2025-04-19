using System.Diagnostics.SymbolStore;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.ApiRepository.Implementations;
using TelegramBot.ApiRepository.Interfaces;
using TelegramBot.Entity.Dtos;
using TelegramBot.Services.Implementations;

namespace TelegramBot;

class Program
{
    private static TelegramBotClient bot;
    private static CommandHandler _commandHandler;

    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        bot = new TelegramBotClient(File.ReadLines("../../../TOKEN.txt").First());
        var me = await bot.GetMe();
        _commandHandler = new CommandHandler(bot,
            new RequestServiceCardRepository(new HttpClientRequestService(), "http://192.168.1.249:5000"),
            new RequestServiceUserRepository(new HttpClientRequestService(), "http://192.168.1.249:5000"));
        
        bot.OnMessage += OnMessage;
        bot.OnUpdate += OnUpdate;
        bot.OnError += OnError;

        Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
        Console.ReadLine();
        cts.Cancel();
    }
    
    static async Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.Text is null) return;
        Console.WriteLine($"Received {type} '{msg.Text}' in {msg.Chat}");

        switch (msg.Text)
        {
            case ("/start"):
                await _commandHandler.StartCommand(msg);
                break;
            case ("💎 Получить карту"):
                await _commandHandler.GenerateCardCommand(msg);
                break;
            case ("🧊 Посмотреть карты"):
                await _commandHandler.GetMyCardsCommand(msg);
                break;
            case ("table"):
                await _commandHandler.ShowCardsTableAsync(msg.Chat.Id);
                break;
            default:
                await _commandHandler.NoCommandMessage(msg);
                break;
        }
    }
    
    static async Task OnUpdate(Update update)
    {
        if (update is { CallbackQuery: { } query })
        {
            switch (query.Data!.Split('_')[0])
            {
                case ("zero"):
                    await bot.AnswerCallbackQuery(query.Id, $"❌❌❌");
                    break;
                case ("card"):
                    await _commandHandler.GetAnotherCardButtonAsync(query);
                    break;
                case ("exit"):
                    await _commandHandler.ExitSliderButtonAsync(query);
                    break;
                case ("send"):
                    await _commandHandler.SendCardButtonAsync(query);
                    break;
                case ("getcard"):
                    await _commandHandler.AcceptCardButtonAsync(query);
                    break;
                case ("table"):
                    await _commandHandler.HandleTableNavigationAsync(query);
                    break;
                case ("view"):
                    await _commandHandler.HandleCardViewAsync(query);
                    break;
                case ("tableview"):
                    await _commandHandler.ShowTableFormatButtonAsync(query);
                    break;
            }
        }
    }
    
    static async Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception);
    }
}