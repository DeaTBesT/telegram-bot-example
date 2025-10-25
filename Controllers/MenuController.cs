using FantasyKingdom.Models;
using FantasyKingdom.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FantasyKingdom.Controllers;

public class MenuController(ITelegramBotClient bot)
{
    public async Task IndexNew(Message msg, UserModel user)
    {
        await bot.SendMessage(msg.Chat,
            $"Вы в главном меню.\n\n" +
            $"👤Ваше прозвище: {user.UserName}\n\n" +
            $"🆔Ваш личный номер: {user.Id}\n\n" +
            $"\ud83e\ude99Монеты: {user.Data.Coins}\n",
            replyMarkup: InlineKeyboards.MenuKeyboard);
    }

    public async Task IndexEdit(CallbackQuery query, UserModel user)
    {
        await bot.EditMessageText(query.From.Id, query.Message.MessageId,
            $"Вы в главном меню.\n\n" +
            $"👤Ваше прозвище: {user.UserName}\n\n" +
            $"🆔Ваш личный номер: {user.Id}\n\n" +
            $"\ud83e\ude99Монеты: {user.Data.Coins}\n",
            replyMarkup: InlineKeyboards.MenuKeyboard);
    }
}