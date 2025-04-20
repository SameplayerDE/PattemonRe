using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HxGLTF;

public class ComponentType
{
    public static ComponentType T5120 = new ComponentType(5120, 8);  //Byte
    public static ComponentType T5121 = new ComponentType(5121, 8);  //UByte
    public static ComponentType T5122 = new ComponentType(5122, 16); //Short
    public static ComponentType T5123 = new ComponentType(5123, 16); //UShort
    public static ComponentType T5125 = new ComponentType(5125, 32); //UInt
    public static ComponentType T5126 = new ComponentType(5126, 32); //Float
        
    private static Dictionary<int, ComponentType> _types = new Dictionary<int, ComponentType>()
    {
        {T5120.Id, T5120},
        {T5121.Id, T5121},
        {T5122.Id, T5122},
        {T5123.Id, T5123},
        {T5125.Id, T5125},
        {T5126.Id, T5126}
    };

    public int Id { get; }
    public int Bits { get; }

    private ComponentType(int id, int bits)
    {
        Id = id;
        Bits = bits;
    }
        
        
    public static ComponentType FromInt(int type)
    {
        return _types.ContainsKey(type) ? _types[type] : null;
    }
}
    
public class Type
{
    public readonly static Type Scalar = new Type("SCALAR", 1);
    public readonly static Type Vec2   = new Type("VEC2", 2);  // vector2
    public readonly static Type Vec3   = new Type("VEC3", 3);  // vector3
    public readonly static Type Vec4   = new Type("VEC4", 4);  // vector4
    public readonly static Type Mat2   = new Type("MAT2", 4);  // 2x2 matrix
    public readonly static Type Mat3   = new Type("MAT3", 9);  // 3x3 matrix
    public readonly static Type Mat4   = new Type("MAT4", 16); // 4x4 matrix

    private static Dictionary<string, Type> _types = new Dictionary<string, Type>()
    {
        {Scalar.Id, Scalar},
        {Vec2.Id, Vec2},
        {Vec3.Id, Vec3},
        {Vec4.Id, Vec4},
        {Mat2.Id, Mat2},
        {Mat3.Id, Mat3},
        {Mat4.Id, Mat4},
    };

    public string Id { get; }
    public int NumberOfComponents { get; }

    private Type(string id, int numberOfComponents)
    {
        Id = id;
        NumberOfComponents = numberOfComponents;
    }
        
    public static Type FromSting(string type)
    {
        if (type == null)
        {
            return null;
        }
        return _types.ContainsKey(type.ToUpper()) ? _types[type.ToUpper()] : null;
    }
}
    
public class Accessor
{
    public BufferView BufferView;
    public int ByteOffset = 0;
    public ComponentType ComponentType;
    public bool Normalized = false;
    public int Count;
    public Type Type;
    public int Max;
    public int Min;
    public string Name;

    public int TotalComponentCount => Type.NumberOfComponents * Count;
    public int BitsPerComponent => ComponentType.Bits;
    public int BytesPerComponent => ComponentType.Bits / 8;
    public int TotalByteCount => BytesPerComponent * TotalComponentCount;
}

public static class AccessorReader
{

    public static float[] ReadData(Accessor accessor)
    {
        if (accessor == null || accessor.BufferView == null || accessor.BufferView.Buffer == null || accessor.ComponentType == null || accessor.Type == null)
        {
            throw new ArgumentNullException(nameof(accessor), "Accessor, BufferView, Buffer, ComponentType, or Type is null.");
        }

        float[] data = new float[accessor.TotalComponentCount];
        switch (accessor.ComponentType.Id)
        {
            case 5120:
                ReadDataInternal<sbyte>(data, accessor);
                break;
            case 5121:
                ReadDataInternal<byte>(data, accessor);
                break;
            case 5122:
                ReadDataInternal<short>(data, accessor);
                break;
            case 5123:
                ReadDataInternal<ushort>(data, accessor);
                break;
            case 5125:
                ReadDataInternal<uint>(data, accessor);
                break;
            case 5126:
                ReadDataInternal<float>(data, accessor);
                break;
            default:
                throw new ArgumentException("Unsupported component type.");
        }

        return data;
    }

    private static float ReadValue<T>(ReadOnlySpan<byte> bufferSpan, int byteIndex, bool debug = false) where T : struct
    {
        if (typeof(T) == typeof(sbyte))
        {
            return (sbyte)bufferSpan[byteIndex];
        }

        if (typeof(T) == typeof(byte))
        {
            return bufferSpan[byteIndex];
        }

        if (typeof(T) == typeof(short))
        {
            return BitConverter.ToInt16(bufferSpan.Slice(byteIndex, sizeof(short)));
        }
        if (typeof(T) == typeof(ushort))
        {
            return BitConverter.ToUInt16(bufferSpan.Slice(byteIndex, sizeof(ushort)));
        }
        if (typeof(T) == typeof(uint))
        {
            return BitConverter.ToUInt32(bufferSpan.Slice(byteIndex, sizeof(uint)));
        }
        if (typeof(T) == typeof(float))
        {
            return BitConverter.ToSingle(bufferSpan.Slice(byteIndex, sizeof(float)));
        }
        throw new ArgumentException("Unsupported data type.");
    }

    private static void ReadDataInternal<T>(float[] data, Accessor accessor) where T : struct
    {
        ReadOnlySpan<byte> bufferSpan = accessor.BufferView.Buffer.Bytes.Span;
        int totalOffset = accessor.ByteOffset + accessor.BufferView.ByteOffset;
        int displacement = accessor.BufferView.ByteStride;
        int bytesPerComp = accessor.BytesPerComponent;
        int compCount = accessor.Type.NumberOfComponents;

        if (displacement != 0)
        {
            /*
             * The data of the attributes that are stored in a single bufferView may be stored as an Array-Of-Structures.
             * A single bufferView may, for example, contain the data for vertex positions and for vertex normals in an interleaved fashion.
             * In this case, the byteOffset of an accessor defines the start of the first relevant data element for the respective attribute,
             * and the bufferView defines an additional byteStride property. This is the number of bytes between the start of one element
             * of its accessors, and the start of the next one.
             */
            for (int i = 0; i < accessor.Count; i++)
            {
                for (int j = 0; j < compCount; j++)
                {
                    int elementOffset = totalOffset + i * displacement + j * bytesPerComp;
                    data[i * compCount + j] = ReadValue<T>(bufferSpan, elementOffset);
                }
            }
        }
        else
        {
            int index = 0;
            int totalBytesLoop = compCount * bytesPerComp * accessor.Count;
            for (int a = 0; a < totalBytesLoop; a += bytesPerComp)
            {
                int byteIndex = totalOffset + a;
                if (byteIndex >= bufferSpan.Length)
                {
                    throw new Exception("Index out of range while reading buffer data.");
                }
                data[index++] = ReadValue<T>(bufferSpan, byteIndex);
            }
        }
    }

    public static float[] ReadDataIndexed(Accessor dataAccessor, Accessor indexAccessor)
    {
        float[] dataBuffer = ReadData(dataAccessor);
        float[] indexBuffer = ReadData(indexAccessor);
        List<float> result = new List<float>();
        int compCount = dataAccessor.Type.NumberOfComponents;

        for (int x = 0; x < indexBuffer.Length; x++)
        {
            ushort idx = (ushort)indexBuffer[x];
            for (int j = 0; j < compCount; j++)
            {
                result.Add(dataBuffer[idx * compCount + j]);
            }
        }

        return result.ToArray();
    }

    public static (float[] uniqueData, int[] remappedIndices) ReadDataIndexed2(Accessor dataAccessor, Accessor indexAccessor)
    {
        float[] indexBuffer = ReadData(indexAccessor);
        float[] dataBuffer = ReadData(dataAccessor);
        HashSet<int> uniqueIndexSet = new HashSet<int>();
        foreach (float f in indexBuffer)
        {
            uniqueIndexSet.Add((int)f);
        }

        List<int> uniqueIndices = new List<int>(uniqueIndexSet);
        uniqueIndices.Sort();
        float[] uniqueData = new float[uniqueIndices.Count];
        Dictionary<int, int> mapping = new Dictionary<int, int>();

        for (int i = 0; i < uniqueIndices.Count; i++)
        {
            mapping[uniqueIndices[i]] = i;
            uniqueData[i] = dataBuffer[uniqueIndices[i]];
        }

        int[] remapped = new int[indexBuffer.Length];
        for (int i = 0; i < indexBuffer.Length; i++)
        {
            remapped[i] = mapping[(int)indexBuffer[i]];
        }

        return (uniqueData, remapped);
    }
}