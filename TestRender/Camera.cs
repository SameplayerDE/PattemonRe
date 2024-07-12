using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestRendering
{
    public class Camera
    {
        protected GraphicsDevice GraphicsDevice { get; set; }

        protected Matrix _view;
        protected Matrix _projection;

        protected Vector3 _up = Vector3.Up;
        protected Vector3 _right = Vector3.Right;
        protected Vector3 _forward = Vector3.Backward;
        protected Vector3 _direction = Vector3.Zero;
        protected Vector3 _translation = Vector3.Zero;
        protected Vector3 _position = Vector3.Zero;
        protected Vector3 _rotation = Vector3.Zero;

        protected float _fov = MathHelper.ToRadians(45);
        protected float _aspectRatio = 1.6f;
        protected float _nearClipPlane = 1f;
        protected float _farClipPlane = 5120f;

        public Matrix View { get { return _view; } }
        public Matrix Projection { get { return _projection; } }

        public Vector3 Direction { get { return _direction; } set { _direction = value; } }
        public Vector3 Up { get { return _up; } }
        public Vector3 Right { get { return _right; } }
        public Vector3 Forward { get { return _forward; } }

        public Matrix RotationMXYZ { get { return Matrix.CreateRotationX(_rotation.X) * Matrix.CreateRotationY(_rotation.Y) * Matrix.CreateRotationZ(_rotation.Z); } }
        public float RotationX { get { return _rotation.X; } set { _rotation.X = value; } }
        public float RotationY { get { return _rotation.Y; } set { _rotation.Y = value; } }
        public float RotationZ { get { return _rotation.Z; } set { _rotation.Z = value; } }
        public Matrix RotationMInvX { get { return Matrix.CreateRotationX(-_rotation.X); } }
        public Vector3 Position { get { return _position; } }
        public Vector3 Rotation { get { return _rotation; } }

        public Camera(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            _position = new Vector3(0, 0, 0);
            GenerateProjectionMatrix();
        }

        protected void GenerateProjectionMatrix()
        {
            var presentationParameters = GraphicsDevice.PresentationParameters;
            var aspectRatio = (float)presentationParameters.BackBufferWidth / (float)presentationParameters.BackBufferHeight;

            _projection = Matrix.CreatePerspectiveFieldOfView(_fov, aspectRatio, _nearClipPlane, _farClipPlane);
        }

        public void Move(Vector3 translation)
        {
            _translation += translation;
        }

        public void Teleport(Vector3 position)
        {
            _position = position;
        }
        
        public void Teleport(float x, float y, float z)
        {
            Teleport(new Vector3(x, y, z));
        }

        public void RotateBy(Vector3 rotation)
        {
            _rotation += rotation;
        }

        public void RotateTo(Vector3 rotation)
        {
            _rotation = rotation;
        }

        public void LookAt(Vector3 target)
        {
            _direction = target;
            _view = Matrix.CreateLookAt(_position, _direction, _up);
        }

        public virtual void Update(GameTime gameTime)
        {
            _translation = Vector3.Transform(_translation, Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z));
            _position += _translation;
            _translation = Vector3.Zero;

            _view = Matrix.CreateLookAt(_position, _position + Vector3.Transform(_forward, Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z)), _up);
        }
    }
}
