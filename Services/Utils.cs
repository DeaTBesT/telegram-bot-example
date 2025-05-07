using FantasyKingdom.Enums;

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