using Asti.Aseprite.Model;
using Asti.Aseprite.Reading.Chunks;

namespace Asti.Aseprite.Reading;

/// <summary>All chunk readers of one file.</summary>
internal sealed class ChunkReaders
{
    private readonly UserDataChunkReader _userData = new();
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

    public AsepriteUserData? SpriteUserData { get; private set; }

    /// <summary>Returns how to read this chunk, or <c>null</c> if it should be skipped.</summary>
    public Action<BinaryReader>? Find(ChunkType type, int frame, List<AsepriteCel> frameCels) => type switch
    {
        ChunkType.Layer => reader =>
        {
            var layer = Layers.Read(reader);
            _userData.ExpectFor(data => layer.UserData = data);
        },
        ChunkType.Cel => reader =>
        {
            var cel = _cels.Read(reader, frame);
            frameCels.Add(cel);
            _userData.ExpectFor(data => cel.UserData = data);
        },
        ChunkType.Tags => reader => _userData.ExpectFor(Tags.Read(reader)),
        ChunkType.Slice => reader =>
        {
            var slice = Slices.Read(reader);
            _userData.ExpectFor(data => slice.UserData = data);
        },
        ChunkType.Palette => reader => ReadPalette(() => _palette.Read(reader), frame),
        ChunkType.OldPalette => reader => ReadPalette(() => _palette.ReadOld(reader, 255), frame),
        ChunkType.OldPalette64 => reader => ReadPalette(() => _palette.ReadOld(reader, 63), frame),
        ChunkType.Tileset => _ => _userData.Ignore(),
        ChunkType.UserData => _userData.Read,
        _ => null,
    };

    /// <summary>Call after all frames are read.</summary>
    public AsepritePalette Finish()
    {
        var palette = new AsepritePalette(_palette.Colors, _transparentIndex);
        _cels.CreateImages(palette);
        return palette;
    }

    private void ReadPalette(Action read, int frame)
    {
        read();
        if (frame == 0)
            _userData.ExpectFor(data => SpriteUserData = data);
        else
            _userData.Ignore();
    }
}
