using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;
using SongyBot.Commands;
using SongyBot.Infrastructure;

namespace SongyBot;

public sealed class Songy : IAsyncDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly CommandsEngine _commandsEngine;
    private readonly SongyConfig _config;
    private readonly ILogger _logger;
    private bool _isRunning;

    public Songy(DiscordSocketClient client, CommandsEngine commandsEngine, SongyConfig config, ILogger logger)
    {
        _client = client;
        _commandsEngine = commandsEngine;
        _config = config;
        _logger = logger;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isRunning)
            return;

        await ShutdownAsync();
    }

    public async Task RunAsync()
    {
        _client.Ready += OnClientReady;
        _client.SlashCommandExecuted += OnClientSlashCommandExecuted;
        _client.Log += OnClientLog;

        await _client.LoginAsync(TokenType.Bot, _config.BotToken);
        await _client.StartAsync();

        _isRunning = true;
        await Task.Delay(-1);
    }

    public async Task ShutdownAsync()
    {
        _client.Ready -= OnClientReady;
        _client.SlashCommandExecuted -= OnClientSlashCommandExecuted;
        _client.Log -= OnClientLog;

        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private Task OnClientLog(LogMessage message)
    {
        Logging.Log(_logger, message);
        return Task.CompletedTask;
    }

    private async Task OnClientSlashCommandExecuted(SocketSlashCommand command)
    {
        await _commandsEngine.RunCommandAsync(command);
    }

    private async Task OnClientReady()
    {
        await _commandsEngine.RegisterCommandsAsync();
    }
}