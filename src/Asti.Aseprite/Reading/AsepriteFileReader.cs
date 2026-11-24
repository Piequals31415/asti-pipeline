using System.Text;
using Asti.Aseprite.Model;
using Asti.Aseprite.Reading.Chunks;

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
    private const int ChunkHeaderSize = 6;

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
        var flags = reader.ReadUInt32();
        Skip(reader, 10);
        int transparentIndex = reader.ReadByte();
        Skip(reader, HeaderSize - 29);

        var layers = new LayerChunkReader(flags);
        var palette = new PaletteChunkReader();
        var frames = new AsepriteFrame[frameCount];
        for (var i = 0; i < frameCount; i++)
            frames[i] = ReadFrame(reader, i, layers, palette);

        return new AsepriteFile(width, height, colorDepth, frames, layers.Layers,
            new AsepritePalette(palette.Colors, transparentIndex));
    }

    private static AsepriteFrame ReadFrame(BinaryReader reader, int index, LayerChunkReader layers,
        PaletteChunkReader palette)
    {
        var frameBytes = reader.ReadUInt32(); 
        var magic = reader.ReadUInt16();
        if (magic != FrameMagic)
            throw new InvalidDataException($"Frame {index}: bad magic 0x{magic:X4}, expected 0x{FrameMagic:X4}.");
        if (frameBytes < FrameHeaderSize)
            throw new InvalidDataException($"Frame {index}: size {frameBytes} is smaller than the frame header.");

        uint oldChunkCount = reader.ReadUInt16();
        int durationMs = reader.ReadUInt16();
        Skip(reader, 2);
        var chunkCount = reader.ReadUInt32();
        if (chunkCount == 0)
            chunkCount = oldChunkCount;

        long remaining = frameBytes - FrameHeaderSize;
        for (uint i = 0; i < chunkCount; i++)
            remaining -= ReadChunk(reader, index, i, remaining, layers, palette);
        Skip(reader, remaining);

        return new AsepriteFrame(TimeSpan.FromMilliseconds(durationMs));
    }

    private static long ReadChunk(BinaryReader reader, int frame, uint index, long remaining,
        LayerChunkReader layers, PaletteChunkReader palette)
    {
        if (remaining < ChunkHeaderSize)
            throw new InvalidDataException($"Frame {frame}, chunk {index}: does not fit into the frame.");

        var size = reader.ReadUInt32();
        var type = (ChunkType)reader.ReadUInt16();
        if (size < ChunkHeaderSize || size > remaining)
            throw new InvalidDataException($"Frame {frame}, chunk {index} (0x{(ushort)type:X4}): bad size {size}.");

        Action<BinaryReader>? read = type switch
        {
            ChunkType.Layer => layers.Read,
            ChunkType.Palette => palette.Read,
            ChunkType.OldPalette => chunk => palette.ReadOld(chunk, 255),
            ChunkType.OldPalette64 => chunk => palette.ReadOld(chunk, 63),
            _ => null,
        };
        if (read is null)
        {
            Skip(reader, size - ChunkHeaderSize);
            return size;
        }

        // Each chunk gets its own reader, so it can't read past its end.
        var data = reader.ReadExactly(size - ChunkHeaderSize);
        try
        {
            using var chunk = new BinaryReader(new MemoryStream(data));
            read(chunk);
        }
        catch (Exception e) when (e is InvalidDataException or EndOfStreamException)
        {
            var reason = e is EndOfStreamException ? "Unexpected end of chunk." : e.Message;
            throw new InvalidDataException($"Frame {frame}, chunk {index} (0x{(ushort)type:X4}): {reason}", e);
        }

        return size;
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
