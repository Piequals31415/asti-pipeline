using Asti.Aseprite.Model;

namespace Asti.Aseprite.Reading.Chunks;

/// <summary>User data chunks have no target in the file, they belong to what was read before them.</summary>
internal sealed class UserDataChunkReader
{
    private const uint HasText = 1;
    private const uint HasColor = 2;

    private readonly Queue<Action<AsepriteUserData>> _targets = [];

    /// <summary>The next user data chunk belongs to this object.</summary>
    public void ExpectFor(Action<AsepriteUserData> target)
    {
        _targets.Clear();
        _targets.Enqueue(target);
    }

    /// <summary>The next user data chunks belong to these tags, one each, in order.</summary>
    public void ExpectFor(IEnumerable<AsepriteTag> tags)
    {
        _targets.Clear();
        foreach (var tag in tags)
            _targets.Enqueue(data => tag.UserData = data);
    }

    /// <summary>The next user data chunks belong to something we don't read (tilesets and their tiles).</summary>
    public void Ignore() => _targets.Clear();

    public void Read(BinaryReader reader)
    {
        var flags = reader.ReadUInt32();
        var text = (flags & HasText) != 0 ? reader.ReadAsepriteString() : null;
        AsepriteColor? color = (flags & HasColor) != 0
            ? new AsepriteColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte())
            : null;

        if (_targets.TryDequeue(out var target))
            target(new AsepriteUserData(text, color));
    }
}
