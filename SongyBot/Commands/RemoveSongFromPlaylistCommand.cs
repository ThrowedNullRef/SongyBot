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

public sealed class RemoveSongFromPlaylistCommand : SongyCommand
{
    private readonly GuildPlayersPool _guildPlayersPool;
    private readonly Func<IAsyncDocumentSession> _createSession;
    public const string PositionOptionName = "position";

    public RemoveSongFromPlaylistCommand( ILogger logger, GuildPlayersPool guildPlayersPool, Func<IAsyncDocumentSession> createSession) : base("remove-song", "removes a song from active playlist", logger)
    {
        _guildPlayersPool = guildPlayersPool;
        _createSession = createSession;
    }

    public override List<SlashCommandOptionBuilder> Options => new()
    {
        new SlashCommandOptionBuilder().WithName(PositionOptionName)
                                       .WithDescription("position of the song to remove")
                                       .WithType(ApplicationCommandOptionType.Integer)
                                       .WithRequired(true)
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

        var positionToRemove = (int)socketCommand.Data.Options.First(o => o.Name == PositionOptionName).Value;

        if (!player.PlaylistSession.Playlist.TryGetSongAtPosition(positionToRemove, out var song))
        {
            await socketCommand.RespondAsync($"There is no song at position {positionToRemove}");
            return;
        }

        player.PlaylistSession.Playlist.RemoveSong(song);

        using var session = _createSession();
        await session.StoreAsync(player.PlaylistSession.Playlist);
        await session.SaveChangesAsync();
    }
}