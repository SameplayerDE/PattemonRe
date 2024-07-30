using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using PatteLib;

namespace HxCameraEditor.Graphics;

public static class CameraFactory
{
    public static Camera CreateFromDSPRE(int distance, ushort rotX, ushort rotY, ushort rotZ, bool ortho, ushort fieldOfView, int near, int far)
    {
        Camera camera = new Camera();
        camera.InitWithTarget(new Vector3(0, 0, 0), (float)NitroUtils.Fx32ToDecimal(distance), new Vector3(NitroUtils.GetAngleFromU16Int(rotX),NitroUtils.GetAngleFromU16Int(rotY), NitroUtils.GetAngleFromU16Int(rotZ)), (float)NitroUtils.GetAngleFromU16Int(fieldOfView), ortho ? CameraProjectionType.Orthographic : CameraProjectionType.Perspective, true);
        camera.SetClipping((float)NitroUtils.Fx32ToDecimal(near), (float)NitroUtils.Fx32ToDecimal(far));
        return camera;
    }
    
    public static GameCameraFile ToDSPRE(Camera camera)
    {
        var file = new GameCameraFile();
        file.distance = (uint)NitroUtils.DecimalToFx32((decimal)camera.Distance);
        file.vertRot = NitroUtils.GetU16IntFromAngle(camera.Rotation.X);
        file.horiRot = NitroUtils.GetU16IntFromAngle(camera.Rotation.Y);
        file.zRot = NitroUtils.GetU16IntFromAngle(camera.Rotation.Z);
        file.fov = NitroUtils.GetU16IntFromAngle(camera.FieldOfViewY);
        return file;
    }
    
    public static Dictionary<int, Camera> CreateFromFile(string path)
    {
        string[] lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            
        }

        return null;
    }
}