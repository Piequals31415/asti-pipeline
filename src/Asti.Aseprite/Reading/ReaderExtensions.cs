using System.Text;

namespace Asti.Aseprite.Reading;

internal static class ReaderExtensions
{
    public static string ReadAsepriteString(this BinaryReader reader)
    {
        int length = reader.ReadUInt16();
        return Encoding.UTF8.GetString(reader.ReadExactly(length));
    }

    /// <summary>Like ReadBytes, but a wrong length in the file can't cause a huge allocation.</summary>
    public static byte[] ReadExactly(this BinaryReader reader, long count)
    {
        using var result = new MemoryStream();
        var buffer = new byte[(int)Math.Min(count, 81920)];
        while (count > 0)
        {
            var read = reader.Read(buffer, 0, (int)Math.Min(count, buffer.Length));
            if (read == 0)
                throw new EndOfStreamException();
            result.Write(buffer, 0, read);
            count -= read;
        }

        return result.ToArray();
    }
}
