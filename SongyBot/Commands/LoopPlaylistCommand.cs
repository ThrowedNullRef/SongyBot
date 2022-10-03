using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;
using SongyBot.AudioPlaying;

namespace SongyBot.Commands;

public sealed class LoopPlaylistCommand : SongyCommand
{
    public const string LoopOptionName = "status";

    private readonly GuildPlayersPool _guildPlayersPool;

    public LoopPlaylistCommand(ILogger logger, GuildPlayersPool guildPlayersPool) : base("loop", "sets looping for this playlist", logger)
    {
        _guildPlayersPool = guildPlayersPool;
    }

    public override List<SlashCommandOptionBuilder> Options => new ()
    {
        new SlashCommandOptionBuilder().WithName(LoopOptionName)
                                       .WithDescription("should the playlist loop")
                                       .WithType(ApplicationCommandOptionType.Integer)
                                       .WithRequired(true)
                                       .AddChoice("on", 1)
                                       .AddChoice("off", 0)
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

        var isLooping = socketCommand.ReadOptionValue<bool>(LoopOptionName);

        player.PlaylistSession.SetLooping(isLooping);

        await socketCommand.RespondAsync("Playlist looping is " + (isLooping ? "on" : "off"));
    }
}