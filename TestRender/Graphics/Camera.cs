using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TestRender.Graphics;

public enum CameraProjectionType
{
    Perspective,
    Orthographic
}

public class Camera
{

    public static Dictionary<int, Camera> CameraLookMap = new Dictionary<int, Camera>
    {
        { 0, CameraFactory.CreateFromDSPRE(2731713 / 16, 54786, 0, 0, false, 1473, 614400 / 16, 3686400 / 16) },
        { 4, CameraFactory.CreateFromDSPRE(6404251 / 16, 56418, 0, 0, true, 641, 614400 / 16, 7106560 / 16) }
    };
    
    public CameraProjectionType ProjectionType;

    public Vector3 Position;
    public Vector3 Target;
    public Vector3 Up;
    public Vector3 Rotation;
    public float Distance;
    
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

    public static Camera ActiveCamera;

    public static void Init(float fieldOfViewY, Camera camera)
    {
        camera.FieldOfViewY = fieldOfViewY;
        camera.FieldOfViewSin = (float)Math.Sin(camera.FieldOfViewY);
        camera.FieldOfViewCos = (float)Math.Cos(camera.FieldOfViewY);
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

        camera.Target = new Vector3(newX, newY, newZ) + camera.Position; 
    }
    
    public static void AdjustPositionAroundTarget(Camera camera)
    {
        float rotationX = -camera.Rotation.X;
        float newX = (float)(Math.Sin(camera.Rotation.Y) * camera.Distance * Math.Cos(camera.Rotation.X));
        float newZ = (float)(Math.Cos(camera.Rotation.Y) * camera.Distance * Math.Cos(camera.Rotation.X));
        float newY = (float)(Math.Sin(rotationX) * camera.Distance);
        
        camera.Position = new Vector3(newX, newY, newZ) + camera.Target; 
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

    public void SetProjectionType(CameraProjectionType type)
    {
        ProjectionType = type;
        ComputeProjectionMatrix(type);
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
        ComputeProjectionMatrix(ProjectionType);
    }

    public void InitWithTarget(Vector3 target, float distance, Vector3 rotation, float fieldOfViewY, CameraProjectionType projectionType, bool trackTarget)
    {
        Init(fieldOfViewY, this);

        Target = target;
        Distance = distance;
        Rotation = rotation;

        AdjustPositionAroundTarget(this);
        ComputeProjectionMatrix(projectionType);

        if (trackTarget) {
            TargetPosition = target;
            //prevTargetPos = *target;
            TrackTargetX = true;
            TrackTargetY = true;
            TrackTargetZ = true;
        }
    }

    public void InitWithPosition(Vector3 position, float distance, Vector3 rotation, float fieldOfViewY, CameraProjectionType projectionType)
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
            float fieldOfViewY = (float)(2 * Math.Atan2(FieldOfViewSin, FieldOfViewCos));
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfViewY, AspectRatio, NearClip, FarClip);
        }
        else
        {
            float top = FieldOfViewSin / FieldOfViewCos * Distance;
            float right = top * AspectRatio;

            ProjectionMatrix = Matrix.CreateOrthographicOffCenter(-right, right, -top, top, NearClip, FarClip);
        }
    }

    public void SetFieldOfView(float fieldOfViewY)
    {
        FieldOfViewY = fieldOfViewY;
        FieldOfViewSin = (float)Math.Sin(FieldOfViewY);
        FieldOfViewCos = (float)Math.Cos(FieldOfViewY);
        ComputeProjectionMatrix(ProjectionType);
    }
    
    public void AdjustFieldOfView(float amount)
    {
        FieldOfViewY += amount;
        FieldOfViewSin = (float)Math.Sin(FieldOfViewY);
        FieldOfViewCos = (float)Math.Cos(FieldOfViewY);
        ComputeProjectionMatrix(ProjectionType);
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
        AdjustTargetAroundPosition(this);
    }

    public void SetRotationAroundTarget(Vector3 rotation)
    {
        Rotation = rotation;
        AdjustPositionAroundTarget(this);
    }

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

    public void SetTargetAndUpdatePosition(Vector3 target)
    {
        Target = target;
        AdjustPositionAroundTarget(this);
    }

    public void Camera_AdjustDistance(float amount)
    {
        Distance += amount;
        AdjustPositionAroundTarget(this);
    }

}