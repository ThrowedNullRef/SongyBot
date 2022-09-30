using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Serilog;
using SongyBot.AudioPlaying;
using SongyBot.Playlists;

namespace SongyBot.Commands;

public sealed class ChangePlaylistCommand : SongyCommand
{
    public const string NameOption = "name";

    private readonly Func<IAsyncDocumentSession> _createSession;
    private readonly GuildPlayersPool _guildPlayersPool;

    public ChangePlaylistCommand(ILogger logger, GuildPlayersPool guildPlayersPool, Func<IAsyncDocumentSession> createSession) : base("change-playlist", "changes the active playlist", logger)
    {
        _guildPlayersPool = guildPlayersPool;
        _createSession = createSession;
    }

    public override List<SlashCommandOptionBuilder> Options => new()
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

        if (guildUser.VoiceChannel is null)
        {
            Logger.Warning("User is not connected to a voice channel");
            return;
        }

        var playlistName = (string)socketCommand.Data.Options.First(o => o.Name == NameOption).Value;

        using var session = _createSession();

        var playlist = await session.Query<Playlist>().FirstOrDefaultAsync(pl => pl.Name == playlistName);
        if (playlist is null)
        {
            await socketCommand.RespondAsync("Playlist not found");
            return;
        }

        var player = _guildPlayersPool.GetOrCreateGuildPlayer(guildUser.Guild);

        ExecuteBackgroundTask(async () =>
        {
            await player.ChangePlaylistAsync(playlist);
            await player.PlayOnChannelAsync(guildUser.VoiceChannel.Id);
        });

        await socketCommand.RespondAsync($"Switched to playlist {playlist.Name}");
    }
}