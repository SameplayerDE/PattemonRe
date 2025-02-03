using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;

namespace PatteLib.Graphics;

public class ImageFontMeta
{
    public required string Key;
    public required string SourcePath;
    public int GlyphHeight = 0;
    public int GlyphWidth = 0;
    public int GlyphSpacing = 0;
    public int Padding = 0;
    public int Space = 0;
}

public struct ImageFontGlyph
{
    public char Character;
    public int X, Y, Width, Height;
}

public class ImageFont
{
    public ImageFontMeta Meta { get; protected set; }
    private List<ImageFontGlyph> _glyphs = [];
    public Texture2D Texture { get; private set; }
    public int GlyphCount => _glyphs.Count;

    public static ImageFont LoadFromFile(GraphicsDevice graphicsDevice, string path)
    {
        var result = new ImageFont();
        
        var imageFontJson = File.ReadAllText(path);
        var jFont = JToken.Parse(imageFontJson);
        
        var jMeta = jFont["meta"];
        if (jMeta == null)
        {
            throw new Exception("");
        }
        
        var jMetaKey = jMeta["key"];
        if (jMetaKey == null)
        {
            throw new Exception("");
        }
        
        var jMetaSource = jMeta["source"];
        if (jMetaSource == null)
        {
            throw new Exception("");
        }
        
        var key = jMetaKey.ToString();
        var sourcePath = jMetaSource.ToString();

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(sourcePath))
        {
            throw new Exception("");
        }

        result.Meta = new ImageFontMeta
        {
            Key = key,
            SourcePath = sourcePath,
            Padding = jMeta["padding"]?.Value<int>() ?? 0,
            Space = jMeta["space"]?.Value<int>() ?? 0xffff,
            GlyphHeight = jMeta["height"]?.Value<int>() ?? 0
        };
        
        var jGlyphs = jFont["glyphs"];
        if (jGlyphs == null)
        {
            throw new Exception("");
        }

        for (var i = 0; i < jGlyphs.Count(); i++)
        {
            var jGlyph = jGlyphs[i];
            if (jGlyph == null)
            {
                throw new Exception();
            }

            var glyph = new ImageFontGlyph();

            var jGlyphCharacter = jGlyph["char"];
            var jGlyphX = jGlyph["x"];
            var jGlyphY = jGlyph["y"];
            var jGlyphW = jGlyph["w"];
            var jGlyphH = jGlyph["h"];

            if (jGlyphCharacter == null)
            {
                throw new Exception("");
            }

            glyph.Character = jGlyphCharacter.ToObject<char>();
            glyph.X = jGlyphX?.Value<int>() * 16 ?? 0;
            glyph.Y = jGlyphY?.Value<int>() * 16 ?? 0;
            glyph.Width = jGlyphW?.Value<int>() ?? result.Meta.GlyphWidth;
            glyph.Height = jGlyphH?.Value<int>() ?? result.Meta.GlyphHeight;

            result._glyphs.Add(glyph);
        }
        
        var combinedPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, sourcePath);
        if (!File.Exists(combinedPath))
        {
            //Todo: add error message
            throw new Exception("");
        }
        
        using var stream = File.OpenRead(combinedPath);
        var image = Texture2D.FromStream(graphicsDevice, stream);

        result.Texture = image;
        
        return result;
    }
    
    public Point MeasureString(string value, int scale = 1)
    {
        int maxWidth = 0;
        int currentLineWidth = 0;
        int totalHeight = Meta.GlyphHeight;

        foreach (char character in value)
        {
            if (character == '\n')
            {
                maxWidth = Math.Max(maxWidth, currentLineWidth);
                currentLineWidth = 0;
                totalHeight += Meta.GlyphHeight;
                continue;
            }

            if (character == ' ')
            {
                //currentLineWidth += Meta.Space;
            }
            else if (character == '\t')
            {
                //currentLineWidth += Meta.Space * 4;
            }
            else
            {
                var glyph = _glyphs.FirstOrDefault(g => g.Character == character);
                if (glyph.Character == character) // Falls Glyph existiert
                {
                    currentLineWidth += glyph.Width + Meta.GlyphSpacing;
                }
            }

            maxWidth = Math.Max(maxWidth, currentLineWidth);
        }

        return new Point(maxWidth * scale, totalHeight * scale);
    }

    
    public bool HasChar(char @char)
    {
        return _glyphs.Any(g => g.Character == @char);
    }
    
    public ImageFontGlyph? GetChar(char @char)
    {
        var glyph = _glyphs.FirstOrDefault(g => g.Character == @char);
        if (glyph.Character == @char)
        {
            return glyph;
        }
        return null;
    }

    
}