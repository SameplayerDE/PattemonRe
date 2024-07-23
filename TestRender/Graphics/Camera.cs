using System;
using Microsoft.Xna.Framework;

namespace TestRender.Graphics;

public enum CameraProjectionType
{
    Perspective,
    Ortho
}

public unsafe class Camera
{

    public CameraProjectionType ProjectionType;

    public Vector3* Position;
    public Vector3* Target { get; set; }
    public Vector3 Up { get; set; }
    public Vector3 Rotation { get; set; }
    public float Distance { get; set; }

    public float FieldOfViewY { get; set; }
    public float FieldOfViewSin { get; set; }
    public float FieldOfViewCos { get; set; }
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

        
    public static void AdjustTargetAroundPosition(Camera camera)
    {
        float rotationX = -camera.Rotation.X;
        float newX = (float)-(Math.Sin(camera.Rotation.Y) * camera.Distance * Math.Cos(camera.Rotation.X));
        float newZ = (float)-(Math.Cos(camera.Rotation.Y) * camera.Distance * Math.Cos(camera.Rotation.X));
        float newY = (float)-(Math.Sin(rotationX) * camera.Distance);

        camera.Target->X = newX + camera.Position->X;
        camera.Target->Y = newY + camera.Position->Y;
        camera.Target->Z = newZ + camera.Position->Z;
    }
    
    public static void AdjustPositionAroundTarget(Camera camera)
    {
        float rotationX = -camera.Rotation.X;
        float newX = (float)(Math.Sin(camera.Rotation.Y) * camera.Distance * Math.Cos(camera.Rotation.X));
        float newZ = (float)(Math.Cos(camera.Rotation.Y) * camera.Distance * Math.Cos(camera.Rotation.X));
        float newY = (float)(Math.Sin(rotationX) * camera.Distance);

        camera.Position->X = newX + camera.Target->X;
        camera.Position->Y = newY + camera.Target->Y;
        camera.Position->Z = newZ + camera.Target->Z;
    }
    
    public static void AdjustDeltaPos(Camera camera, ref Vector3 delta)
    {
        if (camera.TrackTargetX == false) {
            delta.X = 0;
        }

        if (camera.TrackTargetY == false) {
            delta.Y = 0;
        }

        if (camera.TrackTargetZ == false) {
            delta.Z = 0;
        }
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
            Vector3 resultPosition;
            Vector3 targetPositionDelta = Vector3.Subtract(ActiveCamera.TargetPosition.Value, ActiveCamera.PrevTargetPosition);
            
            AdjustDeltaPos(ActiveCamera, ref targetPositionDelta);
            //Camera_UpdateHistory(sActiveCamera, &targetPosDelta, &resultPos);
            ActiveCamera.Move(targetPositionDelta);

            ActiveCamera.PrevTargetPosition = ActiveCamera.TargetPosition.Value;
        }
        ViewMatrix = Matrix.CreateLookAt(*ActiveCamera.Position, *ActiveCamera.Target, ActiveCamera.Up);
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
        ComputeProjectionMatrix(ProjectionType);
    }

    public void InitWithTarget(Vector3* target, float distance, Vector3 rotation, float fieldOfViewY, CameraProjectionType projectionType, bool trackTarget)
    {
        Init(fieldOfViewY, this);

        Target = target;
        Distance = distance;
        Rotation = rotation;

        AdjustPositionAroundTarget(this);
        ComputeProjectionMatrix(projectionType);

        if (trackTarget) {
            TargetPosition = *target;
            //prevTargetPos = *target;
            TrackTargetX = true;
            TrackTargetY = true;
            TrackTargetZ = true;
        }
    }

    public void InitWithPosition(Vector3* position, float distance, Vector3 rotation, float fieldOfViewY, CameraProjectionType projectionType)
    {
        Init(fieldOfViewY, this);
        Position = position;
        Distance = distance;
        Rotation = rotation;

        AdjustTargetAroundPosition(this);
        ComputeProjectionMatrix(projectionType);
    }
    
    public void InitWithTargetAndPosition() {}

    public void ComputeProjectionMatrix(CameraProjectionType projectionType)
    {
        ProjectionType = projectionType;
        if (projectionType == CameraProjectionType.Perspective)
        {
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfViewY), AspectRatio, NearClip, FarClip);
        }
        else
        {
            float top = (float)(Math.Sin(FieldOfViewY) / Math.Cos(FieldOfViewY) * Distance);
            float right = top * AspectRatio;

            ProjectionMatrix = Matrix.CreateOrthographicOffCenter(-right, right, -top, top, NearClip, FarClip);
        }
    }

    public void SetFieldOfView(float fieldOfViewY)
    {
        FieldOfViewY = fieldOfViewY;
        ComputeProjectionMatrix(ProjectionType);
    }
    
    public void AdjustFieldOfView(float amount)
    {
        FieldOfViewY += amount;
        ComputeProjectionMatrix(ProjectionType);
    }

    public void Move(Vector3 delta)
    {
        Position->X += delta.X;
        Position->Y += delta.Y;
        Position->Z += delta.Z;
        Target->X += delta.X;
        Target->Y += delta.Y;
        Target->Z += delta.Z;
    }
    
    public void MoveAlongRotation(Vector3 delta)
    {
        Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
        Vector3 rotatedDelta = Vector3.TransformNormal(delta, rotationMatrix);
        
        Position->X += rotatedDelta.X;
        Position->Y += rotatedDelta.Y;
        Position->Z += rotatedDelta.Z;
        Target->X += rotatedDelta.X;
        Target->Y += rotatedDelta.Y;
        Target->Z += rotatedDelta.Z;
    }
    
    public void SetRotation(Vector3 rotation)
    {
        Rotation = rotation;
        AdjustTargetAroundPosition(this);
    }

    //void SetAngleAroundTarget(const CameraAngle *angle, Camera *camera)
    //{
    //    camera->angle = *angle;
    //    Camera_AdjustPositionAroundTarget(camera);
    //}

    public void AdjustRotation(Vector3 amount)
    {
        Rotation += amount;
        AdjustTargetAroundPosition(this);
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
        AdjustPositionAroundTarget(this);
    }

    public void SetTargetAndUpdatePosition(Vector3* target)
    {
        Target = target;
        AdjustPositionAroundTarget(this);
    }

    public void AdjustDistance(float amount)
    {
        Distance += amount;
        AdjustPositionAroundTarget(this);
    }

}