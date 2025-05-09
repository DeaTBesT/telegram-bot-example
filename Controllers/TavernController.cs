using FantasyKingdom.Models;
using FantasyKingdom.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FantasyKingdom.Controllers;

public class TavernController(ITelegramBotClient bot)
{
    public async Task IndexEdit(CallbackQuery query, UserModel user)
    {
        await bot.EditMessageText(query.From.Id, query.Message.MessageId,
            $"Вы в таверне, тут можно нанять новых героев",
            replyMarkup: InlineKeyboards.TavernMenu);
    }
}