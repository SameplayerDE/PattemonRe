using System;
using System.IO;

namespace HxCameraEditor;

public class GameCameraFile
{
    public const byte PERSPECTIVE = 0;
    public const byte ORTHO = 1;

    public uint distance { get; set; }

    public ushort vertRot { get;  set; }
    public ushort horiRot { get;  set; }
    public ushort zRot { get;  set; }
    public ushort unk1 { get;  set; }

    public byte perspMode { get;  set; }

    public byte unk2 { get;  set; }


    public ushort fov { get;  set; }
    public uint nearClip { get;  set; }
    public uint farClip { get;  set; }

    public int? xOffset { get;  set; }
    public int? yOffset { get;  set; }
    public int? zOffset { get;  set; }

    public object this[int index] {
        get {
            switch (index) {
                case 0:
                    return distance;
                case 1:
                    return vertRot;
                case 2:
                    return horiRot;
                case 3:
                    return zRot;
                case 4:
                    return perspMode;
                case 5:
                    return fov;
                case 6:
                    return nearClip;
                case 7:
                    return farClip;
                case 8:
                    return xOffset;
                case 9:
                    return yOffset;
                case 10:
                    return zOffset;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        set {
            try
            {
                switch (index)
                {
                    case 0:
                        distance = Convert.ToUInt32(value);
                        break;
                    case 1:
                        vertRot = Convert.ToUInt16(value);
                        break;
                    case 2:
                        horiRot = Convert.ToUInt16(value);
                        break;
                    case 3:
                        zRot = Convert.ToUInt16(value);
                        break;
                    case 4:
                        perspMode = (byte)(Convert.ToBoolean(value) ? 1 : 0);
                        break;
                    case 5:
                        fov = Convert.ToUInt16(value);
                        break;
                    case 6:
                        nearClip = Convert.ToUInt32(value);
                        break;
                    case 7:
                        farClip = Convert.ToUInt32(value);
                        break;
                    case 8:
                        xOffset = Convert.ToInt32(value);
                        break;
                    case 9:
                        yOffset = Convert.ToInt32(value);
                        break;
                    case 10:
                        zOffset = Convert.ToInt32(value);
                        break;
                }
            }
            catch (Exception e)
            {
                //ignore
            }
        }
    }

    public GameCameraFile(uint distance = 0x29AEC1, ushort vertRot = 0xD62, ushort horiRot = 0, ushort zRot = 0,
        ushort unk1 = 0, byte perspMode = PERSPECTIVE, byte unk2 = 0,
        ushort fov = 1473, uint nearClip = 614400, uint farClip = 0x384000,
        int? xOffset = null, int? yOffset = null, int? zOffset = null) {

        this.distance = distance;
        this.vertRot = vertRot;
        this.horiRot = horiRot;
        this.zRot = zRot;

        this.unk1 = unk1;
        this.perspMode = perspMode;
        this.unk2 = unk2;

        this.fov = fov;
        this.nearClip = nearClip;
        this.farClip = farClip;

        this.xOffset = xOffset;
        this.yOffset = yOffset;
        this.zOffset = zOffset;
    }
    public GameCameraFile(byte[] camData) {
        if (camData.Length != 36 && camData.Length != 24) {
            return;
        }
        try {
            using (BinaryReader b = new BinaryReader(new MemoryStream(camData))) {
                distance = b.ReadUInt32();
                vertRot = b.ReadUInt16();
                horiRot = b.ReadUInt16();
                zRot = b.ReadUInt16();

                unk1 = b.ReadUInt16();
                perspMode = b.ReadByte();
                unk2 = b.ReadByte();

                fov = b.ReadUInt16();
                nearClip = b.ReadUInt32();
                farClip = b.ReadUInt32();
            }
        } catch (Exception exception) {
            //ignore
        }
    }
    public byte[] ToByteArray() {
        MemoryStream newData = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(newData)) {
            writer.Write(distance);
            writer.Write(vertRot);
            writer.Write(horiRot);
            writer.Write(zRot);

            writer.Write(unk1);
            writer.Write(perspMode);
            writer.Write(unk2);

            writer.Write(fov);
            writer.Write(nearClip);
            writer.Write(farClip);

            if (xOffset != null) {
                writer.Write((int)xOffset);
            }

            if (yOffset != null) {
                writer.Write((int)yOffset);
            }

            if (zOffset != null) {
                writer.Write((int)zOffset);
            }
        }

        return newData.ToArray();
    }
}