using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord.Audio;

namespace SongyBot.AudioPlaying;

public sealed class GuildPlayerSongTransmitter : IAsyncDisposable
{
    private readonly AudioOutStream _audioOutStream;
    private readonly Stream _songStream;
    private readonly CancellationTokenSource _cancellationToken = new ();

    public GuildPlayerSongTransmitter(AudioOutStream audioOutStream, Stream songStream)
    {
        _audioOutStream = audioOutStream;
        _songStream = songStream;
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationToken.Cancel();
        await _audioOutStream.DisposeAsync();
        await _songStream.DisposeAsync();
    }

    public async Task TransmitAsync()
    {
        await _songStream.CopyToAsync(_audioOutStream, _cancellationToken.Token);
    }
}