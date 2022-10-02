using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord.Audio;

namespace SongyBot.AudioPlaying;

public sealed class SongTransmitter : IAsyncDisposable
{
    private readonly Func<AudioOutStream> _createAudioStream;
    private readonly Func<Task<Stream>> _createSongStreamAsync;

    private AudioStream? _audioStream;
    private Stream? _songStream;

    private readonly CancellationTokenSource _cancellationToken = new ();

    public SongTransmitter(Func<AudioOutStream> createAudioStream, Func<Task<Stream>> createSongStreamAsync)
    {
        _createAudioStream = createAudioStream;
        _createSongStreamAsync = createSongStreamAsync;
    }

    public bool IsPaused { get; set; }

    public async ValueTask DisposeAsync()
    {
        _cancellationToken.Cancel();

        if (_audioStream is not null)
            await _audioStream.DisposeAsync();

        if (_songStream is not null)
            await _songStream.DisposeAsync();
    }

    public async Task TransmitAsync()
    {
        _audioStream = _createAudioStream();
        _songStream = await _createSongStreamAsync();

        var buffer = new byte[4096];
        var read = 0;

        do
        {
            if (_cancellationToken.Token.IsCancellationRequested)
            {
                break;
            }

            if (IsPaused)
            {
                await Task.Delay(10);
                continue;
            }

            read = await _songStream.ReadAsync(buffer, 0, buffer.Length);
            _audioStream.Write(buffer, 0, read);
        } while (read > 0);

        //await _songStream.CopyToAsync(_audioStream, _cancellationToken.Token);
    }
}