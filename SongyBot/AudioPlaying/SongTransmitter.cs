using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord.Audio;
using Serilog;

namespace SongyBot.AudioPlaying;

public sealed class SongTransmitter : IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationToken = new ();
    private readonly Func<AudioOutStream> _createAudioStream;
    private readonly Func<Task<Stream>> _createSongStreamAsync;
    private readonly ILogger _logger;

    private readonly Guid _id = Guid.NewGuid();

    private AudioStream? _audioStream;
    private Stream? _songStream;

    public SongTransmitter(Func<AudioOutStream> createAudioStream, Func<Task<Stream>> createSongStreamAsync, ILogger logger)
    {
        _createAudioStream = createAudioStream;
        _createSongStreamAsync = createSongStreamAsync;
        _logger = logger;
    }

    public bool IsPaused { get; set; }

    public async ValueTask DisposeAsync()
    {
        LogDebugMessage("disposed");

        if (_audioStream is not null)
            await _audioStream.DisposeAsync();

        if (_songStream is not null)
            await _songStream.DisposeAsync();
    }

    public void Cancel()
    {
        LogDebugMessage("Cancel token");
        _cancellationToken.Cancel();
    }

    public async Task TransmitAsync()
    {
        LogDebugMessage("Song transmission started");

        if (_audioStream is not null || _songStream is not null)
            return;

        _audioStream = _createAudioStream();
        _songStream = await _createSongStreamAsync();

        var buffer = new byte[4096];
        var read = 0;

        do
        {
            _cancellationToken.Token.ThrowIfCancellationRequested();

            if (IsPaused)
            {
                LogDebugMessage("Song transmission paused");
                await Task.Delay(10);
                continue;
            }

            LogDebugMessage("Song transmission reading bytes");
            read = await _songStream.ReadAsync(buffer, 0, buffer.Length, _cancellationToken.Token);
            LogDebugMessage($"Song transmission finished reading {read} bytes");

            _cancellationToken.Token.ThrowIfCancellationRequested();

            LogDebugMessage($"Song transmission writing bytes");
            await _audioStream.WriteAsync(buffer, 0, read, _cancellationToken.Token);
            LogDebugMessage($"Song transmission finished writing bytes");

        } while (read > 0);
    }

    private void LogDebugMessage(string message) => _logger.Debug($"ST_{_id}: {message}");
}