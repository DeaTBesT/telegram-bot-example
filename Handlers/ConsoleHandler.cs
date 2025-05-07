using FantasyKingdom.Core;

namespace FantasyKingdom.Handlers;

public class ConsoleHandler : IHandler
{
    private class CommandInfo(Action<string[]> action, string description)
    {
        public Action<string[]> Action { get; } = action;
        public string Description { get; } = description;
    }

    private static readonly Dictionary<string, CommandInfo> Commands = new()
    {
        { "help", new CommandInfo(ExecuteHelp, "Показать эту справку") },
        { "echo", new CommandInfo(ExecuteEcho, "Повторить введённый текст. Использование: echo <текст>") },
        { "exit", new CommandInfo(ExecuteExit, "Выйти из программы") }
    };

    private static bool _isRunning = true;

    public Task Handle(params object[] _)
    {
        while (_isRunning)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input)) continue;

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLower();
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (Commands.TryGetValue(cmd, out var commandInfo))
            {
                commandInfo.Action(args);
            }
            else
            {
                Console.WriteLine($"Команда '{cmd}' не найдена. Введите 'help' для списка команд.");
            }
        }


        return Task.CompletedTask;
    }

    private static void ExecuteHelp(string[] _)
    {
        Console.WriteLine("Доступные команды:");
        Console.WriteLine();

        var maxCommandLength = Commands.Keys.Select(cmd => cmd.Length).Prepend(0).Max();

        // Выводим все команды с описаниями
        foreach (var (name, info) in Commands)
        {
            Console.WriteLine($"  {name.PadRight(maxCommandLength)}  -  {info.Description}");
        }

        Console.WriteLine();
    }

    private static void ExecuteEcho(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Ошибка: Не указан текст для вывода");
            Console.WriteLine("Использование: echo <текст>");
            return;
        }

        Console.WriteLine(string.Join(" ", args));
    }

    private static void ExecuteExit(string[] _) =>
        _isRunning = false;
}