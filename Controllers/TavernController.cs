using FantasyKingdom.Models;
using FantasyKingdom.Services;
using FantasyKingdom.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FantasyKingdom.Controllers;

public class TavernController
{
    private ITelegramBotClient _bot;

    public TavernController(ITelegramBotClient bot)
    {
        _bot = bot;

        TimeController.OnTick += UpdateDailyRecruits;
    }

    public async Task IndexEdit(CallbackQuery query, UserModel user)
    {
        await _bot.EditMessageText(query.From.Id, query.Message.MessageId,
            $"Вы в таверне, тут можно нанять новых героев",
            replyMarkup: InlineKeyboards.TavernMenu);
    }

    public async void SendRecruitsList(Message msg, UserModel user)
    {
        var recruits = user.Recruits;

        var markup = PaginationKeyboardBuilder.Build(
            items: recruits,
            currentPage: currentPage,
            itemsPerPage: 8,
            itemButtonGenerator: recruit => (
                text: $"{recruit.Name} (⚔{recruit.Attack} 🛡{recruit.Defense})",
                callbackData: $"recruit_{recruit.Id}"
            ),
            pageNavigationPrefix: "recruits_page_"
        );

        int totalPages = (int)Math.Ceiling((double)recruits.Count / 8);

        string messageText = "Доступные рекруты:\n" +
                            $"Страница {currentPage + 1} из {totalPages}\n\n" +
                            "Выберите рекрута для найма:";

        await _bot.EditMessageText(
            chatId,
            messageText,
            replyMarkup: markup);
    }

    private void UpdateDailyRecruits()
    {
           
    }
}