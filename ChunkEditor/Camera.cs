using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChunkEditor
{
    public class Camera
    {
        private GraphicsDevice GraphicsDevice;
        private Vector3 _position;
        private Vector3 _rotation;
        
        public float Zoom { get; private set; }
        private float _orthographicWidth = 32f;  // Orthographic width
        private float _orthographicHeight = 32f; // Orthographic height
        private const float NearClipPlane = 0.1f;
        private const float FarClipPlane = 1000f;
        private const float MinZoom = 1.0f;
        private const float MaxZoom = 10.0f;

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public Matrix TransformationMatrix { get; private set; }
        
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdateTransformationMatrix();
            }
        }
        public Vector3 Rotation;

        public Camera(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            _position = new Vector3(0, 32, 0);  // Set initial position above the chunk
            _rotation = new Vector3(-MathHelper.PiOver2, 0, 0); // Look straight down
            Zoom = 1.0f;
            GenerateProjectionMatrix();
            UpdateTransformationMatrix();
        }

        private void GenerateProjectionMatrix()
        {
            Projection = Matrix.CreateOrthographic(_orthographicWidth * Zoom, _orthographicHeight * Zoom, NearClipPlane, FarClipPlane);
        }

        public void Move(Vector3 translation)
        {
            _position += translation;
            UpdateTransformationMatrix();
        }

        public void Teleport(Vector3 newPosition)
        {
            _position = newPosition;
            UpdateTransformationMatrix();
        }

        public void Rotate(Vector3 newRotation)
        {
            _rotation = newRotation;
            UpdateTransformationMatrix();
        }
        
        public void ZoomIn(float value = 1.0f)
        {
            value = Math.Abs(value);
            Zoom = Math.Clamp(Zoom + value, MinZoom, MaxZoom);
            GenerateProjectionMatrix();
            UpdateTransformationMatrix();
        }

        public void ZoomOut(float value = 1.0f)
        {
            value = Math.Abs(value);
            Zoom = Math.Clamp(Zoom - value, MinZoom, MaxZoom);
            GenerateProjectionMatrix();
            UpdateTransformationMatrix();
        }

        private void UpdateTransformationMatrix()
        {
            var viewportCenter = GraphicsDevice.Viewport.Bounds.Center.ToVector2();
            var translation = viewportCenter / Zoom - new Vector2(_position.X, _position.Y);
            
            TransformationMatrix = Matrix.CreateTranslation(new Vector3(translation, 0)) * Matrix.CreateScale(Zoom);
        }

        public void Update()
        {
            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotationMatrix);
            Vector3 up = Vector3.Transform(Vector3.Up, rotationMatrix);
            View = Matrix.CreateLookAt(_position, _position + forward, up);
        }

        public void SetOrthographicSize(float width, float height)
        {
            _orthographicWidth = width;
            _orthographicHeight = height;
            GenerateProjectionMatrix();
            UpdateTransformationMatrix();
        }
        
        public void SetOrthographicSize(Vector2 size)
        {
            SetOrthographicSize(size.X, size.Y);
        }
    }
}
