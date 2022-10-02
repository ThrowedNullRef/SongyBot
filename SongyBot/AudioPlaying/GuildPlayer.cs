using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Audio;
using Discord.WebSocket;
using Serilog;
using SongyBot.Playlists;
using SongyBot.Songs;

namespace SongyBot.AudioPlaying;

public sealed class GuildPlayer : IAsyncDisposable
{
    private readonly SocketGuild _guild;
    private readonly ILogger _logger;
    private IAudioClient? _audioClient;

    private SongTransmitter? _songTransmitter;

    private SocketVoiceChannel? _channel;

    public GuildPlayer(SocketGuild guild, ILogger logger)
    {
        _guild = guild;
        _logger = logger;
    }

    public PlaylistSession? PlaylistSession { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await ClearSongTransmissionAsync();

        if (_audioClient is not null)
        {
            _audioClient.Dispose();
            _audioClient = null;
        }
    }

    public async Task ChangePlaylistAsync(Playlist playlist)
    {
        await ClearSongTransmissionAsync();

        PlaylistSession = new PlaylistSession(playlist);
        PlaylistSession.OnSongChanged += OnSongChanged;
        PlaylistSession.OnPausedChanged += OnPausedChanged;
    }

    public async Task PlayOnChannelAsync(ulong voiceChannelId)
    {
        await ClearSongTransmissionAsync();

        if (PlaylistSession is null)
            throw new InvalidOperationException("no playlist");

        var targetChannel = _guild.VoiceChannels.First(vc => vc.Id == voiceChannelId);
        if (_channel is null || _channel.Id != targetChannel.Id)
        {
            _channel = targetChannel;
            _audioClient = await targetChannel.ConnectAsync();
        }

        PlaylistSession.Next();
    }

    private void OnPausedChanged(bool isPaused)
    {
        if (_songTransmitter is null)
            return;

        _songTransmitter.IsPaused = isPaused;
    }

    private async void OnSongChanged(ISong song) =>
        await PlaySongAsync(song);

    private async Task PlaySongAsync(ISong song)
    {
        try
        {
            _songTransmitter = new SongTransmitter(
                () => _audioClient!.CreatePCMStream(AudioApplication.Mixed),
                song.GetStreamAsync);

            await _songTransmitter.TransmitAsync();
            await ClearSongTransmissionAsync();
            PlaylistSession?.Next();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while streaming song");
            await ClearSongTransmissionAsync();
        }
    }

    private async Task ClearSongTransmissionAsync()
    {
        if (_songTransmitter is null)
            return;

        await _songTransmitter.DisposeAsync();
        _songTransmitter = null;
    }
}