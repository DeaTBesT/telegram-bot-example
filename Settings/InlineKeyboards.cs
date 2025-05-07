using Telegram.Bot.Types.ReplyMarkups;

namespace FantasyKingdom.Settings;

public static class InlineKeyboards
{
    public static InlineKeyboardMarkup RegistrationKeyboard = new([
        [InlineKeyboardButton.WithCallbackData("✔", callbackData: "/acceptUsername")]
    ]); 
    
    public static InlineKeyboardMarkup MenuKeyboard = new([
        [InlineKeyboardButton.WithCallbackData("✔", callbackData: "/menu")]
    ]);
}