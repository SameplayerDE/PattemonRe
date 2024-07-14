using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace PatteLib.Graphics;

public class ImageFont
{
    private Dictionary<char, (int x, int y, int w, int h)> _characters = [];
    public Texture2D Texture { get; private set; }
    public int CharCount => _characters.Count;
    public int SpaceWidth = 4;
    public int TabWidth = 8;
    public int LineHeight = 19;

    public ImageFont(Texture2D texture)
    {
        Texture = texture;
    }

    public Point MeasureString(string value, int scale = 1)
    {
        int width = 0;
        int currentLineWidth = 0;
        int maxHeight = 0;

        foreach (char character in value)
        {
            if (character == '\n')
            {
                width = Math.Max(width, currentLineWidth);
                currentLineWidth = 0;
                maxHeight += LineHeight;
                continue;
            }

            if (_characters.TryGetValue(character, out var charInfo))
            {
                currentLineWidth += charInfo.w;
                width = Math.Max(width, currentLineWidth);

                maxHeight = Math.Max(maxHeight, charInfo.h);
            }
            else if (character == ' ')
            {
                currentLineWidth += SpaceWidth;
            }
            else if (character == '\t')
            {
                currentLineWidth += TabWidth;
            }
        }

        width = Math.Max(width, currentLineWidth);

        return new Point(width * scale, maxHeight * scale);
    }
    
    public bool HasChar(char @char)
    {
        return _characters.TryGetValue(@char, out var targetCharTuple);
    }
    
    public (int x, int y, int w, int h)? GetChar(char @char)
    {
        if (_characters.TryGetValue(@char, out var targetCharTuple))
        {
            return targetCharTuple;
        }
        return null;
    }

    public static ImageFont Load(GraphicsDevice graphicsDevice, string path)
    {
        var imageFontJson = File.ReadAllText(path);
        var jFont = JToken.Parse(imageFontJson);

        var jSourceToken = jFont["source"];
        if (jSourceToken == null)
        {
            //Todo: add error message
            throw new Exception("");
        }
        var source = jSourceToken.ToString();
        var combinedPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, source);

        if (!File.Exists(combinedPath))
        {
            //Todo: add error message
            throw new Exception("");
        }
        
        using var stream = File.OpenRead(combinedPath);
        var image = Texture2D.FromStream(graphicsDevice, stream);
        
        var jCharArrayToken = jFont["chars"];
        if (jCharArrayToken == null)
        {
            //Todo: add error message
            throw new Exception("");
        }

        var result = new ImageFont(image);
        
        foreach (var jCharArrayElementToken in jCharArrayToken)
        {
            var charToken = jCharArrayElementToken["char"];
            if (charToken == null)
            {
                //Todo: add error message
                throw new Exception("");
            }
            var @char = charToken.ToObject<char>();
            
            var xToken = jCharArrayElementToken["x"];
            if (xToken == null)
            {
                //Todo: add error message
                throw new Exception("");
            }
            var x = xToken.Value<int>();

            var yToken = jCharArrayElementToken["y"];
            if (yToken == null)
            {
                //Todo: add error message
                throw new Exception("");
            }
            var y = yToken.Value<int>();
            
            var wToken = jCharArrayElementToken["w"];
            if (wToken == null)
            {
                //Todo: add error message
                throw new Exception("");
            }
            var w = wToken.Value<int>();
            
            var hToken = jCharArrayElementToken["h"];
            if (hToken == null)
            {
                //Todo: add error message
                throw new Exception("");
            }
            var h = hToken.Value<int>();
            
            result._characters.Add(@char, (x, y, w, h));
            
        }

        return result;
    }
}