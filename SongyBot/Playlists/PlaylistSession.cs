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

    public bool IsLooping { get; private set; }

    public event Action<PlaylistSession, ISong, bool>? OnSongChanged;

    public event Action<bool>? OnPausedChanged;

    public event Action<bool>? OnLoopingChanged;

    public void Next()
    {
        if (Playlist.Songs.Count < 1)
            return;

        var maxSongPos = Playlist.Songs.Max(s => s.Position);
        var nextPos = CurrentSongPosition is null 
            ? 0 
            : CurrentSongPosition.Value >= maxSongPos ? 0 : CurrentSongPosition.Value + 1;

        var isEndOfPlaylist = CurrentSongPosition is not null && nextPos == 0;

        CurrentSongPosition = nextPos;
        OnSongChanged?.Invoke(this, Playlist.Songs[nextPos], isEndOfPlaylist);
    }

    public void Previous()
    {
        if (Playlist.Songs.Count < 1)
            return;

        var currentIndex = CurrentSongPosition ?? 0;
        var nextIndex = currentIndex == 0 ? Playlist.Songs.Count - 1 : currentIndex - 1;
        CurrentSongPosition = nextIndex;
        OnSongChanged?.Invoke(this, Playlist.Songs[nextIndex], false);
    }

    public void SetPaused(bool isPaused)
    {
        if (IsPaused == isPaused)
            return;

        IsPaused = isPaused;
        OnPausedChanged?.Invoke(isPaused);
    }

    public void SetLooping(bool isLooping)
    {
        if (IsLooping == isLooping)
            return;

        IsLooping = isLooping;
        OnLoopingChanged?.Invoke(isLooping);
    }

    public bool TryGetCurrentSong([NotNullWhen(true)] out ISong? song)
    {
        if (CurrentSongPosition is not null) 
            return Playlist.TryGetSongAtPosition(CurrentSongPosition.Value, out song);

        song = null;
        return false;
    }
}