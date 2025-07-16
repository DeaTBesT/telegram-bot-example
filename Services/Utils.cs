using FantasyKingdom.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FantasyKingdom.Services;

public static class Utils
{
    public static bool TryNormalizeCommand(string prefix, string input, out string result)
    {
        if (input.StartsWith(prefix))
        {
            result = input[1..];
            return true;
        }

        result = input;
        return false;
    }

    public static MessageCommand ParseMessageCommands(string input) => 
        Enum.TryParse<MessageCommand>(input.ToLower(), true, out var command) ? command : MessageCommand.undefined;
    
    public static QueryCommand ParseQueryCommand(string input) => 
        Enum.TryParse<QueryCommand>(input.ToLower(), true, out var command) ? command : QueryCommand.undefined;
}

public static class PaginationKeyboardBuilder
{
    // Универсальный метод для создания клавиатуры с пагинацией
    public static InlineKeyboardMarkup Build<T>(
        List<T> items,
        int currentPage,
        int itemsPerPage,
        Func<T, (string text, string callbackData)> itemButtonGenerator,
        string pageNavigationPrefix = "page_",
        string itemSelectionPrefix = "item_",
        string exitButtonText = "Выход",
        string exitButtonCallback = "exit")
    {
        var totalPages = (int)Math.Ceiling((double)items.Count / itemsPerPage);
        currentPage = Math.Clamp(currentPage, 0, totalPages - 1);

        var pageItems = items
            .Skip(currentPage * itemsPerPage)
            .Take(itemsPerPage)
            .ToList();

        var keyboard = new List<List<InlineKeyboardButton>>();

        // Добавляем кнопки элементов (максимум 8)
        foreach (var item in pageItems.Take(8))
        {
            var (text, callbackData) = itemButtonGenerator(item);
            keyboard.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(text, callbackData)
            });
        }

        // Добавляем кнопки навигации
        var navigationRow = new List<InlineKeyboardButton>();

        if (currentPage > 0)
        {
            navigationRow.Add(InlineKeyboardButton.WithCallbackData("⬅ Назад", $"{pageNavigationPrefix}{currentPage - 1}"));
        }

        if (currentPage < totalPages - 1)
        {
            navigationRow.Add(InlineKeyboardButton.WithCallbackData("Вперед ➡", $"{pageNavigationPrefix}{currentPage + 1}"));
        }

        if (navigationRow.Count > 0)
        {
            keyboard.Add(navigationRow);
        }

        // Добавляем кнопку "Выход"
        keyboard.Add([InlineKeyboardButton.WithCallbackData(exitButtonText, exitButtonCallback)]);

        return new InlineKeyboardMarkup(keyboard);
    }

    // Метод для разбора callback данных пагинации
    public static (string action, int newPage) ParsePageCallback(string callbackData)
    {
        var parts = callbackData.Split('_');
        if (parts.Length < 3) return ("", 0);

        var action = parts[1]; // "prev" или "next"
        var page = int.Parse(parts[2]);

        return (action, page);
    }
}