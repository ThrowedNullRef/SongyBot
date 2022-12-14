using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;
using SongyBot.AudioPlaying;

namespace SongyBot.Commands;

public sealed class ContinuePlaylistCommand : SongyCommand
{
    private readonly GuildPlayersPool _guildPlayersPool;

    public ContinuePlaylistCommand(ILogger logger, GuildPlayersPool guildPlayersPool) : base ("continue-playlist", "get details to the currently active playlist", logger)
    {
        _guildPlayersPool = guildPlayersPool;
    }

    protected override async Task ExecuteInternalAsync(SocketSlashCommand socketCommand)
    {
        if (socketCommand.User is not SocketGuildUser guildUser)
        {
            Logger.Warning("User is no guild user");
            return;
        }

        if (guildUser.VoiceChannel is null)
        {
            Logger.Warning("User is not connected to a voice channel");
            return;
        }

        if (!_guildPlayersPool.TryGetGuildPlayer(guildUser.Guild.Id, out var player) || player.PlaylistSession is null)
        {
            await socketCommand.RespondAsync("There is no active playlist");
            return;
        }

        await player.PlayOnChannelAsync(guildUser.Guild.Id);
        await socketCommand.RespondAsync($"Playlist {player.PlaylistSession.Playlist.Name} continued");
    }
}