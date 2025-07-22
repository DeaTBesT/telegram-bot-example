using FantasyKingdom.Models;
using FantasyKingdom.Services;
using FantasyKingdom.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FantasyKingdom.Controllers;
public class MissionsController
{
    private const int AvaiableMissionsMin = 1;
    private const int AvaiableMissionsMax = 10;

    private ITelegramBotClient _bot;

    public MissionsController(ITelegramBotClient bot)
    {
        _bot = bot;
        TimeController.OnTick += OnGameTick;
    }

    private void OnGameTick()
    {
        var users = DatabaseService.GetAllUsers();
        users.ForEach(user =>
        {
            user.Data.MissionInfo.IsUpdate = true;
            DatabaseService.TrySaveUser(user);
        });
    }

    public async Task IndexEdit(CallbackQuery query, UserModel user)
    {
        await _bot.EditMessageText(query.From.Id, query.Message.MessageId,
            $"Открывая карту вы видите зоны, которые могут контролироваться игроком. В самих зонах есть небольшие миссии, которые можно выполнять для получение денег, либо ослабить игрока, контролирующего зону",
            replyMarkup: InlineKeyboards.MissionsMenu);
    }

    public async Task GetMissionsList(CallbackQuery query, UserModel user)
    {
        try
        {
            if (user.Data.MissionInfo == null)
            {
                user.Data.MissionInfo = new UserData.Missions();
                DatabaseService.TrySaveUser(user);
            }

            var info = user.Data.MissionInfo;

            if (info.AvaiableMissions == null)
            {
                info.AvaiableMissions = new List<MissionModel>();
                DatabaseService.TrySaveUser(user);
            }

            if (info.AvaiableMissions.Count == 0)
            {
                info.IsUpdate = true;
            }

            if (info.IsUpdate)
            {
                UpdateUserMissions(user);
            }

            if (info.AvaiableMissions.Count == 0)
            {
                await _bot.EditMessageText(query.From.Id, query.Message.MessageId,
                    "В данный момент нет миссий. Попробуйте позже.",
                    replyMarkup: InlineKeyboards.SimpleExit);
                Logger.LogWarning($"{user.Id} has no missions");
                return;
            }

            var markup = PaginationKeyboardBuilder.Build(
                items: user.Data.MissionInfo.AvaiableMissions,
                currentPage: user.Data.MissionInfo.PageIndex,
                itemsPerPage: 8,
                itemButtonGenerator: mission => (
                    text: $"{mission.Title} (⚔{mission.Difficulty})",
                    callbackData: $"/missionInfo_{mission.Id}"
                ),
                pageNavigationPrefix: "/missionPage_",
                exitButtonCallback: "/menu"
            );

            var totalPages = (int)Math.Ceiling((double)user.Data.MissionInfo.AvaiableMissions.Count / 8);

            var messageText = "Доступные миссии:\n" +
                              $"Страница {user.Data.MissionInfo.PageIndex + 1} из {totalPages}\n\n" +
                              "Выберите миссию:";

            await _bot.EditMessageText(query.From.Id,
                query.Message.MessageId,
                messageText,
                replyMarkup: markup);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при получении списка миссий.");
        }
    }

    public async Task SetMissionsPage(CallbackQuery query, string parameters, UserModel user)
    {
        if (!int.TryParse(parameters, out var pageIndex))
        {
            Logger.LogError($"Произошла ошибка при переходе на страницу {user.Id}: Ошибка парсинга {parameters}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на страницу");
            return;
        }

        try
        { 
            if (user.Data.MissionInfo == null)
            {
                user.Data.MissionInfo = new UserData.Missions();
                DatabaseService.TrySaveUser(user);
            }

            var info = user.Data.MissionInfo;

            if (info.AvaiableMissions == null)
            {
                info.AvaiableMissions = new List<MissionModel>();
                DatabaseService.TrySaveUser(user);
            }

            info.PageIndex = pageIndex;
            DatabaseService.TrySaveUser(user);
            await GetMissionsList(query, user);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Произошла ошибка при переходе на страницу {user.Id}: {ex.Message}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на страницу");
        }
    }

    private void UpdateUserMissions(UserModel user)
    {
        var info = user.Data.MissionInfo;

        try
        {
            var rnd = new Random();
            info.AvaiableMissions = MissionModel.GenerateNewMissions(rnd.Next(AvaiableMissionsMin,
                AvaiableMissionsMax));
            info.PageIndex = 0;
            info.IsUpdate = false;

            DatabaseService.TrySaveUser(user);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error updating missions for user {user.Id}: {ex.Message}");
            info.AvaiableMissions = new List<MissionModel>();
            info = new UserData.Missions();

            DatabaseService.TrySaveUser(user);
        }
    }

    //SelectZone
    //SelectMission
    //JoinMission
    //CaptureZone
}
