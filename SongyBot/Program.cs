using System;
using System.Threading.Tasks;
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

        var songyConfig = new SongyConfig("MTAyNDc4ODExMDExMTkzMjQzNg.Gc6CSM.V8TLwbbm-Dbe2p7WoCGmJq7i3hIdRwVJKp-A0g");

        services.AddSingleton<DiscordSocketClient>()
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
                .AddTransient<SongyCommand, ContinuePlaylistCommand>();

        var store = new DocumentStore
        {
            Urls = new[] { "http://localhost:10001" },
            Database = "SongyBot"
        }.Initialize();

        services.AddTransient(_ =>
        {
            return new Func<IAsyncDocumentSession>(() => store.OpenAsyncSession());
        });

        return services.BuildServiceProvider();
    }
}