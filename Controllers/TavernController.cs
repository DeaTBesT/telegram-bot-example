using FantasyKingdom.Models;
using FantasyKingdom.Services;
using FantasyKingdom.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FantasyKingdom.Controllers;

public class TavernController
{
    private const int AvaiableRecruitsMin = 1;
    private const int AvaiableRecruitsMax = 10;

    private ITelegramBotClient _bot;

    public TavernController(ITelegramBotClient bot)
    {
        _bot = bot;
        TimeController.OnTick += OnGameTick;
    }

    private void OnGameTick()
    {
        var users = DatabaseService.GetAllUsers();
        users.ForEach(user =>
        {
            user.Data.TavernInfo.IsUpdate = true;
            DatabaseService.TrySaveUser(user);
        });
    }

    public async Task IndexEdit(CallbackQuery query, UserModel user)
    {
        await _bot.EditMessageText(query.From.Id, query.Message.MessageId,
            $"Вы в таверне, тут можно нанять новых героев",
            replyMarkup: InlineKeyboards.TavernMenu);
    }

    public async Task GetRecruitsList(CallbackQuery query, UserModel user)
    {
        try
        {
            if (user.Data.TavernInfo == null)
            {
                user.Data.TavernInfo = new UserData.TavernPanel();
                DatabaseService.TrySaveUser(user);
            }

            var tavernInfo = user.Data.TavernInfo;

            // Инициализация списка рекрутов, если он null
            if (tavernInfo.AvaiableRecruits == null)
            {
                tavernInfo.AvaiableRecruits = new List<HeroModel>();
                DatabaseService.TrySaveUser(user);
            }

            if (tavernInfo.AvaiableRecruits.Count == 0)
            {
                tavernInfo.IsUpdate = true;
            }

            if (tavernInfo.IsUpdate)
            {
                UpdateUserRecruits(user);
            }

            if (tavernInfo.AvaiableRecruits.Count == 0)
            {
                await _bot.EditMessageText(query.From.Id, query.Message.MessageId,
                    "В данный момент нет доступных рекрутов. Попробуйте позже.", 
                    replyMarkup: InlineKeyboards.SimpleExit);
                Logger.LogWarning($"{user.Id} has no recruits");
                return;
            }

            var markup = PaginationKeyboardBuilder.Build(
                items: user.Data.TavernInfo.AvaiableRecruits,
                currentPage: user.Data.TavernInfo.PageIndex,
                itemsPerPage: 8,
                itemButtonGenerator: recruit => (
                    text: $"{recruit.Name} (💡{recruit.Level} ⚔{recruit.Attack} 🛡{recruit.Defense})",
                    callbackData: $"/recruitInfo_{recruit.Id}"
                ),
                pageNavigationPrefix: "/recruitsPage_",
                exitButtonCallback: "/menu"
            );

            var totalPages = (int)Math.Ceiling((double)user.Data.TavernInfo.AvaiableRecruits.Count / 8);

            var messageText = "Доступные рекруты:\n" +
                              $"Страница {user.Data.TavernInfo.PageIndex + 1} из {totalPages}\n\n" +
                              "Выберите рекрута для найма:";

            await _bot.EditMessageText(query.From.Id,
                query.Message.MessageId,
                messageText,
                replyMarkup: markup);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при получении списка рекрутов.");
        }
    }

    public async Task SetRecruitsPage(CallbackQuery query, string parameters, UserModel user)
    {
        if (!int.TryParse(parameters, out var pageIndex))
        {
            Logger.LogError($"Произошла ошибка при переходе на страницу {user.Id}: Ошибка парсинга {parameters}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на страницу");
            return;
        }

        try
        {
            if (user.Data.TavernInfo == null)
            {
                user.Data.TavernInfo = new UserData.TavernPanel();
                DatabaseService.TrySaveUser(user);
            }

            var info = user.Data.TavernInfo;

            if (info.AvaiableRecruits == null)
            {
                info.AvaiableRecruits = new List<HeroModel>();
                DatabaseService.TrySaveUser(user);
            }

            info.PageIndex = pageIndex;
            DatabaseService.TrySaveUser(user);
            await GetRecruitsList(query, user);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Произошла ошибка при переходе на страницу {user.Id}: {ex.Message}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на страницу");
        }
    }

    private void UpdateUserRecruits(UserModel user)
    {
        var tavernInfo = user.Data.TavernInfo;

        try
        {
            var rnd = new Random();
            tavernInfo.AvaiableRecruits = HeroModel.GenerateNewHeroes(rnd.Next(AvaiableRecruitsMin, 
                AvaiableRecruitsMax));
            tavernInfo.PageIndex = 0;
            tavernInfo.IsUpdate = false;

            DatabaseService.TrySaveUser(user);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error updating recruits for user {user.Id}: {ex.Message}");
            tavernInfo.AvaiableRecruits = new List<HeroModel>();
            tavernInfo = new UserData.TavernPanel();

            DatabaseService.TrySaveUser(user);
        }
    }

    public async Task ShowRecruitInfo(CallbackQuery query, string parameters, UserModel user)
    {
        if (!int.TryParse(parameters, out var recruitIndex))
        {
            Logger.LogError($"Произошла ошибка при открытии информации {user.Id}: Ошибка парсинга {parameters}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на информацию");
            return;
        }

        var recruitInfo = user.Data.TavernInfo.AvaiableRecruits.FirstOrDefault(recruit => recruit.Id == recruitIndex);

        if (recruitInfo == null)
        {
            Logger.LogError($"Произошла ошибка {user.Id}: Ошибка поиска рекрута: {recruitIndex}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла непредвиденная ошибка");
            return;
        }

        var messageText = "Информация о персонаже:\n" +
                          $"{recruitInfo.Name}\n" +
                          $"💡Уровень : {recruitInfo.Level}\n" +
                          $"⚔{recruitInfo.Attack} : 🛡{recruitInfo.Defense}\n" +
                          $"💰Стоимость найма: {recruitInfo.HireCost}";

        InlineKeyboardMarkup recruitMarkup = new([
                [InlineKeyboardButton.WithCallbackData("\u2714\ufe0fНанять", callbackData: $"/hireRecruit_{recruitIndex}")],
                [InlineKeyboardButton.WithCallbackData("🔙Назад", callbackData: "/recruitList")]
             ]);

        await _bot.EditMessageText(query.From.Id,
                query.Message.MessageId,
                messageText,
                replyMarkup: recruitMarkup);
    }

    public async Task HireRecruit(CallbackQuery query, string parameters, UserModel user)
    {
        if (!int.TryParse(parameters, out var recruitIndex))
        {
            Logger.LogError($"Произошла ошибка парсинга у {user.Id}: Текст {parameters}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла ошибка при переходе на информацию");
            return;
        }

        var recruitInfo = user.Data.TavernInfo.AvaiableRecruits.FirstOrDefault(recruit => recruit.Id == recruitIndex);

        if (recruitInfo == null)
        {
            Logger.LogError($"Произошла ошибка {user.Id}: Ошибка поиска рекрута: {recruitIndex}");
            await _bot.AnswerCallbackQuery(query.Id, "Произошла непредвиденная ошибка");
            return;
        }

        if (user.Data.InventoryInfo == null)
        {
            user.Data.InventoryInfo = new UserData.UserInventory();
            DatabaseService.TrySaveUser(user);
        }

        if (user.Data.InventoryInfo.HiredHeroes == null)
        {
            user.Data.InventoryInfo.HiredHeroes = new List<HeroModel>();
            DatabaseService.TrySaveUser(user);

        }

        if (user.Data.Coins < recruitInfo.HireCost)
        {
            await _bot.AnswerCallbackQuery(query.Id, "Недостаточно средств");
            return;
        }

        user.Data.InventoryInfo.TryAddHero(recruitInfo);
        user.Data.TavernInfo.TryRemoveRecruit(recruitInfo);

        user.Data.Coins -= recruitInfo.HireCost;

        DatabaseService.TrySaveUser(user);

        await _bot.AnswerCallbackQuery(query.Id, "✅Персонаж нанят");
        await SetRecruitsPage(query, "0", user);
    }
}