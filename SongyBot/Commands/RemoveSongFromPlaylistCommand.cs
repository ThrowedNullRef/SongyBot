using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;

namespace SongyBot.Commands;

public sealed class RemoveSongFromPlaylistCommand : SongyCommand
{
    public RemoveSongFromPlaylistCommand( ILogger logger) : base("remove-song", "removes a song from active playlist", logger) { }

    protected override Task ExecuteInternalAsync(SocketSlashCommand socketCommand)
    {
        throw new NotImplementedException();
    }
}