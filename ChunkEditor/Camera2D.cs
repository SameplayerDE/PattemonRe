using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChunkEditor;

public class Camera2D
{
    private readonly float _minZoom = 1.0f;
    private readonly float _maxZoom = 10.0f;
    public GraphicsDevice GraphicsDevice;
    public float Zoom;
    public float X;
    public float Y;
    public Matrix TransformationMatrix;
    
    public Camera2D(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
        X = 0;
        Y = 0;
        Zoom = 1;
    }
    
    public void ZoomIn(float value = 1.0f)
    {
        value = Math.Abs(value);
        Zoom = Math.Clamp(Zoom + value, _minZoom, _maxZoom);
        UpdateTransformationMatrix();
    }
    
    public void ZoomOut(float value = 1.0f)
    {
        value = Math.Abs(value);
        Zoom = Math.Clamp(Zoom - value, _minZoom, _maxZoom);
        UpdateTransformationMatrix();
    }
    
    public void ZoomAtAnchor(float delta, Vector2 anchorScreenPos)
    {
        Vector2 oldWorldPos = ScreenToWorld(anchorScreenPos, this);
        float newZoom = Math.Clamp(Zoom + delta, _minZoom, _maxZoom);
        Zoom = newZoom;
        UpdateTransformationMatrix();
        Vector2 newWorldPos = ScreenToWorld(anchorScreenPos, this);
        Vector2 diff = oldWorldPos - newWorldPos;
        X += diff.X;
        Y += diff.Y;
        UpdateTransformationMatrix();
    }
    
    public void Move(float deltaX, float deltaY)
    {
        X += deltaX;
        Y += deltaY;
        UpdateTransformationMatrix();
    }

    public void MoveTo(Vector2 targetPosition, float lerpAmount = 1.0f)
    {
        X = MathHelper.Lerp(X, targetPosition.X, lerpAmount);
        Y = MathHelper.Lerp(Y, targetPosition.Y, lerpAmount);
        UpdateTransformationMatrix();
    }

    private void UpdateTransformationMatrix()
    {
        var viewportCenter = GraphicsDevice.Viewport.Bounds.Center.ToVector2();
        TransformationMatrix = Matrix.CreateTranslation(-X, -Y, 0) * Matrix.CreateScale(Zoom) * Matrix.CreateTranslation(viewportCenter.X, viewportCenter.Y, 0);
    }
    
    public void Update(GameTime gameTime)
    {
        UpdateTransformationMatrix();
    }
    
    public static Vector2 WorldToScreen(Vector2 position, Camera2D camera)
    {
        var transformedPosition = Vector2.Transform(position, camera.TransformationMatrix);
        return transformedPosition;
    }

    public static Vector2 ScreenToWorld(Vector2 position, Camera2D camera)
    {
        var inverseMatrix = Matrix.Invert(camera.TransformationMatrix);
        var transformedPosition = Vector2.Transform(position, inverseMatrix);
        return transformedPosition;
    }
    
    public Camera2D Copy()
    {
        return new Camera2D(GraphicsDevice)
        {
            Zoom = this.Zoom,
            X = this.X,
            Y = this.Y
        };
    }
}
