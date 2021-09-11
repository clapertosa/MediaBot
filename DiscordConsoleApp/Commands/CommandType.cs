using System.Linq;

namespace DiscordConsoleApp.Commands
{
    public static class CommandType
    {
        public const string Commands = "m";
        public const string Help = "m-help";
        public const string Search = "m-search";
        public const string GetUserMedia = "m-favorites";
        
        public static string[] GetCommands()
        {
            return typeof(CommandType).GetFields().Select(f => f.GetValue(null)?.ToString()).ToArray();
        }
    }
}