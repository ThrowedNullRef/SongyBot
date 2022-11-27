using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using SongyBot.AudioPlaying;
using SongyBot.Commands;
using SongyBot.Infrastructure;

namespace SongyBot;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var services = BuildServices();

        var songy = services.GetRequiredService<Songy>();
        await songy.RunAsync();

        return 1;
    }

    public static IServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        var songyConfig = new SongyConfig("<discord_bot_token>");

        var discordConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Debug,
            GatewayIntents = GatewayIntents.AllUnprivileged,
            UseInteractionSnowflakeDate = true
        };

        var discord = new DiscordSocketClient(discordConfig);

        services.AddSingleton(discord)
                .AddSingleton<Songy>()
                .AddSingleton(songyConfig)
                .AddSingleton<CommandsEngine>()
                .AddSingleton<GuildPlayersPool>()
                .AddSingleton(Logging.CreateLogger())
                .AddTransient<SongyCommand, PlaySongCommand>()
                .AddTransient<SongyCommand, AddSongToPlaylistCommand>()
                .AddTransient<SongyCommand, ChangePlaylistCommand>()
                .AddTransient<SongyCommand, NewPlaylistCommand>()
                .AddTransient<SongyCommand, RemoveSongFromPlaylistCommand>()
                .AddTransient<SongyCommand, GetPlaylistDetailsCommand>()
                .AddTransient<SongyCommand, PausePlaylistCommand>()
                .AddTransient<SongyCommand, ResumePlaylistCommand>()
                .AddTransient<SongyCommand, LoopPlaylistCommand>()
                .AddTransient<SongyCommand, NextSongCommand>()
                .AddTransient<SongyCommand, PreviousSongCommand>();

        var store = new DocumentStore
        {
            Urls = new[] { "http://localhost:10001" },
            Database = "SongyBot"
        }.Initialize();

        services.AddTransient<Func<IAsyncDocumentSession>>(_ => () => store.OpenAsyncSession());

        return services.BuildServiceProvider();
    }
}