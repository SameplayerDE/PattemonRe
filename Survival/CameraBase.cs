using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survival
{
    public abstract class CameraBase : GameObject
    {
        protected float _fieldOfView;
        protected Matrix _viewMatrix;
        protected Matrix _projectionMatrix;
        public GraphicsDevice GraphicsDevice;

        public CameraBase(GraphicsDevice device)
        {
            _fieldOfView = 75f;
            _viewMatrix = Matrix.Identity;
            _projectionMatrix = Matrix.Identity;
            GraphicsDevice = device;
        }

        public abstract Matrix GetViewMatrix();
        public Matrix GetProjectionMatrix(GraphicsDevice graphicsDevice)
        {
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(_fieldOfView),
                graphicsDevice.Viewport.AspectRatio,
                0.1f,
                100f
            );
            return _projectionMatrix;
        }
    }
}
