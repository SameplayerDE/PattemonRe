using HxGLTF;
using HxGLTF.Implementation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TestRender
{
    public class Hero
    {
        private KeyboardState _prev;
        private KeyboardState _curr;
        
        public GameModel _model;
        public Vector3 Position = new Vector3(0, 16, 0);
        private const int TileSize = 32;

        public void LoadContent(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _model = GameModel.From(graphicsDevice, GLTFLoader.Load(@"Content\hilda_regular_00"));
            _model.Scale *= 40;
        }

        public void Update(GameTime gameTime)
        {

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _prev = _curr;
            _curr = Keyboard.GetState();

            if (_curr.IsKeyDown(Keys.W))
            {
                
                _model.Play(37);
                Position += Vector3.Forward * delta * TileSize;
                _model.RotateTo(Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(180)));
            }
            else if (_curr.IsKeyDown(Keys.S))
            {
                
                _model.Play(37);
                Position += Vector3.Backward * delta * TileSize;
                _model.RotateTo(Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(0)));
            }
            else if (_curr.IsKeyDown(Keys.D))
            {
                
                _model.Play(37);
                Position += Vector3.Right * delta * TileSize;
                _model.RotateTo(Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(90)));
            }
            else if (_curr.IsKeyDown(Keys.A))
            {
                
                _model.Play(37);
                Position += Vector3.Left * delta * TileSize;
                _model.RotateTo(Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(-90)));
            }
            else
            {
                _model.Play(28);
            }
            
            _model.Translation = Position;
            _model.Update(gameTime);
        }
    }
}