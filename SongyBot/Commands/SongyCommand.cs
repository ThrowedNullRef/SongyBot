using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace SongyBot.Commands;

public abstract class SongyCommand
{
    protected SongyCommand(string commandName, string description, ILogger logger)
    {
        CommandName = commandName;
        Description = description;
        Logger = logger;
    }

    public string CommandName { get; }

    public string Description { get; }

    protected  ILogger Logger { get; }

    public virtual List<SlashCommandOptionBuilder> Options { get; } = new ();

    public async Task ExecuteAsync(SocketSlashCommand socketCommand)
    {
        Logger.Information($"Executing command {CommandName}...");
        await ExecuteInternalAsync(socketCommand);
        Logger.Information($"Executed command {CommandName}");
    }

    protected abstract Task ExecuteInternalAsync(SocketSlashCommand socketCommand);

    protected void ExecuteBackgroundTask(Func<Task> action)
    {
        Task.Run(action);
    }
}