using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BattleGround;

public class Pokemon
{
    private Palette _normal;
    private Palette _shiny;
        
    private Texture2D _front;
    private Texture2D _back;

    public bool IsShiny = false;

    public static Pokemon FromName(GraphicsDevice graphicsDevice, string name)
    {
        Pokemon pokemon = new Pokemon();
        
        pokemon._normal = new Palette($"Assets/Pokemon/{name}/normal.pal");
        pokemon._shiny = new Palette($"Assets/Pokemon/{name}/shiny.pal");
        
        pokemon._back = Texture2D.FromFile(graphicsDevice, $"Assets/Pokemon/{name}/male_back.png");
        pokemon._front = Texture2D.FromFile(graphicsDevice, $"Assets/Pokemon/{name}/male_front.png");

        return pokemon;
    }

    public void Draw(SpriteBatch spriteBatch, int direction, Vector2 position, Rectangle source, Effect effect)
    {
        effect.Parameters["NormalPalette"].SetValue(_normal.Colors.Select(c => c.ToVector4()).ToArray());
        effect.Parameters["SwapPalette"].SetValue(_shiny.Colors.Select(c => c.ToVector4()).ToArray());
        effect.Parameters["UseSwapPalette"].SetValue(IsShiny);

        spriteBatch.Begin(effect: effect);
        spriteBatch.Draw(direction == 1 ? _back : _front, position.ToPoint().ToVector2(), source, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        spriteBatch.End();
    }
}