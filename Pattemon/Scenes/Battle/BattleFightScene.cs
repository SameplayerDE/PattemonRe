using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pattemon.Engine;

namespace Pattemon.Scenes.Battle;

public class BattleFightScene : SceneA
{
    private const int _stateFadeIn = 10;
    private const int _stateWaitFadeIn = 20;
    private const int _stateProcess = 30;
    private const int _stateFadeOut = 40;
    private const int _stateWaitFadeOut = 50;
    
    private int _state = 0;
    
    public BattleFightScene(Game game, object args = null, string contentDirectory = "Content") : base(game, args, contentDirectory)
    {
    }

    public override bool Init()
    {
        _state = _stateFadeIn;
        return true;
    }

    public override bool Exit()
    {
        throw new System.NotImplementedException();
    }

    public override bool Update(GameTime gameTime, float delta)
    {
        throw new System.NotImplementedException();
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, float delta)
    {
        throw new System.NotImplementedException();
    }
}