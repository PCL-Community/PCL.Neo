namespace PCL.Neo.Core.Models.Audio;

public class AudioFileSearcher
{
    private static readonly string[] Extensions = [".mp3", ".wav", ".ogg", ".flac", ".m4a", ".aac"];

    public static List<AudioData> SearchFiles(string path)
    {
        var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
            .Where(s => Extensions.Contains(Path.GetExtension(s).ToLower()));

        return (from file in files
            let extension = Path.GetExtension(file).ToLower()
            select new AudioData
            {
                Name = Path.GetFileNameWithoutExtension(file), Path = file,
                Type = (FileType)Enum.Parse(typeof(FileType), extension.Substring(1).ToUpper())
            }).ToList();
    }
}
