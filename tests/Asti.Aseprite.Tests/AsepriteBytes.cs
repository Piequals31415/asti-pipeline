using System.Text;

namespace Asti.Aseprite.Tests;

/// <summary>Builds small files in memory, for cases the samples don't cover.</summary>
internal static class AsepriteBytes
{
    public static AsepriteFile Read(params byte[][] chunks) => Read(1, chunks);

    public static AsepriteFile Read(uint headerFlags, params byte[][] chunks) =>
        AsepriteFile.FromStream(new MemoryStream(File(headerFlags, chunks)));

    /// <summary>Reads a file with one frame per array of chunks.</summary>
    public static AsepriteFile ReadFrames(int colorDepth, params byte[][][] frames) =>
        AsepriteFile.FromStream(new MemoryStream(File(1, frames, colorDepth)));

    public static byte[] File(uint headerFlags, byte[][] chunks, bool oldChunkCount = false) =>
        File(headerFlags, [chunks], 32, oldChunkCount);

    private static byte[] File(uint headerFlags, byte[][][] frames, int colorDepth, bool oldChunkCount = false) =>
        Build(w =>
        {
            w.Write((uint)(128 + frames.Sum(f => 16 + f.Sum(c => c.Length))));
            w.Write((ushort)0xA5E0);
            w.Write((ushort)frames.Length);
            w.Write((ushort)8);
            w.Write((ushort)8);
            w.Write((ushort)colorDepth);
            w.Write(headerFlags);
            w.Write(new byte[128 - 18]);

            foreach (var chunks in frames)
            {
                w.Write((uint)(16 + chunks.Sum(c => c.Length)));
                w.Write((ushort)0xF1FA);
                w.Write((ushort)chunks.Length);
                w.Write((ushort)100);
                w.Write((ushort)0);
                w.Write(oldChunkCount ? 0u : (uint)chunks.Length);
                foreach (var chunk in chunks)
                    w.Write(chunk);
            }
        });

    public static byte[] Chunk(ushort type, Action<BinaryWriter> write)
    {
        var data = Build(write);
        return Build(w =>
        {
            w.Write((uint)(data.Length + 6));
            w.Write(type);
            w.Write(data);
        });
    }

    public static byte[] Layer(string name, int level = 0, int kind = 0, int flags = 3, int blend = 0,
        byte opacity = 255) => Chunk(0x2004, w =>
    {
        w.Write((ushort)flags);
        w.Write((ushort)kind);
        w.Write((ushort)level);
        w.Write(0u);
        w.Write((ushort)blend);
        w.Write(opacity);
        w.Write(new byte[3]);
        WriteString(w, name);
    });

    public static byte[] Cel(int type, Action<BinaryWriter> write, int layer = 0, int x = 0, int y = 0,
        byte opacity = 255, int zIndex = 0) => Chunk(0x2005, w =>
    {
        w.Write((ushort)layer);
        w.Write((short)x);
        w.Write((short)y);
        w.Write(opacity);
        w.Write((ushort)type);
        w.Write((short)zIndex);
        w.Write(new byte[5]);
        write(w);
    });

    public static byte[] RawCel(int width, int height, params byte[] pixels) => Cel(0, w =>
    {
        w.Write((ushort)width);
        w.Write((ushort)height);
        w.Write(pixels);
    });

    public static byte[] CompressedCel(int width, int height, params byte[] data) => Cel(2, w =>
    {
        w.Write((ushort)width);
        w.Write((ushort)height);
        w.Write(data);
    });

    public static byte[] LinkedCel(int frame, int x = 0, byte opacity = 255) =>
        Cel(1, w => w.Write((ushort)frame), x: x, opacity: opacity);

    public static byte[] Tags(params string[] names) => Chunk(0x2018, w =>
    {
        w.Write((ushort)names.Length);
        w.Write(new byte[8]);
        foreach (var name in names)
            WriteTag(w, name);
    });

    public static void WriteTag(BinaryWriter w, string name, int from = 0, int to = 0, int direction = 0)
    {
        w.Write((ushort)from);
        w.Write((ushort)to);
        w.Write((byte)direction);
        w.Write((ushort)0);
        w.Write(new byte[6]);
        w.Write(new byte[] { 1, 2, 3, 0 });
        WriteString(w, name);
    }

    public static void WriteString(BinaryWriter w, string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        w.Write((ushort)bytes.Length);
        w.Write(bytes);
    }

    private static byte[] Build(Action<BinaryWriter> write)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        write(writer);
        return stream.ToArray();
    }
}
