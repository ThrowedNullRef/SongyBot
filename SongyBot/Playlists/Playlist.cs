using System;
using System.Collections.Generic;
using SongyBot.Songs;

namespace SongyBot.Playlists;

public sealed class Playlist
{
    public Playlist(string? id, string name, List<ISong> songs)
    {
        Id = id ?? Guid.NewGuid().ToString();
        Name = name;
        Songs = songs;
    }

    public string Id { get; set; }

    public List<ISong> Songs { get; set; }

    public string Name { get; set; }

    public Action? SongAdded;

    public void AddSong(ISong song)
    {
        Songs.Add(song);
        SongAdded?.Invoke();
    }
}