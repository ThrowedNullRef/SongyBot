using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Raven.Client.Documents.Session;
using Serilog;
using SongyBot.AudioPlaying;
using SongyBot.Playlists;

namespace SongyBot.Commands;

public sealed class NewPlaylistCommand : SongyCommand
{
    public const string NameOption = "name";

    private readonly Func<IAsyncDocumentSession> _createSession;
    private readonly GuildPlayersPool _guildPlayersPool;

    public NewPlaylistCommand(ILogger logger, Func<IAsyncDocumentSession> createSession, GuildPlayersPool guildPlayersPool) : base("new-playlist", "creates a new playlist and selects it", logger)
    {
        _createSession = createSession;
        _guildPlayersPool = guildPlayersPool;
    }

    public override List<SlashCommandOptionBuilder> Options => new ()
    {
        new SlashCommandOptionBuilder().WithName(NameOption)
                                       .WithDescription("name of the playlist")
                                       .WithType(ApplicationCommandOptionType.String)
                                       .WithRequired(true)
    };

    protected override async Task ExecuteInternalAsync(SocketSlashCommand socketCommand)
    {
        if (socketCommand.User is not SocketGuildUser guildUser)
        {
            Logger.Warning("User is no guild user");
            return;
        }

        using var session = _createSession();

        var playlistName = (string) socketCommand.Data.Options.First(o => o.Name == NameOption).Value;

        var playlist = new Playlist(null, playlistName, new ());
        await session.StoreAsync(playlist);
        await session.SaveChangesAsync();

        var player = _guildPlayersPool.GetOrCreateGuildPlayer(guildUser.Guild);
        await player.ChangePlaylistAsync(playlist);

        await socketCommand.RespondAsync($"{playlist.Name} playlist created");
    }
}