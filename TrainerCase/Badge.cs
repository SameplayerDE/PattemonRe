using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TrainerCase;

public enum BadgeState
{
    Clean,
    Normal,
    Dust0,
    Dust1
}

public class Badge
{
    private BadgeState _state;
    private List<Texture2D> _textures;

    public static Badge CreateBadge(int index, ContentManager contentManager)
    {
        var badge = new Badge
        {
            _textures =
            [
                contentManager.Load<Texture2D>($"Badge_{index}_Clean"),
                contentManager.Load<Texture2D>($"Badge_{index}"),
                contentManager.Load<Texture2D>($"Badge_{index}_Dust_0"),
                contentManager.Load<Texture2D>($"Badge_{index}_Dust_1")
            ],
            _state = BadgeState.Normal
        };
        return badge;
    }
    
    public Texture2D GetTexture()
    {
        return _textures[(int)_state];
    }
}