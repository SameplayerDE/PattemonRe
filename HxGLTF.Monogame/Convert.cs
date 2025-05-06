using Microsoft.Xna.Framework;

namespace HxGLTF.Monogame;

internal static class Convert
{
    public static Vector2 ToVector2(HxGLTF.Core.PrimitiveDataStructures.Vector2 vector) =>
        new Vector2(vector.X, vector.Y);

    public static Vector3 ToVector3(HxGLTF.Core.PrimitiveDataStructures.Vector3 vector) =>
        new Vector3(vector.X, vector.Y, vector.Z);

    public static Vector4 ToVector4(HxGLTF.Core.PrimitiveDataStructures.Vector4 vector) =>
        new Vector4(vector.X, vector.Y, vector.Z, vector.W);

    public static Quaternion ToQuaternion(HxGLTF.Core.PrimitiveDataStructures.Quaternion quaternion) =>
        new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

    public static Matrix ToMatrix(HxGLTF.Core.PrimitiveDataStructures.Matrix matrix) =>
        new Matrix(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        );
}