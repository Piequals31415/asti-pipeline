using System.Text;

namespace Asti.Aseprite.Tests;

/// <summary>Builds small one-frame files in memory, for cases the samples don't cover.</summary>
internal static class AsepriteBytes
{
    public static AsepriteFile Read(params byte[][] chunks) => Read(1, chunks);

    public static AsepriteFile Read(uint headerFlags, params byte[][] chunks) =>
        AsepriteFile.FromStream(new MemoryStream(File(headerFlags, chunks)));

    public static byte[] File(uint headerFlags, byte[][] chunks, bool oldChunkCount = false) => Build(w =>
    {
        var data = chunks.SelectMany(c => c).ToArray();
        w.Write((uint)(128 + 16 + data.Length));
        w.Write((ushort)0xA5E0);
        w.Write((ushort)1);
        w.Write((ushort)8);
        w.Write((ushort)8);
        w.Write((ushort)32);
        w.Write(headerFlags);
        w.Write(new byte[128 - 18]);

        w.Write((uint)(16 + data.Length));
        w.Write((ushort)0xF1FA);
        w.Write((ushort)chunks.Length);
        w.Write((ushort)100);
        w.Write((ushort)0);
        w.Write(oldChunkCount ? 0u : (uint)chunks.Length);
        w.Write(data);
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
