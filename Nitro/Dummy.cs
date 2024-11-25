using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Pattern
{
    public string MaterialName { get; set; }
    public List<string> TextureNames { get; set; }
    public List<string> PaletteNames { get; set; }
    public List<Keyframe> Keyframes { get; set; }

    public static Pattern ReadFromBinaryFile(string filePath)
    {
        using (var reader = new BinaryReader(File.OpenRead(filePath)))
        {
            long fileSize = reader.BaseStream.Length;

            // Signatur prüfen
            var fileSignature = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (fileSignature != "BTP0")
            {
                throw new InvalidDataException("Invalid file format: Missing 'BTP0' signature.");
            }

            // Zusätzliche Header-Felder überspringen
            reader.BaseStream.Seek(20, SeekOrigin.Begin); // Springe zu Offset 0x14 (PAT0)

            // PAT0-Block prüfen
            var blockSignature = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (blockSignature != "PAT0")
            {
                throw new InvalidDataException("Invalid block: Missing 'PAT0' identifier.");
            }

            // PAT0-Header lesen
            var sectionSize = reader.ReadUInt32(); // Größe des Blocks
            var numFrames = reader.ReadUInt16(); // Anzahl Frames
            var numTextureNames = reader.ReadByte(); // Anzahl Texturen
            var numPaletteNames = reader.ReadByte(); // Anzahl Paletten
            var textureNamesOffset = reader.ReadUInt16(); // Offset zu Texturen
            var paletteNamesOffset = reader.ReadUInt16(); // Offset zu Paletten
            var keyframeOffset = reader.ReadUInt16(); // Offset zu Keyframes

            // Validierung der Offsets
            if (textureNamesOffset >= fileSize) throw new InvalidDataException("Invalid texture names offset.");
            if (paletteNamesOffset >= fileSize) paletteNamesOffset = 0; // Keine Paletten vorhanden
            if (keyframeOffset >= fileSize) throw new InvalidDataException("Invalid keyframe offset.");

            // Materialnamen lesen
            reader.BaseStream.Seek(textureNamesOffset, SeekOrigin.Begin);
            var materialName = ReadNullTerminatedString(reader);

            // Textur-Namen lesen
            var textureNames = ReadInfoBlock(reader, numTextureNames, 6);

            // Paletten-Namen lesen (falls vorhanden)
            var paletteNames = new List<string>();
            if (paletteNamesOffset > 0)
            {
                reader.BaseStream.Seek(paletteNamesOffset, SeekOrigin.Begin);
                paletteNames = ReadInfoBlock(reader, numPaletteNames, 3);
            }

            // Keyframes lesen
            reader.BaseStream.Seek(keyframeOffset, SeekOrigin.Begin);
            var keyframes = new List<Keyframe>();
            while (reader.BaseStream.Position < fileSize)
            {
                var frame = reader.ReadUInt16();
                var textureIdx = reader.ReadByte();
                var paletteIdx = reader.ReadByte();
                keyframes.Add(new Keyframe
                {
                    Frame = frame,
                    TextureIndex = textureIdx,
                    PaletteIndex = paletteIdx
                });
            }

            // Ergebnis zurückgeben
            return new Pattern
            {
                MaterialName = materialName,
                TextureNames = textureNames,
                PaletteNames = paletteNames,
                Keyframes = keyframes
            };
        }
    }

    private static List<string> ReadInfoBlock(BinaryReader reader, int count, int padding)
    {
        var names = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var name = ReadNullTerminatedString(reader);
            names.Add(name);
            reader.BaseStream.Seek(padding, SeekOrigin.Current); // Überspringe Leerbytes
        }
        return names;
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        var bytes = new List<byte>();
        while (true)
        {
            byte b = reader.ReadByte();
            if (b == 0) break;
            bytes.Add(b);
        }
        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}

public class Keyframe
{
    public ushort Frame { get; set; } // Zeitpunkt des Frames
    public byte TextureIndex { get; set; } // Index in der Textur-Liste
    public byte PaletteIndex { get; set; } // Index in der Paletten-Liste
}
