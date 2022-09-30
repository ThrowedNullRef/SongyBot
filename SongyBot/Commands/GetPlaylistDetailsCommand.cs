using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;
using SongyBot.AudioPlaying;

namespace SongyBot.Commands;

public sealed class GetPlaylistDetailsCommand : SongyCommand
{
    private readonly GuildPlayersPool _guildPlayersPool;

    public GetPlaylistDetailsCommand(ILogger logger, GuildPlayersPool guildPlayersPool) : base ("playlist-details", "get details to the currently active playlist", logger)
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

        if (!_guildPlayersPool.TryGetGuildPlayer(guildUser.Guild.Id, out var player) || player.Playlist is null)
        {
            await socketCommand.RespondAsync("There is no active playlist");
            return;
        }

        var songNames = player.Playlist.Songs.Select(s => s.SongName).ToList();
        await socketCommand.RespondAsync($"Playlist: {player.Playlist.Name} \\\\\\ Songs: {string.Join(",", songNames)}");
    }
}