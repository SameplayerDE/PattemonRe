using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Survival.HxPly;
using System;

namespace Survival
{
    public class SceneObject
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        private Effect _effect;
        private Texture2D _texture;
        private Texture2D _normalMap;
        private PlyFile _mesh;

        private float _time; // Variable für Animation

        public SceneObject(PlyFile mesh, Effect effect, Texture2D texture = null, Texture2D normalMap = null)
        {
            _normalMap = normalMap;
            _texture = texture;
            _effect = effect;
            _mesh = mesh;
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
            _time = 0f;
            _normalMap = normalMap;

        }

        public void Move(float x, float y, float z)
        {
            Position.X += x;
            Position.Y += y;
            Position.Z += z;
        }

        public void Update(GameTime gameTime)
        {
            _time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            Rotation = new Vector3(Rotation.X, _time, Rotation.Z);
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
        {
            Matrix world = Matrix.CreateScale(Scale) *
                           Matrix.CreateRotationX(Rotation.X) *
                           Matrix.CreateRotationY(Rotation.Y) *
                           Matrix.CreateRotationZ(Rotation.Z) *
                           Matrix.CreateTranslation(Position);

            _effect.Parameters["World"]?.SetValue(world);
            _effect.Parameters["ViewProjection"]?.SetValue(view * projection);
            if (_texture != null)
            {
                _effect.Parameters["Texture"]?.SetValue(_texture);
            }
            _effect.Parameters["UseTexture"]?.SetValue(_texture == null ? false : true);
            if (_normalMap != null)
            {
                _effect.Parameters["NormalMap"]?.SetValue(_normalMap);
            }
            _effect.Parameters["UseNormalMap"]?.SetValue(_normalMap == null ? false : true);

            ApplyEffect(graphicsDevice, _mesh.VertexData.ToArray(), _mesh.IndexData[0]);
        }

        private void ApplyEffect(GraphicsDevice graphicsDevice, VertexPositionColorNormalTexture[] vertices, int[] indices)
        {

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
            }
        }
    }
}
