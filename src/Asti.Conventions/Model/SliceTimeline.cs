using Asti.Aseprite.Model;

namespace Asti.Conventions.Model;

/// <summary>Finds the slice key that is valid in a frame.</summary>
internal sealed class SliceTimeline(AsepriteSlice slice, int frameCount)
{
    private readonly AsepriteSliceKey[] _keys = [.. slice.Keys.OrderBy(k => k.Frame)];

    /// <summary>Returns <c>null</c> before the first key and while the slice is hidden.</summary>
    public AsepriteSliceKey? GetKey(int frame)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(frame);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(frame, frameCount);

        var key = Array.FindLast(_keys, k => k.Frame <= frame);
        return key is null || key.IsHidden ? null : key;
    }
}
