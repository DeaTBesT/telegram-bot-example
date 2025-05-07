namespace FantasyKingdom.Services;

public static class Logger
{
    public static void Log(string message)
    {
        Console.WriteLine(message);
        ResetConsole();
    }

    public static void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        ResetConsole();
    }

    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        ResetConsole();
    }

    private static void ResetConsole()
    {
        Console.ResetColor();
        Console.Write("> ");
    }
}