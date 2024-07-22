using Microsoft.Xna.Framework;

namespace Pattemon.Graphics;

public class Camera
{

    public Vector3 Position { get; set; }
    public Vector3 Target { get; set; }
    public Vector3 Up { get; set; }

    public float FieldOfViewY { get; set; }
    public float AspectRatio { get; set; }
    public float NearClip { get; set; }
    public float FarClip { get; set; }

    public Matrix ViewMatrix { get; private set; }
    public Matrix ProjectionMatrix { get; private set; }
    
    public Vector3 PrevTargetPosition { get; private set; }
    public Vector3? TargetPosition { get; private set; }
    public bool TrackTargetX { get; private set; }
    public bool TrackTargetY { get; private set; }
    public bool TrackTargetZ { get; private set; }
    
    public static Camera ActiveCamera { get; private set; }

    public static void Init(float fieldOfViewY, Camera camera)
    {
        camera.FieldOfViewY = fieldOfViewY;
        camera.AspectRatio = Constants.CameraDefaultFarClip;
        camera.NearClip = Constants.CameraDefaultNearClip;
        camera.FarClip = Constants.CameraDefaultFarClip;
        camera.Up = new Vector3(0, 1, 0);
        camera.TargetPosition = Vector3.Zero;
        camera.TrackTargetX = false;
        camera.TrackTargetY = false;
        camera.TrackTargetZ = false;
    }

    public void SetAsActive()
    {
        ActiveCamera = this;
    }
    
    public static void ClearActive()
    {
        ActiveCamera = null;
    }

    public void ComputeViewMatrix()
    {
        if (ActiveCamera == null)
        {
            return;
        }

        if (ActiveCamera.TargetPosition.HasValue)
        {
            Vector3 targetPositionDelta;
            Vector3 resultPosition;

            targetPositionDelta = Vector3.Subtract(ActiveCamera.TargetPosition.Value, ActiveCamera.PrevTargetPosition);

            ViewMatrix = Matrix.CreateLookAt(ActiveCamera.Position, ActiveCamera.Target, ActiveCamera.Up);
        }
    }
    
    public void ComputeViewMatrixWithRoll()
    {
        
    }

    public void SetUp(Vector3 up)
    {
        Up = up;
    }

    public void CaptureTarget(ref Vector3 target)
    {
        TargetPosition = target;
        TrackTargetX = true;
        TrackTargetY = true;
        TrackTargetZ = true;
    }
    
    public void ReleaseTarget()
    {
        TargetPosition = null;
        TrackTargetX = true;
        TrackTargetY = true;
        TrackTargetZ = true;
    }

    public void SetClipping(float nearClip, float farClip)
    {
        NearClip = nearClip;
        FarClip = farClip;
    }
    
    public void InitWithTarget() {}
    
    public void InitWithPosition() {}
    
    public void InitWithTargetAndPosition() {}
    
    public void ComputeProjectionMatrix() {}

    public void SetFieldOfView(float fieldOfViewY)
    {
        FieldOfViewY = fieldOfViewY;
        ComputeProjectionMatrix();
    }
    
    public void AdjustFieldOfView(float amount)
    {
        FieldOfViewY += amount;
        ComputeProjectionMatrix();
    }

    public void Move(Vector3 delta)
    {
        Position += delta;
        Target += delta;
    }
}