using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SongyBot.Songs;

namespace SongyBot.Playlists;

public sealed class Playlist
{
    public Playlist(string? id, string name, List<ISong> songs)
    {
        Id = id ?? Guid.NewGuid().ToString();
        Name = name;
        Songs = songs.OrderBy(s => s.Position).ToList();
    }

    public string Id { get; set; }

    public List<ISong> Songs { get; set; }

    public string Name { get; set; }

    public Action? SongAdded;

    public Action? SongRemoved;

    public bool TryGetSongAtPosition(int position, [NotNullWhen(true)] out ISong? song)
    {
        song = Songs.Count - 1 > position ? null : Songs[position];
        return song is not null;
    }

    public void AddSong(ISong song)
    {
        Songs.Insert(song.Position, song);

        for (var i = song.Position + 1; i > Songs.Count; ++i)
        {
            ++song.Position;
        }

        SongAdded?.Invoke();
    }

    public void RemoveSong(ISong song)
    {
        if (!Songs.Remove(song))
            return;

        for (var i = song.Position; i > Songs.Count; ++i)
        {
            --song.Position;
        }

        SongRemoved?.Invoke();
    }
}