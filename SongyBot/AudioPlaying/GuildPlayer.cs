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
    private readonly ulong _songyUserId;
    private IAudioClient? _audioClient;

    private SocketVoiceChannel? _channel;

    private SongTransmitter? _songTransmitter;

    public GuildPlayer(ulong songyUserId, SocketGuild guild, ILogger logger)
    {
        _songyUserId = songyUserId;
        _guild = guild;
        _logger = logger;
    }

    public PlaylistSession? PlaylistSession { get; private set; }

    public bool HasSongTransmission => _songTransmitter is not null;

    public ValueTask DisposeAsync()
    {
        ClearSongTransmission();
        ClearChannelConnection();
        return ValueTask.CompletedTask;
    }

    public void ChangePlaylist(Playlist playlist)
    {
        ClearSongTransmission();

        PlaylistSession = new PlaylistSession(playlist);
        PlaylistSession.OnSongChanged += OnSongChanged;
        PlaylistSession.OnPausedChanged += OnPausedChanged;
    }

    public async Task PlayOnChannelAsync(ulong voiceChannelId)
    {
        ClearSongTransmission();

        if (PlaylistSession is null)
            throw new InvalidOperationException("no playlist");

        var targetChannel = _guild.VoiceChannels.First(vc => vc.Id == voiceChannelId);
        var isBotConnected = _audioClient is not null && _channel is not null && _channel.ConnectedUsers.Any(u => u.Id == _songyUserId);

        if (!isBotConnected)
        {
            ClearChannelConnection();

            try
            {
                _channel = targetChannel;
                _audioClient = await targetChannel.ConnectAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while connecting to voice channel");
                ClearChannelConnection();
                return;
            }
        }

        PlaylistSession.Next();
    }

    private void OnPausedChanged(bool isPaused)
    {
        if (_songTransmitter is null)
            return;

        _songTransmitter.IsPaused = isPaused;
    }

    private async void OnSongChanged(PlaylistSession session, ISong song, bool isEndOfPlaylist)
    {
        ClearSongTransmission();

        if (!session.IsLooping && isEndOfPlaylist)
            return;

        await PlaySongAsync(song);
    }

    private async Task PlaySongAsync(ISong song)
    {
        var songTransmitter = new SongTransmitter(
            () => _audioClient!.CreatePCMStream(AudioApplication.Mixed),
            song.GetStreamAsync,
            _logger);

        try
        {
            _songTransmitter = songTransmitter;
            await songTransmitter.TransmitAsync();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while streaming song");
            return;
        }
        finally
        {
            await songTransmitter.DisposeAsync();
        }

        PlaylistSession?.Next();
    }

    private void ClearSongTransmission()
    {
        if (_songTransmitter is null)
            return;

        _songTransmitter.Cancel();
        _songTransmitter = null;
    }

    private void ClearChannelConnection()
    {
        if (_audioClient is null)
            return;

        _audioClient.Dispose();
        _audioClient = null;
        _channel = null;
    }
}