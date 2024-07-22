using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PatteLib;

namespace TestRendering
{
    public class Camera2
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
        protected float _nearClipPlane = 0.01f;
        protected float _farClipPlane = 5120f;

        public Matrix View { get { return _view; } }
        public Matrix Projection { get { return _projection; } }

        public Vector3 Direction { get { return _direction; } set { _direction = value; } }
        public Vector3 Up { get { return _up; } }
        public Vector3 Right { get { return _right; } }
        public Vector3 Forward { get { return _forward; } }

        public Matrix RotationMXYZ { get { return Matrix.Multiply(Matrix.Multiply(Matrix.CreateRotationX(_rotation.X), Matrix.CreateRotationY(_rotation.Y)), Matrix.CreateRotationZ(_rotation.Z)); } }
        public Matrix RotationMXY { get { return Matrix.Multiply(Matrix.CreateRotationX(_rotation.X), Matrix.CreateRotationY(_rotation.Y)); } }
        public Matrix RotationMXZ { get { return Matrix.Multiply(Matrix.CreateRotationX(_rotation.X), Matrix.CreateRotationZ(_rotation.Z)); } }
        public Matrix RotationMYZ { get { return Matrix.Multiply(Matrix.CreateRotationY(_rotation.Y), Matrix.CreateRotationZ(_rotation.Z)); } }
        public Matrix RotationMX { get { return Matrix.CreateRotationX(_rotation.X); } }
        public Matrix RotationMInvX { get { return Matrix.CreateRotationX(-_rotation.X); } }
        public Matrix RotationMY { get { return Matrix.CreateRotationY(_rotation.Y); } }
        public Matrix RotationMZ { get { return Matrix.CreateRotationZ(_rotation.Z); } }

        public float RotationX { get { return _rotation.X; } set { _rotation.X = value; } }
        public float RotationY { get { return _rotation.Y; } set { _rotation.Y = value; } }
        public float RotationZ { get { return _rotation.Z; } set { _rotation.Z = value; } }
        public Vector3 Position { get { return _position; } }
        public Vector3 Rotation { get { return _rotation; } }

        public bool EnableMix = false;
        public float OrthoFactor = 0.00001f;
        
        public Camera2(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            //RotateX(MathHelper.ToDegrees((float)Utils.Q412ToDouble(-10750)));
            _position = new Vector3(0, 0, 0);
            GenerateProjectionMatrix();
        }

        protected void GenerateProjectionMatrix()
        {
            var presentationParameters = GraphicsDevice.PresentationParameters;
            var aspectRatio = (float)presentationParameters.BackBufferWidth / (float)presentationParameters.BackBufferHeight;
            
            var width = 32 * 42f;
            var height = width / aspectRatio;
            //_projection = Matrix.CreateOrthographic(width, height, _nearClipPlane, _farClipPlane);
            
            var perspective = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70f), aspectRatio, _nearClipPlane, _farClipPlane);
            _projection = perspective;
        }

        
        public void Move(float x, float y, float z)
        {
            Move(new Vector3(x, y, z));
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

        public void Look(Vector3 rotation)
        {
            _rotation = rotation;
        }

        public void RotateBy(Vector3 rotation)
        {
            _rotation += rotation;
        }
        
        public void RotateTo(Vector3 rotation)
        {
            _rotation = rotation;
        }

        public void RotateX(float x)
        {
            _rotation.X += MathHelper.ToRadians(x);
            RotationX = Math.Clamp(RotationX, MathHelper.ToRadians(-89.9f), MathHelper.ToRadians(89.9f));
        }

        public void RotateZ(float z)
        {
            _rotation.Z += MathHelper.ToRadians(z);
        }

        public void RotateY(float y)
        {
            _rotation.Y += MathHelper.ToRadians(y);
            if (RotationY < 0) RotationY += MathHelper.ToRadians(360);
        }

        private float accumulatedTime = 0.0f;
        public virtual void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _translation = Vector3.Transform(_translation, RotationMY);
            _position += _translation;
            _translation = Vector3.Zero;

            Vector3 forward = Vector3.Transform(Forward, RotationMXY);
            _direction = _position + forward;

            _view = Matrix.CreateLookAt(_position, _direction, _up);
            

            GenerateProjectionMatrix();
        }
        
    }
}