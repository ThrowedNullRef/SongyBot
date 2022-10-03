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
    private readonly DiscordSocketClient _discordClient;
    private readonly Dictionary<ulong, GuildPlayer> _playersByGuildId = new ();

    public GuildPlayersPool(ILogger logger, DiscordSocketClient discordClient)
    {
        _logger = logger;
        _discordClient = discordClient;
    }

    public GuildPlayer GetOrCreateGuildPlayer(SocketGuild guild)
    {
        if (!_playersByGuildId.TryGetValue(guild.Id, out var guildPlayer))
        {
            guildPlayer = new GuildPlayer(_discordClient.CurrentUser.Id, guild, _logger);
            _playersByGuildId.Add(guild.Id, guildPlayer);
        }

        return guildPlayer;
    }

    public bool TryGetGuildPlayer(ulong guildId, [NotNullWhen(true)] out GuildPlayer? guildPlayer)
    {
        return _playersByGuildId.TryGetValue(guildId, out guildPlayer);
    }
}