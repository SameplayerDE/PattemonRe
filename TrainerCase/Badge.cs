using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TrainerCase;

public enum BadgeState
{
    Clean1 = 0,
    Clean0 = 1,
    Normal = 2,
    Dust0 = 3,
    Dust1 = 4
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
                contentManager.Load<Texture2D>($"Badge_{index}_Clean"),
                contentManager.Load<Texture2D>($"Badge_{index}"),
                contentManager.Load<Texture2D>($"Badge_{index}_Dust_0"),
                contentManager.Load<Texture2D>($"Badge_{index}_Dust_1")
            ],
            _state = BadgeState.Normal
        };
        return badge;
    }

    public void CycleState()
    {
        if (_state == BadgeState.Dust1)
        {
            _state = BadgeState.Clean1;
        }
        else
        {
            _state = (BadgeState)((int)_state + 1);
        }
    }
    
    public BadgeState GetState()
    {
        return _state;
    }
    
    public Texture2D GetTexture()
    {
        return _textures[(int)_state];
    }
}