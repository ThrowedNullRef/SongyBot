using System.IO;
using System.Threading.Tasks;

namespace SongyBot.Songs;

public interface ISong
{
    Task<Stream> GetStreamAsync();

    string SongName { get; }
}