using FantasyKingdom.Models;
using FantasyKingdom.Services;
using FantasyKingdom.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FantasyKingdom.Controllers;
public class HeroController
{
    private ITelegramBotClient _bot;

    public HeroController(ITelegramBotClient bot) => 
        _bot = bot;

    public async Task IndexEdit(CallbackQuery query, UserModel user) => 
        await SetHeroPage(query, "0", user);

    public async Task GetHeroList(CallbackQuery query, UserModel user)
    {
        try
        {
            if (user.Data.InventoryInfo == null)
            {
                user.Data.InventoryInfo = new UserData.UserInventory();
                DatabaseService.TrySaveUser(user);
            }

            var inventoryInfo = user.Data.InventoryInfo;

            if (inventoryInfo.HiredHeroes == null)
            {
                inventoryInfo.HiredHeroes = new List<HeroModel>();
                DatabaseService.TrySaveUser(user);
            }

            if (inventoryInfo.HiredHeroes.Count == 0)
            {
                await _bot.EditMessageText(query.From.Id, query.Message.MessageId,
                    "В данный момент нет доступных героев. Попробуйте нанять в таверне.",
                    replyMarkup: InlineKeyboards.SimpleExit);
                Logger.LogWarning($"{user.Id} has no heroes");
                return;
            }

            var markup = PaginationKeyboardBuilder.Build(
                items: user.Data.InventoryInfo.HiredHeroes,
                currentPage: user.Data.InventoryInfo.PageIndex,
                itemsPerPage: 8,
                itemButtonGenerator: recruit => (
                    text: $"{recruit.Name} (💡{recruit.Level} ⚔{recruit.Attack} 🛡{recruit.Defense})",
                    callbackData: $"/heroInfo_{recruit.Id}"
                ),
                pageNavigationPrefix: "/heroPage_",
                exitButtonCallback: "/menu"
            );

            var totalPages = (int)Math.Ceiling((double)user.Data.InventoryInfo.HiredHeroes.Count / 8);

            var messageText = "Доступные герои:\n" +
                              $"Страница {user.Data.InventoryInfo.PageIndex + 1} из {totalPages}\n\n" +
                              "Выберите героя для настройки:";

            await _bot.EditMessageText(query.From.Id,
                query.Message.MessageId,
                messageText,
                replyMarkup: markup);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при получении списка героев.");
        }
    }

    public async Task SetHeroPage(CallbackQuery query, string parameters, UserModel user)
    {
        if (!int.TryParse(parameters, out var pageIndex))
        {
            Logger.LogError($"Произошла ошибка при переходе на страницу {user.Id}: Ошибка парсинга {parameters}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на страницу");
            return;
        }

        try
        {
            if (user.Data.InventoryInfo == null)
            {
                user.Data.InventoryInfo = new UserData.UserInventory();
                DatabaseService.TrySaveUser(user);
            }

            var info = user.Data.InventoryInfo;

            if (info.HiredHeroes == null)
            {
                info.HiredHeroes = new List<HeroModel>();
                DatabaseService.TrySaveUser(user);
            }

            info.PageIndex = pageIndex;
            DatabaseService.TrySaveUser(user);
            await GetHeroList(query, user);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Произошла ошибка при переходе на страницу {user.Id}: {ex.Message}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на страницу");
        }
    }

    public async Task ShowHeroInfo(CallbackQuery query, string parameters, UserModel user)
    {
        if (!int.TryParse(parameters, out var heroIndex))
        {
            Logger.LogError($"Произошла ошибка при открытии информации {user.Id}: Ошибка парсинга {parameters}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на информацию");
            return;
        }

        var heroInfo = user.Data.InventoryInfo.HiredHeroes.FirstOrDefault(hero => hero.Id == heroIndex);

        if (heroInfo == null)
        {
            Logger.LogError($"Произошла ошибка {user.Id}: Ошибка поиска героя: {heroIndex}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла непредвиденная ошибка");
            return;
        }

        var messageText = "Информация о персонаже:\n" +
                          $"{heroInfo.Name}\n" +
                          $"💡Уровень : {heroInfo.Level}\n" +
                          $"⚔{heroInfo.Attack} : 🛡{heroInfo.Defense}\n" +
                          $"💰Стоимость увольнения: {heroInfo.HireCost / 2}";

        InlineKeyboardMarkup heroMarkup = new([
                [InlineKeyboardButton.WithCallbackData("\u2714\ufe0fУволить", callbackData: $"/kickHero_{heroIndex}")],
                [InlineKeyboardButton.WithCallbackData("🔙Назад", callbackData: "/heroList")]
             ]);

        await _bot.EditMessageText(query.From.Id,
                query.Message.MessageId,
                messageText,
                replyMarkup: heroMarkup);
    }

    public async Task KickHero(CallbackQuery query, string parameters, UserModel user)
    {
        if (!int.TryParse(parameters, out var heroIndex))
        {
            Logger.LogError($"Произошла ошибка парсинга у {user.Id}: Текст {parameters}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на информацию");
            return;
        }

        var heroInfo = user.Data.InventoryInfo.HiredHeroes.FirstOrDefault(hero => hero.Id == heroIndex);

        if (heroInfo == null)
        {
            Logger.LogError($"Произошла ошибка {user.Id}: Ошибка поиска героя: {heroIndex}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла непредвиденная ошибка");
            return;
        }

        if (!user.Data.InventoryInfo.TryRemoveHero(heroInfo))
        {
            Logger.LogError($"Произошла ошибка {user.Id}: Героя не существует: {heroIndex}");
            await _bot.AnswerCallbackQuery(query.Id, "Героя не существует");
            return;
        }

        user.Data.Coins += heroInfo.HireCost / 2;
        DatabaseService.TrySaveUser(user);
        
        await _bot.AnswerCallbackQuery(query.Id, "⚠️Персонаж уволен");
        await SetHeroPage(query, "0", user);
    }
}