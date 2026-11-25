using Asti.Aseprite.Model;

namespace Asti.Aseprite.Reading.Chunks;

internal sealed class TagsChunkReader(int frameCount)
{
    private readonly List<AsepriteTag> _tags = [];

    public IReadOnlyList<AsepriteTag> Tags => _tags.AsReadOnly();

    /// <summary>Returns the tags of this chunk.</summary>
    public IReadOnlyList<AsepriteTag> Read(BinaryReader reader)
    {
        int count = reader.ReadUInt16();
        reader.ReadExactly(8);

        var start = _tags.Count;
        for (var i = 0; i < count; i++)
            _tags.Add(ReadTag(reader));
        return _tags.GetRange(start, count);
    }

    private AsepriteTag ReadTag(BinaryReader reader)
    {
        int from = reader.ReadUInt16();
        int to = reader.ReadUInt16();
        var direction = (AsepriteTagDirection)reader.ReadByte();
        int repeat = reader.ReadUInt16();
        reader.ReadExactly(6);
        var color = new AsepriteColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        reader.ReadByte();
        var name = reader.ReadAsepriteString();

        if (from > to || to >= frameCount)
            throw new InvalidDataException($"Tag '{name}': bad frame range {from}-{to} ({frameCount} frames).");
        if (!Enum.IsDefined(direction))
            throw new InvalidDataException($"Tag '{name}': unknown direction {(int)direction}.");

        return new AsepriteTag(name, from, to, direction, repeat, color);
    }
}
