using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Survival
{
    public class BasicShaderEffect : IShaderEffect
    {
        private Effect _effect;
        private Texture2D _texture;

        public void LoadContent(ContentManager content)
        {
            _effect = content.Load<Effect>("shader");
            _effect.Parameters["Texture"]?.SetValue(_texture);
        }

        public void SetMatrices(Matrix world, Matrix view, Matrix projection)
        {
            _effect.Parameters["World"]?.SetValue(world);
            _effect.Parameters["ViewProjection"]?.SetValue(view * projection);
        }

        public void SetTime(float deltaTime, float totalTime)
        {
            _effect.Parameters["Delta"]?.SetValue(deltaTime);
            _effect.Parameters["Total"]?.SetValue(totalTime);
        }

        public void ApplyEffect(GraphicsDevice graphicsDevice, VertexPositionColorNormalTexture[] vertices)
        {
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
            }
        }

        public void ApplyEffect(GraphicsDevice graphicsDevice, VertexPositionColorNormalTexture[] vertices, int[] indices)
        {
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
            }
        }
    }
}
