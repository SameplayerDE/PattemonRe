using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Survival.Rendering;

namespace Survival
{
    public interface IShaderEffect
    {
        void LoadContent(ContentManager content);
        void SetMatrices(Matrix world, Matrix view, Matrix projection);
        void SetTime(float deltaTime, float totalTime);
        void ApplyEffect(GraphicsDevice graphicsDevice, VertexPositionColorNormalTexture[] vertices);
        void ApplyEffect(GraphicsDevice graphicsDevice, VertexPositionColorNormalTexture[] vertices, int[] indices);
    }
}
