using FantasyKingdom.Services;
using Telegram.Bot;

namespace FantasyKingdom.Controllers;

public class NotificationController
{
    private ITelegramBotClient _bot;

    public NotificationController(ITelegramBotClient bot)
    {
        _bot = bot;

        TimeController.OnTick += OnTick;
        TimeController.OnMissedTicks += OnMissedTicks;
    }

    private void OnMissedTicks(int ticks)
    {
        var message = $"Прошло несколько дней({ticks}). В мире произошли перемены. В таверне появились новые люди.";

        NotificateUsers(message);   
    }

    private void OnTick()
    {
        var message = "Начался новый день. В мире произошли перемены. В таверне появились новые люди.";

        NotificateUsers(message);   
    }

    public void NotificateUsers(string message)
    {
        var users = DatabaseService.GetAllUsers();

        var usersSendCount = 1;

        users.ForEach(async user =>
        {
            await _bot.SendMessage(user.Id,
                $"{message}");
                
            usersSendCount++;
        });

        Logger.Log($"Сообщение отправленно {usersSendCount} людям. Текст сообщения\n: {message}");
    }

    public async void NotificateUser(long id, string message)
    {
        await _bot.SendMessage(id,
            $"{message}");

        Logger.Log($"Сообщение отправленно человеку с Id: {id}. Текст сообщения\n: {message}");
    }
}