using Asti.Aseprite.Model;

namespace Asti.Aseprite.Reading.Chunks;

internal sealed class PaletteChunkReader
{
    private const int MaxOldColors = 256;
    private const int MaxColors = ushort.MaxValue + 1;
    private const ushort HasName = 1;

    private readonly List<AsepriteColor> _oldColors = [];
    private readonly List<AsepriteColor> _colors = [];
    private bool _hasNewPalette;

    /// <summary>The new chunk wins, no matter in which order they are stored.</summary>
    public IReadOnlyList<AsepriteColor> Colors => (_hasNewPalette ? _colors : _oldColors).AsReadOnly();

    public void Read(BinaryReader reader)
    {
        _hasNewPalette = true;
        var size = reader.ReadUInt32();
        var first = reader.ReadUInt32();
        var last = reader.ReadUInt32();
        reader.ReadExactly(8);

        if (size > MaxColors)
            throw new InvalidDataException($"Palette with {size} colors is too large.");
        if (first > last || last >= size)
            throw new InvalidDataException($"Palette range {first}-{last} does not fit into {size} colors.");

        Resize(_colors, (int)size);
        for (var i = (int)first; i <= (int)last; i++)
        {
            var flags = reader.ReadUInt16();
            _colors[i] = new AsepriteColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            if ((flags & HasName) != 0)
                reader.ReadAsepriteString();
        }
    }

    /// <param name="reader">The chunk data.</param>
    /// <param name="maxValue">Highest channel value: 255, or 63 in very old files.</param>
    public void ReadOld(BinaryReader reader, int maxValue)
    {
        int packets = reader.ReadUInt16();
        var index = 0;
        for (var packet = 0; packet < packets; packet++)
        {
            index += reader.ReadByte();
            int count = reader.ReadByte();
            if (count == 0)
                count = MaxOldColors;
            if (index + count > MaxOldColors)
                throw new InvalidDataException($"Old palette has more than {MaxOldColors} colors.");

            if (index + count > _oldColors.Count)
                Resize(_oldColors, index + count);
            for (var i = 0; i < count; i++)
                _oldColors[index++] = new AsepriteColor(Channel(), Channel(), Channel());
        }

        byte Channel()
        {
            var value = reader.ReadByte();
            if (value > maxValue)
                throw new InvalidDataException($"Old palette value {value} is larger than {maxValue}.");
            return (byte)(value * 255 / maxValue);
        }
    }

    private static void Resize(List<AsepriteColor> colors, int size)
    {
        if (colors.Count > size)
            colors.RemoveRange(size, colors.Count - size);
        while (colors.Count < size)
            colors.Add(new AsepriteColor(0, 0, 0));
    }
}
