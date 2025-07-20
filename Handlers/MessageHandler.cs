using FantasyKingdom.Controllers;
using FantasyKingdom.Services;
using FantasyKingdom.Core;
using FantasyKingdom.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FantasyKingdom.Handlers;

public class MessageHandler(
    ITelegramBotClient bot,
    RegistrationController registrationController,
    MenuController menuController,
    NotificationController notificationController) : IHandler
{
    private const string CommandsPrefix = "/";

    public async Task Handle(params object[] args)
    {
        var msg = args[0] as Message;

        if (msg is { Text: null })
        {
            return;
        }

        var user = DatabaseService.GetUserById(msg.From.Id);

        if (Utils.TryNormalizeCommand(CommandsPrefix, msg.Text, out var cmdStr))
        {
            var command = Utils.ParseMessageCommands(cmdStr);

            switch (command)
            {
                case MessageCommand.start:
                {
                    await registrationController.Index(msg);
                }
                    break;
                case MessageCommand.menu:
                {
                    await menuController.IndexNew(msg, user);
                }
                    break;
                default:
                {
                    Logger.LogWarning($"Unknown command: {cmdStr}");
                }
                    break;
            }
        }
        else
        {
            if (user == null)
            {
                Logger.LogWarning($"User is null: {cmdStr}");
                notificationController.NotificateUser(msg.From.Id, $"Сначала зарегестрируйтесь (/start)");
                return;
            }

            if (string.IsNullOrEmpty(user.UserName))
            {
                Logger.LogWarning($"Unknown command: {cmdStr}");
            }

            switch (user.Data.UserState)
            {
                case UserAction.registration:
                {
                    await registrationController.SetNickName(msg, user);
                }
                    break;
                default:
                {
                    Logger.LogWarning($"Unknown command: {cmdStr}");
                }
                    break;
            }
        }
    }
}