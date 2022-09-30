using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SongyBot.Commands;

public sealed class CommandsEngine
{
    private readonly DiscordSocketClient _client;
    private readonly Dictionary<string, SongyCommand> _commandsByName;

    public CommandsEngine(IEnumerable<SongyCommand> commands, DiscordSocketClient client)
    {
        _client = client;
        _commandsByName = commands.ToDictionary(command => command.CommandName);
    }

    public async Task RunCommandAsync(SocketSlashCommand socketCommand)
    {
        if (!_commandsByName.TryGetValue(socketCommand.CommandName, out var command))
            throw new InvalidOperationException("Unknown Command");

        await command.ExecuteAsync(socketCommand);
    }

    public async Task RegisterCommandsAsync()
    {
        var slashCommandProperties = _commandsByName.Select(dict => dict.Value)
                                                    .Select(command =>
                                                     {
                                                         var builder = new SlashCommandBuilder();
                                                         builder.WithName(command.CommandName)
                                                                .WithDescription(command.Description);

                                                         if (command.Options.Count > 0)
                                                             builder.AddOptions(command.Options.ToArray());

                                                         return (ApplicationCommandProperties)builder.Build();
                                                     }).ToArray();

        await _client.BulkOverwriteGlobalApplicationCommandsAsync(slashCommandProperties);
    }
}