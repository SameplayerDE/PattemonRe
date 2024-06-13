using System;
using System.Collections.Generic;

namespace HxGLTF
{

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

        public int TotalComponentCount => Type.NumberOfComponents * Count;
        public int BitsPerComponent => ComponentType.Bits;
        public int BytesPerComponent => ComponentType.Bits / 8;
        public int TotalByteCount => BytesPerComponent * TotalComponentCount;
    }

    public class AccessorReader
    {
        public static float[] ReadData(Accessor accessor)
        {
            if (accessor == null || accessor.BufferView == null || accessor.BufferView.Buffer == null || accessor.ComponentType == null || accessor.Type == null)
            {
                throw new ArgumentNullException("Accessor, BufferView, Buffer, Accessor.ComponentType, or Accessor.Type is null.");
            }

            var data = new float[accessor.TotalComponentCount];

            switch (accessor.ComponentType.Id)
            {
                case 5120: // Byte
                    //ReadDataInternal<sbyte>(data, bufferData, bufferOffset, accessorOffset, byteStride, componentSizeInBytes, componentCount, componentsPerElement);
                    ReadDataInternal<sbyte>(data, accessor);
                    break;
                case 5121: // UByte
                   // ReadDataInternal<byte>(data, bufferData, bufferOffset, accessorOffset, byteStride, componentSizeInBytes, componentCount, componentsPerElement);
                    ReadDataInternal<byte>(data, accessor);
                    break;
                case 5122: // Short
                    //ReadDataInternal<short>(data, bufferData, bufferOffset, accessorOffset, byteStride, componentSizeInBytes, componentCount, componentsPerElement);
                    ReadDataInternal<short>(data, accessor);
                    break;
                case 5123: // UShort
                   // ReadDataInternal<ushort>(data, bufferData, bufferOffset, accessorOffset, byteStride, componentSizeInBytes, componentCount, componentsPerElement);
                    ReadDataInternal<ushort>(data, accessor);
                    break;
                case 5125: // UInt
                   // ReadDataInternal<uint>(data, bufferData, bufferOffset, accessorOffset, byteStride, componentSizeInBytes, componentCount, componentsPerElement);
                    ReadDataInternal<uint>(data, accessor);
                    break;
                case 5126: // Float
                   // ReadDataInternal<float>(data, bufferData, bufferOffset, accessorOffset, byteStride, componentSizeInBytes, componentCount, componentsPerElement);
                    ReadDataInternal<float>(data, accessor);
                    break;
                default:
                    throw new ArgumentException("Unsupported component type.");
            }

            return data;
        }

        private static float ReadValue<T>(byte[] bufferData, int byteIndex) where T : struct
        {
            if (typeof(T) == typeof(sbyte))
            {
                return (float)Convert.ToSByte(bufferData[byteIndex]);
            }
            else if (typeof(T) == typeof(byte))
            {
                return (float)bufferData[byteIndex];
            }
            else if (typeof(T) == typeof(short))
            {
                return (float)BitConverter.ToInt16(bufferData, byteIndex);
            }
            else if (typeof(T) == typeof(ushort))
            {
                return (float)BitConverter.ToUInt16(bufferData, byteIndex);
            }
            else if (typeof(T) == typeof(uint))
            {
                return (float)BitConverter.ToUInt32(bufferData, byteIndex);
            }
            else if (typeof(T) == typeof(float))
            {
                return BitConverter.ToSingle(bufferData, byteIndex);
            }
            else
            {
                throw new ArgumentException("Unsupported data type.");
            }
        }

        private static void ReadDataInternal<T>(float[] data, Accessor accessor) where T : struct
        {

            var bufferData = accessor.BufferView.Buffer.Bytes;
            var bufferViewOffset = accessor.BufferView.ByteOffset;
            var accessorOffset = accessor.ByteOffset;

            var totalOffset = accessorOffset + bufferViewOffset;

            int i = 0;
            for (int a = 0; a < accessor.Type.NumberOfComponents * accessor.BytesPerComponent * accessor.Count; a += accessor.BytesPerComponent)
            {
                var index = a + totalOffset;
                var value = ReadValue<T>(bufferData, index);
                data[i++] = value;
            }
        }

        public static float[] ReadDataIndexed(Accessor dataAccessor, Accessor indexAccessor)
        {
            var dataBuffer = ReadData(dataAccessor);
            var indexBuffer = ReadData(indexAccessor);

            //var result = new float[indexAccessor.Count * dataAccessor.TypeComponentAmount()];
            var result = new List<float>();
            for (var x = 0; x < indexBuffer.Length; x++)
            {
                var index = (ushort)indexBuffer[x];
                for (var j = 0; j < dataAccessor.Type.NumberOfComponents; j++)
                {
                    var calculatedIndex = index * dataAccessor.Type.NumberOfComponents + j;
                    var d = dataBuffer[calculatedIndex];
                    result.Add(d);
                    //result[calculatedIndex] = d;
                }
            }

            return result.ToArray();
        }
        
        public static (float[],int[]) ReadDataIndexed2(Accessor dataAccessor, Accessor indexAccessor)
        {
            
// Read indices and data
            var indexBuffer = ReadData(indexAccessor);
            var dataBuffer = ReadData(dataAccessor);

// Filter unique indices and sort them in ascending order
            HashSet<int> uniqueIndicesSet = new HashSet<int>();
            foreach (int index in indexBuffer)
            {
                uniqueIndicesSet.Add(index);
            }

            List<int> uniqueIndices = uniqueIndicesSet.ToList();
            uniqueIndices.Sort();  // Sort in ascending order

            // Load data based on sorted unique indices
            List<float> uniqueData = new List<float>();
            Dictionary<int, int> indexMapping = new Dictionary<int, int>();
            int newIndex = 0;

            foreach (int index in uniqueIndices)
            {
                uniqueData.Add(dataBuffer[index]);
                indexMapping[index] = newIndex++;
            }

// Map the original indices to the new indices
            int[] newIndexBuffer = new int[indexBuffer.Length];
            for (int i = 0; i < indexBuffer.Length; i++)
            {
                newIndexBuffer[i] = indexMapping[(int)indexBuffer[i]];
            }

            return (uniqueData.ToArray(), newIndexBuffer);
        }

    }

}