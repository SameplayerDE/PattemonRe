using System;
using Microsoft.Xna.Framework;

namespace TestRender.Graphics;

public class Camera
{

    public Vector3 Position { get; set; }
    public Vector3 Target { get; set; }
    public Vector3 Up { get; set; }
    public Vector3 Rotation { get; set; }
    public float Distance { get; set; }

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
        camera.AspectRatio = Constants.CameraDefaultAspectRatio;
        camera.NearClip = Constants.CameraDefaultNearClip;
        camera.FarClip = Constants.CameraDefaultFarClip;
        camera.Up = new Vector3(0, 1, 0);
        camera.TargetPosition = null;
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
        }
        ViewMatrix = Matrix.CreateLookAt(ActiveCamera.Position, ActiveCamera.Target, ActiveCamera.Up);
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
        ComputeProjectionMatrix();
    }
    
    public void InitWithTarget() {}

    public void InitWithPosition(Vector3 position, float distance, Vector3 rotation, float fieldOfViewY)
    {
        Init(fieldOfViewY, this);
        Position = position;
        Distance = distance;
        Rotation = rotation;

        AdjustTargetFromRotation(this);
        ComputeProjectionMatrix();
    }
    
    public void InitWithTargetAndPosition() {}

    public void ComputeProjectionMatrix()
    {
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfViewY), AspectRatio, NearClip, FarClip);
    }

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
    
    public void MoveAlongRotation(Vector3 delta)
    {
        Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
        Vector3 rotatedDelta = Vector3.TransformNormal(delta, rotationMatrix);
        
        Position += rotatedDelta;
        Target += rotatedDelta;
    }
    
    public void SetRotation(Vector3 rotation)
    {
        Rotation = rotation;
        AdjustTargetFromRotation(this);
    }

    //void SetAngleAroundTarget(const CameraAngle *angle, Camera *camera)
    //{
    //    camera->angle = *angle;
    //    Camera_AdjustPositionAroundTarget(camera);
    //}

    public void AdjustRotation(Vector3 amount)
    {
        Rotation += amount;
        AdjustTargetFromRotation(this);
    }

    //void Camera_AdjustAngleAroundTarget(const CameraAngle *amount, Camera *camera)
    //{
    //    camera->angle.x += amount->x;
    //    camera->angle.y += amount->y;
    //    camera->angle.z += amount->z;
    //    Camera_AdjustPositionAroundTarget(camera);
    //}

    public void SetDistance(float distance)
    {
        Distance = distance;
        //Camera_AdjustPositionAroundTarget(camera);
    }

    public void SetTargetAndUpdatePosition(Vector3 target)
    {
        Target = target;
        //Camera_AdjustPositionAroundTarget(camera);
    }

    public void Camera_AdjustDistance(float amount)
    {
        Distance += amount;
        //Camera_AdjustPositionAroundTarget(camera);
    }
    
    static void AdjustTargetFromRotation(Camera camera)
    {
        float rotationX = -camera.Rotation.X;
        float newX = (float)-(Math.Sin(camera.Rotation.Y) * camera.Distance * Math.Cos(camera.Rotation.X));
        float newZ = (float)-(Math.Cos(camera.Rotation.Y) * camera.Distance * Math.Cos(camera.Rotation.X));
        float newY = (float)-(Math.Sin(rotationX) * camera.Distance);

        camera.Target = new Vector3(newX, newY, newZ) + camera.Position; 
    }
}