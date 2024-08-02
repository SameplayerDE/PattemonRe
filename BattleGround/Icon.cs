using System;
using Microsoft.Xna.Framework.Graphics;

namespace BattleGround;

public class Icon : IDisposable
{
    private Texture2D _texture;

   //public static Icon FromFile(GraphicsDevice graphicsDevice, string path)
   //{
   //    Icon result = new Icon();
   //    result._texture = Texture2D.FromFile(graphicsDevice, path);
   //}
    
    public void Dispose()
    {
        _texture?.Dispose();
    }
}