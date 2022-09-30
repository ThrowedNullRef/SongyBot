using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;
using SongyBot.Playlists;

namespace SongyBot.AudioPlaying;

public sealed class GuildPlayersPool
{
    private readonly ILogger _logger;
    private readonly Dictionary<ulong, GuildPlayer> _playersByGuildId = new ();

    public GuildPlayersPool(ILogger logger)
    {
        _logger = logger;
    }

    public async Task PlayOnChannelAsync(SocketVoiceChannel channel, Playlist playlist)
    {
        var channelPlayer = GetOrCreateGuildPlayer(channel.Guild);
        await channelPlayer.ChangePlaylistAsync(playlist);

#pragma warning disable CS4014
        Task.Run(async () =>
#pragma warning restore CS4014
                 {
                     await channelPlayer.PlayOnChannelAsync(channel.Id);
                 });
    }

    public GuildPlayer GetOrCreateGuildPlayer(SocketGuild guild)
    {
        if (!_playersByGuildId.TryGetValue(guild.Id, out var guildPlayer))
        {
            guildPlayer = new GuildPlayer(guild, _logger);
            _playersByGuildId.Add(guild.Id, guildPlayer);
        }

        return guildPlayer;
    }

    public bool TryGetGuildPlayer(ulong guildId, [NotNullWhen(true)] out GuildPlayer? guildPlayer)
    {
        return _playersByGuildId.TryGetValue(guildId, out guildPlayer);
    }
}