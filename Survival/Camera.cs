using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survival
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Target;
        private float _fieldOfView;

        public Camera()
        {
            Position = new Vector3(0, 0, 5);
            Rotation = Vector3.Zero;
            _fieldOfView = 75f;
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            const float movementSpeed = 2f;
            const float rotationSpeed = 2f;

            var velocity = Vector3.Zero;
            var rotation = Vector3.Zero;

            if (keyboardState.IsKeyUp(Keys.LeftShift))
            {
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    velocity.Z -= 1f;
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    velocity.X += 1f;
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    velocity.Z += 1f;
                }
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    velocity.X -= 1f;
                }
                if (keyboardState.IsKeyDown(Keys.PageUp))
                {
                    velocity.Y += 1f;
                }
                if (keyboardState.IsKeyDown(Keys.PageDown))
                {
                    velocity.Y -= 1f;
                }
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    rotation.X += 1f;
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    rotation.X -= 1f;
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    rotation.Y -= 1f;
                }
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    rotation.Y += 1f;
                }
            }

            if (rotation.Length() != 0)
            {
                rotation.Normalize();
                rotation *= rotationSpeed;
                rotation *= deltaTime;

                Rotation += rotation;
            }

            if (velocity.Length() != 0)
            {
                velocity.Normalize();
                velocity *= movementSpeed;
                velocity *= deltaTime;

                var rotationMatrix = Matrix.CreateRotationY(Rotation.Y);
                Position += Vector3.Transform(velocity, rotationMatrix);
            }
        }

        public Matrix GetViewMatrix()
        {
            var cameraRotationMatrix = Matrix.CreateRotationX(Rotation.X) *
                                       Matrix.CreateRotationY(Rotation.Y) *
                                       Matrix.CreateRotationZ(Rotation.Z);
            var cameraDirection = Vector3.Transform(Vector3.Forward, cameraRotationMatrix);

            return Matrix.CreateLookAt(
                Position,
                Target,
                Vector3.Up
            );
        }

        public Matrix GetProjectionMatrix(GraphicsDevice graphicsDevice)
        {
            return Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(_fieldOfView),
                graphicsDevice.Viewport.AspectRatio,
                0.1f,
                100f
            );
        }
    }
}
