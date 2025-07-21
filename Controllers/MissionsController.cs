using FantasyKingdom.Models;
using FantasyKingdom.Services;
using FantasyKingdom.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FantasyKingdom.Controllers;
public class MissionsController
{
    private ITelegramBotClient _bot;

    public MissionsController(ITelegramBotClient bot)
    {
        _bot = bot;
        TimeController.OnTick += OnGameTick;
    }

    private void OnGameTick()
    {
    }

    public async Task IndexEdit(CallbackQuery query, UserModel user)
    {
        await _bot.EditMessageText(query.From.Id, query.Message.MessageId,
            $"Открывая карту вы видите зоны, которые могут контролироваться игроком. В самих зонах есть небольшие миссии, которые можно выполнять для получение денег, либо ослабить игрока, контролирующего зону",
            replyMarkup: InlineKeyboards.MissionsMenu);
    }

    //SelectZone
    //SelectMission
    //JoinMission
    //CaptureZone
}
