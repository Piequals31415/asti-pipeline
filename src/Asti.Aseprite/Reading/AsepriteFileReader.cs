using System.Text;
using Asti.Aseprite.Model;

namespace Asti.Aseprite.Reading;

/// <summary>
/// Reads the binary .aseprite format.
/// Spec: https://github.com/aseprite/aseprite/blob/main/docs/ase-file-specs.md
/// </summary>
internal static class AsepriteFileReader
{
    private const ushort HeaderMagic = 0xA5E0;
    private const ushort FrameMagic = 0xF1FA;
    private const int HeaderSize = 128;
    private const int FrameHeaderSize = 16;

    public static AsepriteFile Read(Stream stream)
    {
        
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        try
        {
            return ReadFile(reader);
        }
        catch (EndOfStreamException e)
        {
            throw new InvalidDataException("Unexpected end of file.", e);
        }
    }

    private static AsepriteFile ReadFile(BinaryReader reader)
    {
        
        reader.ReadUInt32(); 
        var magic = reader.ReadUInt16();
        if (magic != HeaderMagic)
            throw new InvalidDataException($"Not an Aseprite file (magic 0x{magic:X4}, expected 0x{HeaderMagic:X4}).");

        int frameCount = reader.ReadUInt16();
        int width = reader.ReadUInt16();
        int height = reader.ReadUInt16();
        var colorDepth = ReadColorDepth(reader.ReadUInt16());
        Skip(reader, HeaderSize - 14); 

        var frames = new AsepriteFrame[frameCount];
        for (var i = 0; i < frameCount; i++)
            frames[i] = ReadFrame(reader, i);

        return new AsepriteFile(width, height, colorDepth, frames);
    }

    private static AsepriteFrame ReadFrame(BinaryReader reader, int index)
    {
        var frameBytes = reader.ReadUInt32(); 
        var magic = reader.ReadUInt16();
        if (magic != FrameMagic)
            throw new InvalidDataException($"Frame {index}: bad magic 0x{magic:X4}, expected 0x{FrameMagic:X4}.");
        if (frameBytes < FrameHeaderSize)
            throw new InvalidDataException($"Frame {index}: size {frameBytes} is smaller than the frame header.");

        reader.ReadUInt16(); 
        int durationMs = reader.ReadUInt16();
        Skip(reader, 2);     
        reader.ReadUInt32(); 

        
        Skip(reader, frameBytes - FrameHeaderSize);

        return new AsepriteFrame(TimeSpan.FromMilliseconds(durationMs));
    }

    private static AsepriteColorDepth ReadColorDepth(ushort bitsPerPixel) => bitsPerPixel switch
    {
        8 => AsepriteColorDepth.Indexed,
        16 => AsepriteColorDepth.Grayscale,
        32 => AsepriteColorDepth.Rgba,
        _ => throw new InvalidDataException($"Unknown color depth {bitsPerPixel}."),
    };

    private static void Skip(BinaryReader reader, long count)
    {
        var stream = reader.BaseStream;
        if (stream.CanSeek)
        {
            if (stream.Position + count > stream.Length)
                throw new EndOfStreamException();
            stream.Seek(count, SeekOrigin.Current);
            return;
        }

        Span<byte> buffer = stackalloc byte[4096];
        while (count > 0)
        {
            var read = stream.Read(buffer[..(int)Math.Min(count, buffer.Length)]);
            if (read == 0)
                throw new EndOfStreamException();
            count -= read;
        }
    }
}
