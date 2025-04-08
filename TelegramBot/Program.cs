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
            new RequestServiceCardRepository(new HttpClientRequestService(), "http://localhost:5052"),
            new RequestServiceUserRepository(new HttpClientRequestService(), "http://localhost:5052"));
        
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
        }
        
        await bot.SendMessage(msg.Chat, $"{msg.From} said: {msg.Text}");
    }
    
    static async Task OnUpdate(Update update)
    {
        if (update is { CallbackQuery: { } query })
        {
            await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
            await bot.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
        }
    }
    
    static async Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception);
    }
}