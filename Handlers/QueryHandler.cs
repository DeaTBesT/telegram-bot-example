using FantasyKingdom.Controllers;
using FantasyKingdom.Core;
using FantasyKingdom.Enums;
using FantasyKingdom.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FantasyKingdom.Handlers;

public class QueryHandler(
    ITelegramBotClient bot,
    RegistrationController registrationController,
    MenuController menuController) : IHandler
{
    private const string CommandsPrefix = "/";

    public async Task Handle(params object[] args)
    {
        var update = args[0] as Update;
        
        if (args[0] is not Update { CallbackQuery: { } query }) // non-null CallbackQuery
        {
            return;
        }

        var user = DatabaseService.GetUserById(query.From.Id);

        if (Utils.TryNormalizeCommand(CommandsPrefix, query.Data, out var cmdStr))
        {
            var command = Utils.ParseQueryCommand(cmdStr);

            switch (command)
            {
                case QueryCommand.acceptUsername:
                    await registrationController.AcceptNickname(query, user);
                    await menuController.IndexEdit(query, user);
                    break;
                default:
                    Logger.LogWarning($"Unknown command: {cmdStr}");
                    break;
            }
        }
    }
}