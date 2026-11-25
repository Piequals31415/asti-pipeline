using Asti.Aseprite.Model;

namespace Asti.Aseprite.Reading.Chunks;

internal sealed class LayerChunkReader(uint headerFlags)
{
    private const uint LayerOpacityValid = 1;
    private const uint GroupBlendValid = 2;

    private readonly List<AsepriteLayer> _layers = [];
    private readonly List<AsepriteLayer> _ancestors = [];

    public IReadOnlyList<AsepriteLayer> Layers => _layers.AsReadOnly();

    public AsepriteLayer Read(BinaryReader reader)
    {
        var flags = (AsepriteLayerFlags)reader.ReadUInt16();
        var kind = (AsepriteLayerKind)reader.ReadUInt16();
        int level = reader.ReadUInt16();
        reader.ReadUInt32();
        var blendMode = (AsepriteBlendMode)reader.ReadUInt16();
        var opacity = reader.ReadByte();
        reader.ReadExactly(3);
        var name = reader.ReadAsepriteString();

        if (!Enum.IsDefined(kind))
            throw new InvalidDataException($"Layer '{name}': unknown layer type {(int)kind}.");
        if (!Enum.IsDefined(blendMode))
            throw new InvalidDataException($"Layer '{name}': unknown blend mode {(int)blendMode}.");

        // The file stores garbage (often 0) in fields the header does not mark as valid.
        if (kind == AsepriteLayerKind.Group)
        {
            if ((headerFlags & GroupBlendValid) == 0)
            {
                blendMode = AsepriteBlendMode.Normal;
                opacity = 255;
            }
        }
        else if ((headerFlags & LayerOpacityValid) == 0)
        {
            opacity = 255;
        }

        var parent = FindParent(name, level);
        var layer = new AsepriteLayer(_layers.Count, name, kind, flags, blendMode, opacity, parent);
        _layers.Add(layer);
        _ancestors.Add(layer);
        return layer;
    }

    private AsepriteLayer? FindParent(string name, int level)
    {
        if (level > _ancestors.Count)
            throw new InvalidDataException($"Layer '{name}': level {level} has no parent layer.");

        _ancestors.RemoveRange(level, _ancestors.Count - level);
        if (level == 0)
            return null;

        var parent = _ancestors[^1];
        if (parent.Kind != AsepriteLayerKind.Group)
            throw new InvalidDataException($"Layer '{name}': parent '{parent.Name}' is not a group.");
        return parent;
    }
}
