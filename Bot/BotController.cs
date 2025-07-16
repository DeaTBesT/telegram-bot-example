using FantasyKingdom.Controllers;
using FantasyKingdom.Core;
using FantasyKingdom.Handlers;
using FantasyKingdom.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FantasyKingdom.Bot;

public class BotController(string token)
{
    private TimeController _timeController;
    private NotificationController _notificationController;
    private RegistrationController _registrationController;
    private MenuController _menuController;
    private TavernController _tavernController;

    private IHandler _consoleHandler;
    private IHandler _messageHandler;
    private IHandler _queryHandler;

    public async Task StartBot()
    {
        var cts = new CancellationTokenSource();
        var bot = new TelegramBotClient(token, cancellationToken: cts.Token);

        InitializeServices(bot);

        var me = await bot.GetMe();
        bot.OnMessage += OnMessage;
        bot.OnUpdate += OnUpdate;
        
        Console.WriteLine($"@{me.Username} is running...");
        await _consoleHandler.Handle();
        await cts.CancelAsync();
        Console.WriteLine($"@{me.Username} is terminated...");
    }

    private void InitializeServices(ITelegramBotClient bot)
    {
        DatabaseService.Run();

        _notificationController = new NotificationController(bot);
        _timeController = new TimeController();
        _registrationController = new RegistrationController(bot);
        _menuController = new MenuController(bot);
        _tavernController = new TavernController(bot);
        
        _consoleHandler = new ConsoleHandler(_timeController, _notificationController);
        _messageHandler = new MessageHandler(bot, _registrationController, _menuController);
        _queryHandler = new QueryHandler(bot, _registrationController, _menuController, _tavernController);
    }

    private async Task OnMessage(Message msg, UpdateType type) =>
        await _messageHandler.Handle(msg, type);

    private async Task OnUpdate(Update update) =>
        await _queryHandler.Handle(update);
}