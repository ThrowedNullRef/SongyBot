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

    private GuildPlayerSongTransmitter? _songTransmitter;

    private SocketVoiceChannel? _channel;

    private PlaylistSession? _playlistSession;

    public GuildPlayer(SocketGuild guild, ILogger logger)
    {
        _guild = guild;
        _logger = logger;
    }

    public Playlist? Playlist => _playlistSession?.Playlist;

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

        _playlistSession = new PlaylistSession(playlist);
        _playlistSession.OnSongChanged += async song => await PlayListOnSongChanged(song);
    }

    public async Task PlayOnChannelAsync(ulong voiceChannelId)
    {
        await ClearSongTransmissionAsync();

        if (_playlistSession is null)
            throw new InvalidOperationException("no playlist");

        var targetChannel = _guild.VoiceChannels.First(vc => vc.Id == voiceChannelId);
        if (_channel is null || _channel.Id != targetChannel.Id)
        {
            _channel = targetChannel;
            _audioClient = await targetChannel.ConnectAsync();
        }

        _playlistSession.Next();
    }

    private async Task PlayListOnSongChanged(ISong song)
    {
        try
        {
            var songStream = await song.GetStreamAsync();
            var audioOutStream = _audioClient!.CreatePCMStream(AudioApplication.Mixed);
            _songTransmitter = new GuildPlayerSongTransmitter(audioOutStream, songStream);
            await _songTransmitter.TransmitAsync();
            _playlistSession?.Next();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error while streaming song");
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