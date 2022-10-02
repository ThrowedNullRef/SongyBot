using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;
using SongyBot.AudioPlaying;
using SongyBot.Playlists;
using SongyBot.Songs;

namespace SongyBot.Commands;

public sealed class PlaySongCommand : SongyCommand
{
    public const string LinkOptionName = "link";

    private readonly GuildPlayersPool _playersPool;

    public PlaySongCommand(ILogger logger, GuildPlayersPool playersPool) : base("play", "plays a song", logger)
    {
        _playersPool = playersPool;
    }

    public override List<SlashCommandOptionBuilder> Options => new ()
    {
        new SlashCommandOptionBuilder().WithName(LinkOptionName)
                                       .WithDescription("link to the song (youtube or spotify)")
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

        var link = (string)socketCommand.Data.Options.First(o => o.Name == LinkOptionName).Value;

        var song = await SongFactory.CreateSongAsync(link, 0);
        var playlist = new Playlist(null, "SINGLE_SONG", new List<ISong> { song });

        var player = _playersPool.GetOrCreateGuildPlayer(guildUser.Guild);

        ExecuteBackgroundTask(async () =>
        {
            await player.ChangePlaylistAsync(playlist);
            await player.PlayOnChannelAsync(guildUser.VoiceChannel.Id);
        });
        

        await socketCommand.RespondAsync("Here is your song");
    }
}