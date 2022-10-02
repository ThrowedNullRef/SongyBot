using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VideoLibrary;

namespace SongyBot.Songs;

public sealed class YoutubeSong : ISong
{
    public YoutubeSong(string url, string songName, int position)
    {
        Url = url;
        SongName = songName;
        Position = position;
    }

    public string Url { get; set; }

    public string SongName { get; set; }

    public int Position { get; set; }

    public async Task<Stream> GetStreamAsync()
    {
        var directory = new DirectoryInfo(".\\TempVideos");
        if (!directory.Exists)
            directory.Create();

        var video = await YouTube.Default.GetVideoAsync(Url);
        var videoPath = $"{directory.FullName}\\{Guid.NewGuid()}{Path.GetExtension(video.FullName)}";

        var videoBytes = await video.GetBytesAsync();
        await File.WriteAllBytesAsync(videoPath, videoBytes);

        var process = StartFfmpeg(videoPath);
        return process.StandardOutput.BaseStream;
    }

    private static Process StartFfmpeg(string path) =>
        Process.Start(new ProcessStartInfo
        {
            FileName = ".\\ffmpeg-5.1.2-full_build\\bin\\ffmpeg.exe",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        })!;

    public static async Task<YoutubeSong> CreateNewAsync(string url, int position)
    {
        var video = await YouTube.Default.GetVideoAsync(url);
        return new YoutubeSong(url, Path.GetFileNameWithoutExtension(video.FullName), position);
    }
}