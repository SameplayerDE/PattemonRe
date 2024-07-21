using Microsoft.Xna.Framework;

namespace PatteLib.Graphics;

public class Palette
{
    public Color[] Colors;

    public Palette(string path)
    {
        var lines = File.ReadAllLines(path);

        if (lines.Length < 3 || lines[0] != "JASC-PAL" || lines[1] != "0100")
        {
            throw new InvalidDataException();
        }

        int colourCount = int.Parse(lines[2]);
        Colors = new Color[colourCount];

        for (int i = 0; i < colourCount; i++)
        {
            string[] parts = lines[3 + i].Split(' ');
            int r = int.Parse(parts[0]);
            int g = int.Parse(parts[1]);
            int b = int.Parse(parts[2]);
            Colors[i] = new Color((byte)r, (byte)g, (byte)b);
        }
    }
}