using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PatteLib.Graphics;

public class SpriteCollection : IDisposable
{
    
    public int Id;
    public ContentManager ContentManager;
    public Dictionary<int, Texture2D> Sprites = [];
    
    protected string _basePath;
    protected string _fileName;
    protected int _count = 0;
    
    public SpriteCollection(IServiceProvider serviceProvider)
    {
        ContentManager = new ContentManager(serviceProvider);
    }

    public void Load(string path, string fileName, int count)
    {
        _basePath = path;
        _fileName = fileName;
        _count = count;
        for (var i = 0; i < count; i++)
        {
            var texturePath = $@"{_basePath}\{_fileName}_{i + 1}";
            Sprites.Add(i, ContentManager.Load<Texture2D>(texturePath));
        }
    }

    public bool Has(int index)
    {
        return Sprites.ContainsKey(index);
    }
    
    public Texture2D Get(int index)
    {
        return Sprites[index];
    }
    
    public void Dispose()
    {
        ContentManager.Dispose();
        GC.SuppressFinalize(this);
    }
}