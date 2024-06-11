using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Survival.HxPly;
using Survival.Rendering;
using System;
using System.Collections.Generic;

namespace Survival
{
    public class CameraController
    {
        private readonly TargetCamera _camera;
        private Vector3 _targetPosition;
        private float _yaw;
        private float _pitch;

        private MouseState _previousMouseState;
        private float _zoomSpeed = 0.1f;  // Geschwindigkeit des Zoomens
        private float _minDistance = 0.1f; // Minimale Distanz zur Zielposition
        private float _maxDistance = 20f; // Maximale Distanz zur Zielposition

        public CameraController(TargetCamera camera, Vector3 initialTargetPosition)
        {
            _camera = camera;
            _targetPosition = initialTargetPosition;
            _yaw = 0f;
            _pitch = 0f;

            // Set the initial mouse state
            _previousMouseState = Mouse.GetState();
            CenterMouse();
        }

        private void CenterMouse()
        {
            var centerX = _camera.GraphicsDevice.Viewport.Width / 2;
            var centerY = _camera.GraphicsDevice.Viewport.Height / 2;
            Mouse.SetPosition(centerX, centerY);
            _previousMouseState = Mouse.GetState();
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var currentMouseState = Mouse.GetState();

            // Calculate mouse movement
            var deltaX = currentMouseState.X - _previousMouseState.X;
            var deltaY = currentMouseState.Y - _previousMouseState.Y;

            // Update yaw and pitch based on mouse movement
            const float rotationSpeed = 0.1f;
            _yaw -= deltaX * rotationSpeed * deltaTime;
            _pitch += deltaY * rotationSpeed * deltaTime;

            // Clamp pitch to prevent flipping
            _pitch = MathHelper.Clamp(_pitch, -MathHelper.PiOver2, MathHelper.PiOver2);

            // Create rotation quaternion from yaw and pitch
            _camera.Rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);

            // Update camera target
            _camera.Target = _targetPosition;

            // Zoom with mouse wheel
            int scrollDelta = currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
            if (scrollDelta != 0)
            {
                float zoomAmount = scrollDelta * _zoomSpeed * deltaTime;
                _camera.Distance = MathHelper.Clamp(_camera.Distance - zoomAmount, _minDistance, _maxDistance);
            }

            // Update camera view matrix
            _camera.GetViewMatrix();

            // Center the mouse
            CenterMouse();
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private RenderTarget2D _target;

        private List<SceneObject> _sceneObjects;
        private ThirdPersonCamera _camera2;

        private Vector3 _position = new Vector3(0, 1.0f, 0);
        private Vector3 _rotation;

        private SceneObject _bushBody;
        private SceneObject _bushHead;
        private SceneObject _bushHair;

        private MouseState _prevMouseState;
        private MouseState _currMouseState;

        private TargetCamera _camera;
        private CameraController _cameraController;

        private Model _fox;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            Content.RootDirectory = "Content";

            _graphics.ApplyChanges();
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            _camera = new TargetCamera(GraphicsDevice, _position, 10f);
            _cameraController = new CameraController(_camera, _position);

            _target = new RenderTarget2D(
                GraphicsDevice,
                _graphics.PreferredBackBufferWidth / 1,
                _graphics.PreferredBackBufferHeight / 1,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24
            );

           // _camera = new ThirdPersonCamera(Vector3.Zero, 5f, MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
            _sceneObjects = new List<SceneObject>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var gameModel = GameModel.LoadFrom(GraphicsDevice, @"Content\Fox.glb");

            _fox = Content.Load<Model>("model");

            var normalEffect = Content.Load<Effect>("NormalMap");
            var cellEffect = Content.Load<Effect>("cellOnlyShader");

            _bushBody = new SceneObject(PlyLoader.Load(@"G:\Modelle\bush_body.ply"), cellEffect, Content.Load<Texture2D>("bush_body"));
            _bushHead = new SceneObject(PlyLoader.Load(@"G:\Modelle\bush_head.ply"), cellEffect, Content.Load<Texture2D>("bush_head"));
            _bushHair = new SceneObject(PlyLoader.Load(@"G:\Modelle\bush_hair.ply"), cellEffect, Content.Load<Texture2D>("bush_hair"));

            _sceneObjects.Add(_bushBody);
            _sceneObjects.Add(_bushHead);
            _sceneObjects.Add(_bushHair);

            MovePlayer(new Vector3(2, 0, 0));

            var chunBodyNormalMap = Content.Load<Texture2D>("chun_body_nmap");
            var chunHairNormalMap = Content.Load<Texture2D>("hair_n");
            var chunHeadNormalMap = Content.Load<Texture2D>("head_n");

            var chunArms = new SceneObject(PlyLoader.Load(@"G:\Modelle\chun_arms.ply"), normalEffect, Content.Load<Texture2D>("chun_body"), chunBodyNormalMap);
            var chunBody = new SceneObject(PlyLoader.Load(@"G:\Modelle\chun_body.ply"), normalEffect, Content.Load<Texture2D>("chun_body"), chunBodyNormalMap);
            var chunShoes = new SceneObject(PlyLoader.Load(@"G:\Modelle\chun_shoes.ply"), normalEffect, Content.Load<Texture2D>("chun_body_s"), chunBodyNormalMap);
            var chunHead = new SceneObject(PlyLoader.Load(@"G:\Modelle\chun_head.ply"), normalEffect, Content.Load<Texture2D>("chun_head"), chunHeadNormalMap);
            var chunHair = new SceneObject(PlyLoader.Load(@"G:\Modelle\chun_hair.ply"), normalEffect, Content.Load<Texture2D>("chun_hair"), chunHairNormalMap);

            _sceneObjects.Add(chunArms);
            _sceneObjects.Add(chunBody);
            _sceneObjects.Add(chunShoes);
            _sceneObjects.Add(chunHead);
            _sceneObjects.Add(chunHair);

            var random = new Random();
            for (int i = 0; i < 100; i++)
            {
                var tree = new SceneObject(PlyLoader.Load(@"G:\Modelle\tree.ply"), normalEffect);
                tree.Position.X = random.Next(-10, 10);
                tree.Position.Z = random.Next(-10, 10);
                _sceneObjects.Add(tree);
            }

        }

        private void MovePlayer(Vector3 position)
        {
            _bushBody.Position = position;
            _bushHead.Position = position;
            _bushHair.Position = position;
        }

        private void RotatePlayer(Vector3 rotation)
        {
            _bushBody.Rotation = rotation;
            _bushHead.Rotation = rotation;
            _bushHair.Rotation = rotation;
        }

        protected override void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            _prevMouseState = _currMouseState;
            _currMouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            _cameraController.Update(gameTime);


            /*var mouseDelta = _prevMouseState.Position - _currMouseState.Position;
            var mouseWheelDelta = _prevMouseState.ScrollWheelValue - _currMouseState.ScrollWheelValue;

            Vector3 moveDirection = Vector3.Zero;
            if (keyboardState.IsKeyDown(Keys.W))
            {
                moveDirection += Vector3.Forward;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                moveDirection += Vector3.Backward;
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                moveDirection += Vector3.Left;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                moveDirection += Vector3.Right;
            }

            if (moveDirection != Vector3.Zero)
            {
                moveDirection.Normalize();
                var rotationMatrix = Matrix.CreateRotationY(_camera.Rotation.Y);
                moveDirection = Vector3.Transform(moveDirection, rotationMatrix);
                _position += moveDirection * deltaTime * 5f; // Adjust the speed factor as needed
                _rotation.Y = (float)Math.Atan2(-moveDirection.X, -moveDirection.Z);
            }

            MovePlayer(_position);
            RotatePlayer(_rotation);

            if (_currMouseState.RightButton == ButtonState.Pressed)
            {
                _camera.Rotate(mouseDelta.X * deltaTime, mouseDelta.Y * deltaTime);
            }

            var delta = Math.Sign(mouseWheelDelta);
            _camera.Zoom(0.1f * delta);

            _camera.Update(gameTime, _position);*/

            foreach (var sceneObject in _sceneObjects)
            {
                sceneObject.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            var view = _camera.GetViewMatrix();
            var projection = _camera.GetProjectionMatrix(GraphicsDevice);

            foreach (var sceneObject in _sceneObjects)
            {
                sceneObject.Draw(GraphicsDevice, view, projection);
            }

            var transforms = new Matrix[_fox.Bones.Count];
            _fox.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (var mesh in _fox.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.View = _camera.GetViewMatrix();
                    effect.Projection = _camera.GetProjectionMatrix(GraphicsDevice);
                    effect.World = transforms[mesh.ParentBone.Index];
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }

    public class ThirdPersonCamera
    {
        public Vector3 Position { get; private set; }
        public Vector3 Target { get; private set; }
        public Vector3 Rotation { get; private set; }

        private float _distance;
        private float _fieldOfView;
        private float _aspectRatio;
        private float _nearPlane;
        private float _farPlane;

        public ThirdPersonCamera(Vector3 target, float distance, float fieldOfView, float aspectRatio, float nearPlane, float farPlane)
        {
            Target = target;
            _distance = distance;
            _fieldOfView = fieldOfView;
            _aspectRatio = aspectRatio;
            _nearPlane = nearPlane;
            _farPlane = farPlane;
            Rotation = Vector3.Zero;
        }

        public void Rotate(float yaw, float pitch)
        {
            Rotation = new Vector3(
                MathHelper.Clamp(Rotation.X + pitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f),
                Rotation.Y + yaw,
                0
            );
        }

        public void Zoom(float value)
        {
            _distance += value;
            _distance = MathHelper.Clamp(_distance, 0.5f, 5f);
        }

        public void Update(GameTime gameTime, Vector3 target)
        {
            target.Y += 1.5f;
            Target = target;

            var rotationMatrix = Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, 0);
            Position = Target - Vector3.Transform(Vector3.Forward * _distance, rotationMatrix);
        }

        public Matrix GetViewMatrix()
        {
            return Matrix.CreateLookAt(Position, Target, Vector3.Up);
        }

        public Matrix GetProjectionMatrix()
        {
            return Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearPlane, _farPlane);
        }
    }
}
