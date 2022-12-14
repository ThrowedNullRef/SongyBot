using System.Threading.Tasks;

namespace SongyBot.Songs;

public static class SongFactory
{
    public static async Task<ISong> CreateSongAsync(string input, int position)
    {
        return await YoutubeSong.CreateNewAsync(input, position);
    }
}