using Asti.Aseprite.Model;
using Asti.Aseprite.Reading.Chunks;

namespace Asti.Aseprite.Reading;

/// <summary>All chunk readers of one file.</summary>
internal sealed class ChunkReaders
{
    private readonly PaletteChunkReader _palette = new();
    private readonly CelChunkReader _cels;
    private readonly int _transparentIndex;

    public ChunkReaders(uint headerFlags, AsepriteColorDepth colorDepth, int transparentIndex, int frameCount)
    {
        _transparentIndex = transparentIndex;
        Layers = new LayerChunkReader(headerFlags);
        _cels = new CelChunkReader(colorDepth, Layers.Layers);
        Tags = new TagsChunkReader(frameCount);
    }

    public LayerChunkReader Layers { get; }

    public TagsChunkReader Tags { get; }

    public SliceChunkReader Slices { get; } = new();

    /// <summary>Returns how to read this chunk, or <c>null</c> if it should be skipped.</summary>
    public Action<BinaryReader>? Find(ChunkType type, int frame, List<AsepriteCel> frameCels) => type switch
    {
        ChunkType.Layer => Layers.Read,
        ChunkType.Cel => reader => frameCels.Add(_cels.Read(reader, frame)),
        ChunkType.Tags => reader => Tags.Read(reader),
        ChunkType.Slice => reader => Slices.Read(reader),
        ChunkType.Palette => _palette.Read,
        ChunkType.OldPalette => reader => _palette.ReadOld(reader, 255),
        ChunkType.OldPalette64 => reader => _palette.ReadOld(reader, 63),
        _ => null,
    };

    /// <summary>Call after all frames are read.</summary>
    public AsepritePalette Finish()
    {
        var palette = new AsepritePalette(_palette.Colors, _transparentIndex);
        _cels.CreateImages(palette);
        return palette;
    }
}
