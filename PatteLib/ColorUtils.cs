using Microsoft.Xna.Framework;

namespace PatteLib;

public class ColorUtils
{
    public static Color FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Hex color code cannot be null or empty.", nameof(hex));

        hex = hex.Replace("#", "");

        if (hex.Length == 3) // z.B. "FA4"
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }
        else if (hex.Length == 4) // z.B. "FA48" (inkl. Alpha)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
        }
        
        if (hex.Length == 6)
        {
            return new Color(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16)
            );
        }
        else if (hex.Length == 8)
        {
            return new Color(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16),
                Convert.ToInt32(hex.Substring(6, 2), 16)
            );
        }
        else
        {
            throw new ArgumentException("Hex color code must be in the format #RRGGBB or #RRGGBBAA.", nameof(hex));
        }
    }
}