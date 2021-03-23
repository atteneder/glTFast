using System;
using GLTFast;
using GLTFast.Schema;
using UnityEngine;

namespace GLTFast
{
    public static class VertexInputDataExtensions
    {
        public static int[] GetIntArray(this VertexInputData vertexInputData, bool flipFaces = true)
        {
            var accessor = vertexInputData.accessor;
            if (accessor.typeEnum != GLTFAccessorAttributeType.SCALAR)
            {
                return null;
            }

            var array = new int[accessor.count];

            GetTypeDetails(accessor.componentType, out int componentSize, out float _);
            var stride = vertexInputData.byteStride > 0 ? vertexInputData.byteStride : componentSize;
            var bufferData = vertexInputData.buffer;

            for (int i = 0; i < accessor.count; i++)
            {
                if (accessor.componentType == GLTFComponentType.Float)
                {
                    array[i] = (int)Mathf.Floor(BitConverter.ToSingle(bufferData, vertexInputData.startOffset + i * stride));
                }
                else
                {
                    array[i] = (int)GetDiscreteUnsignedElement(bufferData, vertexInputData.startOffset + i * stride, accessor.componentType);
                }
            }

            if (flipFaces)
            {
                for (int i = 0; i < array.Length; i += 3)
                {
                    var temp = array[i];
                    array[i] = array[i + 2];
                    array[i + 2] = temp;
                }
            }

            return array;
        }
        
        public static Color[] GetColorArray(this VertexInputData vertexInputData)
        {
            var accessor = vertexInputData.accessor;
            if (accessor.typeEnum != GLTFAccessorAttributeType.VEC3 && accessor.typeEnum != GLTFAccessorAttributeType.VEC4 || accessor.componentType == GLTFComponentType.UnsignedInt)
            {
                return null;
            }

            var array = new Color[accessor.count];

            GetTypeDetails(accessor.componentType, out int componentSize, out float maxValue);
            bool hasAlpha = accessor.typeEnum == GLTFAccessorAttributeType.VEC4;

            var stride = vertexInputData.byteStride > 0 ? vertexInputData.byteStride : componentSize * (hasAlpha ? 4 : 3);
            var bufferData = vertexInputData.buffer;

            for (int i = 0; i < accessor.count; i++)
            {
                if (accessor.componentType == GLTFComponentType.Float)
                {
                    array[i].r = BitConverter.ToSingle(bufferData, vertexInputData.startOffset + i * stride + componentSize * 0);
                    array[i].g = BitConverter.ToSingle(bufferData, vertexInputData.startOffset + i * stride + componentSize * 1);
                    array[i].b = BitConverter.ToSingle(bufferData, vertexInputData.startOffset + i * stride + componentSize * 2);
                    array[i].a = hasAlpha ? BitConverter.ToSingle(bufferData, vertexInputData.startOffset + i * stride + componentSize * 3) : 1f;
                }
                else
                {
                    array[i].r = GetDiscreteElement(bufferData, vertexInputData.startOffset + i * stride + componentSize * 0, accessor.componentType) / maxValue;
                    array[i].g = GetDiscreteElement(bufferData, vertexInputData.startOffset + i * stride + componentSize * 1, accessor.componentType) / maxValue;
                    array[i].b = GetDiscreteElement(bufferData, vertexInputData.startOffset + i * stride + componentSize * 2, accessor.componentType) / maxValue;
                    array[i].a = hasAlpha ? GetDiscreteElement(bufferData, vertexInputData.startOffset + i * stride + componentSize * 3, accessor.componentType) / maxValue : 1f;
                }
            }

            return array;
        }

        private static void GetTypeDetails(GLTFComponentType type, out int componentSize, out float maxValue)
        {
            componentSize = 1;
            maxValue = byte.MaxValue;

            switch (type)
            {
                case GLTFComponentType.Byte:
                    componentSize = sizeof(sbyte);
                    maxValue = sbyte.MaxValue;
                    break;
                case GLTFComponentType.UnsignedByte:
                    componentSize = sizeof(byte);
                    maxValue = byte.MaxValue;
                    break;
                case GLTFComponentType.Short:
                    componentSize = sizeof(short);
                    maxValue = short.MaxValue;
                    break;
                case GLTFComponentType.UnsignedShort:
                    componentSize = sizeof(ushort);
                    maxValue = ushort.MaxValue;
                    break;
                case GLTFComponentType.UnsignedInt:
                    componentSize = sizeof(uint);
                    maxValue = uint.MaxValue;
                    break;
                case GLTFComponentType.Float:
                    componentSize = sizeof(float);
                    maxValue = float.MaxValue;
                    break;
                default:
                    throw new Exception("Unsupported component type.");
            }
        }

        private static int GetDiscreteElement(byte[] data, int offset, GLTFComponentType type)
        {
            switch (type)
            {
                case GLTFComponentType.Byte:
                    return Convert.ToSByte(data[offset]);
                case GLTFComponentType.UnsignedByte:
                    return data[offset];
                case GLTFComponentType.Short:
                    return BitConverter.ToInt16(data, offset);
                case GLTFComponentType.UnsignedShort:
                    return BitConverter.ToUInt16(data, offset);
                case GLTFComponentType.UnsignedInt:
                    return (int)BitConverter.ToUInt32(data, offset);
                default:
                    throw new Exception($"Unsupported type passed in: {type}");
            }
        }

        private static uint GetDiscreteUnsignedElement(byte[] data, int offset, GLTFComponentType type)
        {
            switch (type)
            {
                case GLTFComponentType.Byte:
                    return (uint)Convert.ToSByte(data[offset]);
                case GLTFComponentType.UnsignedByte:
                    return data[offset];
                case GLTFComponentType.Short:
                    return (uint)BitConverter.ToInt16(data, offset);
                case GLTFComponentType.UnsignedShort:
                    return BitConverter.ToUInt16(data, offset);
                case GLTFComponentType.UnsignedInt:
                    return BitConverter.ToUInt32(data, offset);
                default:
                    throw new Exception($"Unsupported type passed in: {type}");
            }
        }
    }
}
