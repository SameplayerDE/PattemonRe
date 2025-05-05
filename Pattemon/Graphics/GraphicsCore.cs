using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pattemon.Graphics;

public static class GraphicsCore
{
    private const string PokemonPath = @"A:\Coding\Survival\TestRender\Content\Pokemon";
    private static Dictionary<string, Texture2D> _textures;
    private static GraphicsDevice _graphics;

    public static void Init(GraphicsDevice graphicsDevice)
    {
        _graphics = graphicsDevice;
        _textures = new Dictionary<string, Texture2D>();
    }

    public static void Load(ContentManager contentManager)
    {
    }
    
    public static async Task<Texture2D> LoadTextureAsync(string key, string path)
    {
        return await Task.Run(() =>
        {
            var texture = Texture2D.FromFile(_graphics, path);
            lock (_textures)
            {
                _textures[key] = texture;
            }
            return texture;
        });
    }

    public static Texture2D LoadTexture(string key, string path)
    {
        _textures[key] = Texture2D.FromFile(_graphics, path);
        return _textures[key];
    }
    
    public static Texture2D GetTexture(string key)
    {
        if (_textures.TryGetValue(key, out Texture2D value))
        {
            return value;
        }
        throw new KeyNotFoundException();
    }
    
    public static void FreeTexture(string key)
    {
        if (!_textures.TryGetValue(key, out var texture))
        {
            return;
        }
        texture.Dispose();
        _textures.Remove(key);
    }
}