using Microsoft.Xna.Framework.Graphics;

namespace PatteLib.Graphics;

public class SpriteCollection
{
    public Dictionary<string, Texture2D> Content = [];

    public void Add(string key, Texture2D value)
    {
        Content.Add(key, value);
    }
}