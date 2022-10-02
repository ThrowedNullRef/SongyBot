using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SongyBot.Songs;

namespace SongyBot.Playlists;

public sealed class PlaylistSession
{
    public PlaylistSession(Playlist playlist)
    {
        Playlist = playlist;
    }

    public Playlist Playlist { get; }

    public int? CurrentSongPosition { get; private set; }

    public bool IsPaused { get; private set; }

    public event Action<ISong>? OnSongChanged;

    public event Action<bool>? OnPausedChanged;

    public void Next()
    {
        if (Playlist.Songs.Count < 1)
            return;

        var maxSongPos = Playlist.Songs.Max(s => s.Position);
        var currentPos = CurrentSongPosition ?? 0;
        var nextPos = currentPos >= maxSongPos ? 0 : currentPos + 1;
        CurrentSongPosition = nextPos;
        OnSongChanged?.Invoke(Playlist.Songs[nextPos]);
    }

    public void Previous()
    {
        if (Playlist.Songs.Count < 1)
            return;

        var currentIndex = CurrentSongPosition ?? 0;
        var nextIndex = currentIndex == 0 ? Playlist.Songs.Count - 1 : currentIndex - 1;
        CurrentSongPosition = nextIndex;
        OnSongChanged?.Invoke(Playlist.Songs[nextIndex]);
    }

    public void SetPaused(bool isPaused)
    {
        if (IsPaused == isPaused)
            return;

        IsPaused = isPaused;
        OnPausedChanged?.Invoke(isPaused);
    }

    public bool TryGetCurrentSong([NotNullWhen(true)] out ISong? song)
    {
        if (CurrentSongPosition is not null) 
            return Playlist.TryGetSongAtPosition(CurrentSongPosition.Value, out song);

        song = null;
        return false;

    }
}