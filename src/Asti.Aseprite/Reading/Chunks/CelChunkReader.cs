using System.IO.Compression;
using Asti.Aseprite.Model;

namespace Asti.Aseprite.Reading.Chunks;

internal sealed class CelChunkReader(AsepriteColorDepth colorDepth, IReadOnlyList<AsepriteLayer> layers)
{
    private const int RawImage = 0;
    private const int LinkedCel = 1;
    private const int CompressedImage = 2;
    private const int CompressedTilemap = 3;

    private readonly Dictionary<(int Frame, int Layer), AsepriteCel> _cels = [];
    private readonly List<(AsepriteCel Cel, int Width, int Height, byte[] Pixels)> _images = [];
    private readonly List<AsepriteCel> _links = [];

    public AsepriteCel Read(BinaryReader reader, int frame)
    {
        int layerIndex = reader.ReadUInt16();
        int x = reader.ReadInt16();
        int y = reader.ReadInt16();
        var opacity = reader.ReadByte();
        int type = reader.ReadUInt16();
        int zIndex = reader.ReadInt16();
        reader.ReadExactly(5);

        if (layerIndex >= layers.Count)
            throw new InvalidDataException($"Cel points to layer {layerIndex}, but there are only {layers.Count} layers.");

        int? linkedFrame = type == LinkedCel ? reader.ReadUInt16() : null;
        var cel = new AsepriteCel(layers[layerIndex], x, y, opacity, zIndex, linkedFrame);
        if (!_cels.TryAdd((frame, layerIndex), cel))
            throw new InvalidDataException($"Layer '{cel.Layer.Name}' has more than one cel in this frame.");

        switch (type)
        {
            case RawImage:
            case CompressedImage:
                int width = reader.ReadUInt16();
                int height = reader.ReadUInt16();
                _images.Add((cel, width, height, ReadPixels(reader, width, height, type == CompressedImage)));
                break;
            case LinkedCel:
                // Only earlier frames, so links can't form a circle.
                if (linkedFrame >= frame || !_cels.ContainsKey((linkedFrame!.Value, layerIndex)))
                    throw new InvalidDataException(
                        $"Cel on layer '{cel.Layer.Name}' links to frame {linkedFrame}, which has no earlier cel.");
                _links.Add(cel);
                break;
            case CompressedTilemap:
                break;
            default:
                throw new InvalidDataException($"Unknown cel type {type}.");
        }

        return cel;
    }

    /// <summary>Creates the images. Runs after all frames are read, because it needs the final palette.</summary>
    public void CreateImages(AsepritePalette palette)
    {
        foreach (var (cel, width, height, pixels) in _images)
        {
            var rgba = PixelConverter.ToRgba(pixels, colorDepth, palette, cel.Layer.IsBackground);
            cel.Image = new AsepriteImage(width, height, rgba);
        }

        foreach (var cel in _links)
            cel.Image = _cels[(cel.LinkedFrame!.Value, cel.Layer.Index)].Image;
    }

    private byte[] ReadPixels(BinaryReader reader, int width, int height, bool compressed)
    {
        if (width == 0 || height == 0 || (long)width * height * 4 > Array.MaxLength)
            throw new InvalidDataException($"Bad cel size {width}x{height}.");

        var expected = (long)width * height * ((int)colorDepth / 8);
        using var pixels = compressed
            ? new BinaryReader(new ZLibStream(reader.BaseStream, CompressionMode.Decompress, leaveOpen: true))
            : reader;
        try
        {
            return pixels.ReadExactly(expected);
        }
        catch (EndOfStreamException e)
        {
            throw new InvalidDataException($"Cel {width}x{height} has too few pixels.", e);
        }
    }
}
