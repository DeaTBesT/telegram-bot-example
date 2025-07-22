using Telegram.Bot.Types.ReplyMarkups;

namespace FantasyKingdom.Settings;

public static class InlineKeyboards
{
    public static readonly InlineKeyboardMarkup RegistrationKeyboard = new([
        [InlineKeyboardButton.WithCallbackData("✔", callbackData: "/acceptUsername")]
    ]);

    public static readonly InlineKeyboardMarkup MenuKeyboard = new([
        [
            InlineKeyboardButton.WithCallbackData("\ud83c\udf7aТаверна", callbackData: "/tavern"),
            InlineKeyboardButton.WithCallbackData("\ud83d\udc64Герои", callbackData: "/heroList")
        ],
        [
            InlineKeyboardButton.WithCallbackData("\ud83c\udff0Замок", callbackData: "/kingdom"),
            InlineKeyboardButton.WithCallbackData("🌎Мир", callbackData: "/missionsMenu")
        ],
        //[InlineKeyboardButton.WithCallbackData("💎Магазин", callbackData: "/shop")],
    ]);

    public static readonly InlineKeyboardMarkup TavernMenu = new([
        [
            InlineKeyboardButton.WithCallbackData("🧑Персонал", callbackData: "/hiredRecruits"),
            InlineKeyboardButton.WithCallbackData("👥Рекруты", callbackData: "/recruitList")
        ],
        [InlineKeyboardButton.WithCallbackData("🔙Назад", callbackData: "/menu")]
    ]);

    public static readonly InlineKeyboardMarkup RecruitMenu = new([
        [InlineKeyboardButton.WithCallbackData("\u2714\ufe0fНанять", callbackData: "/hireRecruits")],
        [InlineKeyboardButton.WithCallbackData("🔙Назад", callbackData: "/recruitList")]
    ]);

    public static readonly InlineKeyboardMarkup KickHero = new([
        [InlineKeyboardButton.WithCallbackData("🔴Да", callbackData: "/acceptKickRecruit")],
        [InlineKeyboardButton.WithCallbackData("🔙Назад", callbackData: "/hiredRecruits")]
    ]);

    public static readonly InlineKeyboardMarkup MissionsMenu = new([
        [
            InlineKeyboardButton.WithCallbackData("Зоны", callbackData: "/zones"),
            InlineKeyboardButton.WithCallbackData("Миссии", callbackData: "/missionsList")
        ],
        [InlineKeyboardButton.WithCallbackData("🔙Назад", callbackData: "/menu")],
    ]);

    public static readonly InlineKeyboardMarkup SimpleExit = new([
        [InlineKeyboardButton.WithCallbackData("Выход", callbackData: "/menu")],
    ]);

    // public static readonly InlineKeyboardMarkup GetHireRecruitMarkup(bool isHired)
    // {
    //     string _text = isHired ? "[Нанятый]" : "➕Нанять";
    //     string _callbackData = isHired ? "/notificate Вы_уже_наняли_этого_человека" : "/hireRecruit";
    //
    //     InlineKeyboardMarkup hireRecruitMenu = new InlineKeyboardMarkup(new[]
    //     {
    //         new[] { InlineKeyboardButton.WithCallbackData(_text, callbackData: _callbackData), },
    //         new[] { InlineKeyboardButton.WithCallbackData("🔙Назад", callbackData: "/recruitsMenu") }
    //     });
    //
    //     return hireRecruitMenu;
    // }
}