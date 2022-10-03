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

    public bool TryGetSongAtPosition(int position, [NotNullWhen(true)] out ISong? song)
    {
        song = Songs.FirstOrDefault(song => song.Position == position);
        return song is not null;
    }

    public void AddSong(ISong song)
    {
        if (Songs.Count > 1)
        {
            var maxSongPos = Songs.Max(s => s.Position);

            for (var i = song.Position; i <= maxSongPos; ++i)
            {
                var currentSong = Songs.FirstOrDefault(s => s.Position == i);
                if (currentSong is null)
                    continue;

                ++currentSong.Position;
            }
        }

        var newSongs = new List<ISong>(Songs) { song };
        Songs = newSongs.OrderBy(s => s.Position).ToList();
    }

    public void RemoveSong(ISong song)
    {
        var newSongs = new List<ISong>(Songs);
        var isSongRemoved = newSongs.Remove(song);
        Songs = newSongs;

        if (!isSongRemoved || Songs.Count < 1)
            return;

        var maxSongPos = Songs.Max(s => s.Position);
        for (var i = song.Position + 1; i <= maxSongPos; ++i)
        {
            var currentSong = Songs.FirstOrDefault(s => s.Position == i);
            if (currentSong is null)
                continue;

            --currentSong.Position;
        }
    }
}