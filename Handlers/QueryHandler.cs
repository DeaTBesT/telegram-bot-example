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
    MenuController menuController,
    TavernController tavernController,
    HeroController heroController,
    MissionsController missionsController) : IHandler
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
            var parameters = cmdStr.Split("_");
            var command = Utils.ParseQueryCommand(parameters[0]);

            switch (command)
            {
                case QueryCommand.acceptUsername:
                {
                    await registrationController.AcceptNickname(query, user);
                    await menuController.IndexEdit(query, user);
                }
                    break;
                case QueryCommand.menu:
                {
                    await menuController.IndexEdit(query, user);
                }
                    break;
                case QueryCommand.tavern:
                {
                    await tavernController.IndexEdit(query, user);
                }
                    break;
                case QueryCommand.recruitList:
                {
                    await tavernController.SetRecruitsPage(query, "0", user);
                }
                    break;
                case QueryCommand.recruitsPage:
                {
                    await tavernController.SetRecruitsPage(query, parameters[1], user);
                }
                    break;
                case QueryCommand.recruitInfo:
                {
                    await tavernController.ShowRecruitInfo(query, parameters[1], user);
                }
                    break;
                case QueryCommand.hireRecruit:
                {
                    await tavernController.HireRecruit(query, parameters[1], user);
                }
                    break;
                case QueryCommand.heroList:
                {
                    await heroController.IndexEdit(query, user);
                }
                    break;
                case QueryCommand.heroPage:
                {
                    await heroController.SetHeroPage(query, parameters[1], user);
                }
                    break;
                case QueryCommand.heroInfo:
                {
                    await heroController.ShowHeroInfo(query, parameters[1], user);
                }
                    break;
                case QueryCommand.kickHero:
                {
                    await heroController.KickHero(query, parameters[1], user);
                }
                    break;
                case QueryCommand.missionsMenu:
                {
                    await missionsController.IndexEdit(query, user);
                }
                    break;
                case QueryCommand.missionsList:
                {
                    await missionsController.SetMissionsPage(query, "0", user);
                }
                    break;
                case QueryCommand.missionsPage:
                {
                    await missionsController.SetMissionsPage(query, parameters[1], user);
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