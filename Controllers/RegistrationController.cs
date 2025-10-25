using FantasyKingdom.Enums;
using FantasyKingdom.Models;
using FantasyKingdom.Services;
using FantasyKingdom.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FantasyKingdom.Controllers;

public class RegistrationController(ITelegramBotClient bot)
{
    public async Task Index(Message msg)
    {
        await Task.Run(() =>
        {
            bot.SendMessage(msg.Chat, "✏Напиши свое имя");
            DatabaseService.TryCreateUser(msg.From.Id);
        });
    }

    public async Task SetNickName(Message msg, UserModel user)
    {
        await bot.SendMessage(msg.Chat,
            $"📝Отлично, твоё прозвище : {msg.Text} \nP.S. Напиши другое имя, чтобы заменить его",
            replyMarkup: InlineKeyboards.RegistrationKeyboard);
        user.UserName = msg.Text;
        DatabaseService.TrySaveUser(user);
    }

    public async Task AcceptNickname(CallbackQuery query, UserModel user)
    {
        await bot.AnswerCallbackQuery(query.Id, $"👋Теперь ты можешь начать свою карьеру!");
        user.Data.UserState = UserAction.menu;
        DatabaseService.TrySaveUser(user);
    }
}