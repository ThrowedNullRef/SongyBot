using System;
using System.Collections.Generic;
using SongyBot.Songs;

namespace SongyBot.Playlists;

public sealed class PlaylistSession
{
    public PlaylistSession(Playlist playlist)
    {
        Playlist = playlist;
        CurrentSongIndex = 0;
    }

    public Playlist Playlist { get; }


    public event Action<ISong>? OnSongChanged;

    public int? CurrentSongIndex { get; private set; }

    public void Next()
    {
        if (Playlist.Songs.Count < 1)
            return;

        var currentIndex = CurrentSongIndex ?? 0;
        var nextIndex = currentIndex > Playlist.Songs.Count ? 0 : currentIndex;
        CurrentSongIndex = nextIndex;
        OnSongChanged?.Invoke(Playlist.Songs[nextIndex]);
    }

    public void Previous()
    {

    }
}