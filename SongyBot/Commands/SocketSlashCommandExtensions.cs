using System;
using System.Linq;
using Discord.WebSocket;

namespace SongyBot.Commands;

public static class SocketSlashCommandExtensions
{
    public static T? ReadOptionValue<T>(this SocketSlashCommand command, string optionName)
    {
        var optionValue = command.Data.Options.FirstOrDefault(o => o.Name == optionName)?.Value;

        if (optionValue is null)
            return default;

        var desiredType = typeof(T);
        if (desiredType == typeof(bool))
            return (T) (object) Convert.ToBoolean(optionValue);

        // Even though option type is declared as integer discord.net api stores the value as long
        if (desiredType == typeof(int) || desiredType == typeof(int?))
            return (T)(object)Convert.ToInt32(optionValue);

        return (T) command.Data.Options.First(o => o.Name == optionName).Value;
    }
}