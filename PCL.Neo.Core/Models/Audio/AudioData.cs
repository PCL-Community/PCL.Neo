namespace PCL.Neo.Core.Models.Audio;

public enum FileType
{
    MP3,
    WAV,
    OGG,
    FLAC,
    M4A,
    AAC
}

public class AudioData
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public required FileType Type { get; set; }
}
