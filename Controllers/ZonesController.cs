using Telegram.Bot;

namespace FantasyKingdom.Controllers;

public class ZonesController
{
    private ITelegramBotClient _bot;
    
    public ZonesController(ITelegramBotClient bot)
    {
        _bot = bot;
        TimeController.OnTick += OnGameTick;
    }

    private void OnGameTick()
    {
        //Update zones info
    }
}