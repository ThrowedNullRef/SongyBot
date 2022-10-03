using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Raven.Client.Documents.Session;
using Serilog;
using SongyBot.AudioPlaying;
using SongyBot.Songs;

namespace SongyBot.Commands;

public sealed class AddSongToPlaylistCommand : SongyCommand
{
    public const string LinkOptionName = "link";
    public const string PositionOptionName = "position";

    private readonly GuildPlayersPool _guildPlayersPool;
    private readonly Func<IAsyncDocumentSession> _createSession;

    public AddSongToPlaylistCommand(ILogger logger, GuildPlayersPool guildPlayersPool, Func<IAsyncDocumentSession> createSession) : base("add-song", "adds a song to active playlist", logger)
    {
        _guildPlayersPool = guildPlayersPool;
        _createSession = createSession;
    }

    public override List<SlashCommandOptionBuilder> Options => new()
    {
        new SlashCommandOptionBuilder().WithName(LinkOptionName)
                                       .WithDescription("link to the song (youtube or spotify)")
                                       .WithType(ApplicationCommandOptionType.String)
                                       .WithRequired(true),
        new SlashCommandOptionBuilder().WithName(PositionOptionName)
                                       .WithDescription("position of the song")
                                       .WithType(ApplicationCommandOptionType.Integer)
    };

    protected override async Task ExecuteInternalAsync(SocketSlashCommand socketCommand)
    {
        if (socketCommand.User is not SocketGuildUser guildUser)
        {
            Logger.Warning("User is no guild user");
            return;
        }

        if (!_guildPlayersPool.TryGetGuildPlayer(guildUser.Guild.Id, out var player) || player.PlaylistSession is null)
        {
            await socketCommand.RespondAsync("There is no active playlist");
            return;
        }

        var link = socketCommand.ReadOptionValue<string>(LinkOptionName)!;
        var position = socketCommand.ReadOptionValue<int?>(PositionOptionName);

        var nextSongPos = player.PlaylistSession.Playlist.Songs.Count > 0 
            ? player.PlaylistSession.Playlist.Songs.Max(s => s.Position) + 1 
            : 0;

        var song = await SongFactory.CreateSongAsync(link, position ?? nextSongPos);
        player.PlaylistSession.Playlist.AddSong(song);

        using var session = _createSession();
        await session.StoreAsync(player.PlaylistSession.Playlist);
        await session.SaveChangesAsync();
    }
}