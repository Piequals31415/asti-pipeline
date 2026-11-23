using Asti.Aseprite.Model;
using Asti.Aseprite.Reading;

namespace Asti.Aseprite;

/// <summary>A parsed .aseprite / .ase file. Plain data, no MonoGame.</summary>
public sealed class AsepriteFile
{
    internal AsepriteFile(int width, int height, AsepriteColorDepth colorDepth, IReadOnlyList<AsepriteFrame> frames)
    {
        Width = width;
        Height = height;
        ColorDepth = colorDepth;
        Frames = frames;
    }

    /// <summary>Canvas width in pixels.</summary>
    public int Width { get; }

    /// <summary>Canvas height in pixels.</summary>
    public int Height { get; }

    public AsepriteColorDepth ColorDepth { get; }

    public IReadOnlyList<AsepriteFrame> Frames { get; }

    /// <summary>Reads an Aseprite file from disk.</summary>
    /// <exception cref="InvalidDataException">The file is not a valid Aseprite file.</exception>
    public static AsepriteFile FromFile(string path)
    {
        using var stream = File.OpenRead(path);
        try
        {
            return FromStream(stream);
        }
        catch (InvalidDataException e)
        {
            throw new InvalidDataException($"{path}: {e.Message}", e);
        }
    }

    /// <summary>Reads an Aseprite file from a stream. The stream is not closed.</summary>
    /// <exception cref="InvalidDataException">The data is not a valid Aseprite file.</exception>
    public static AsepriteFile FromStream(Stream stream) => AsepriteFileReader.Read(stream);
}
