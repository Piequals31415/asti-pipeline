using Asti.Aseprite.Model;

namespace Asti.Aseprite.Reading.Chunks;

internal sealed class SliceChunkReader
{
    private const uint HasCenter = 1;
    private const uint HasPivot = 2;

    private readonly List<AsepriteSlice> _slices = [];

    public IReadOnlyList<AsepriteSlice> Slices => _slices.AsReadOnly();

    public AsepriteSlice Read(BinaryReader reader)
    {
        var count = reader.ReadUInt32();
        var flags = reader.ReadUInt32();
        reader.ReadUInt32();
        var name = reader.ReadAsepriteString();

        var keys = new List<AsepriteSliceKey>();
        for (uint i = 0; i < count; i++)
        {
            var frame = ReadSize(reader);
            var bounds = ReadRect(reader);
            AsepriteRect? center = (flags & HasCenter) != 0 ? ReadRect(reader) : null;
            AsepritePoint? pivot = (flags & HasPivot) != 0
                ? new AsepritePoint(reader.ReadInt32(), reader.ReadInt32())
                : null;
            keys.Add(new AsepriteSliceKey(frame, bounds, center, pivot));
        }

        var slice = new AsepriteSlice(name, keys.AsReadOnly());
        _slices.Add(slice);
        return slice;
    }

    private static AsepriteRect ReadRect(BinaryReader reader) =>
        new(reader.ReadInt32(), reader.ReadInt32(), ReadSize(reader), ReadSize(reader));

    private static int ReadSize(BinaryReader reader)
    {
        var value = reader.ReadUInt32();
        if (value > int.MaxValue)
            throw new InvalidDataException($"Slice value {value} is too large.");
        return (int)value;
    }
}
