using FantasyKingdom.Bot;

namespace FantasyKingdom
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var botController = new BotController(BotConfig.BotToken);
            await Task.Run(() => botController.StartBot());
        }
    }
}