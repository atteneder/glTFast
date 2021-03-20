// using System;
// using GLTFast.Schema;
// using UnityEngine;
//
// namespace glTFast.Runtime.Scripts.Schema
// {
//     public static class AccessorExtensions
//     {
//         
//         private static readonly string scalar = GLTFAccessorAttributeType.SCALAR.ToString();
//         private static readonly string vec2 = GLTFAccessorAttributeType.VEC2.ToString();
//         private static readonly string vec3 = GLTFAccessorAttributeType.VEC3.ToString();
//         private static readonly string vec4 = GLTFAccessorAttributeType.VEC4.ToString();
//         
//         public static int[] GetIntArray(this Accessor accessor, bool flipFaces = true)
//         {
//             if (accessor.type != scalar)
//             {
//                 return null;
//             }
//
//             var array = new int[accessor.count];
//
//             GetTypeDetails(accessor.componentType, out int componentSize, out float _);
//             var stride = accessor.BufferView.byteStride > 0 ?accessor.bufferView.byteStride : componentSize;
//
//             var bufferData = accessor.BufferView.Buffer.BufferData;
//
//             if (byteOffset >= 0)
//             {
//                 byteOffset += byteOffset;
//             }
//
//             for (int i = 0; i < accessor.count; i++)
//             {
//                 if (accessor.componentType == GLTFComponentType.Float)
//                 {
//                     array[i] = (int)Mathf.Floor(BitConverter.ToSingle(bufferData, accessor.byteOffset + i * stride));
//                 }
//                 else
//                 {
//                     array[i] = (int)GetDiscreteUnsignedElement(bufferData, accessor.byteOffset + i * stride, accessor.componentType);
//                 }
//             }
//
//             if (flipFaces)
//             {
//                 for (int i = 0; i < array.Length; i += 3)
//                 {
//                     var temp = array[i];
//                     array[i] = array[i + 2];
//                     array[i + 2] = temp;
//                 }
//             }
//
//             return array;
//         }
//         
//         public static Color[] GetColorArray(this Accessor accessor)
//         {
//             if (accessor.type != vec3 && accessor.type != vec4 || accessor.componentType == GLTFComponentType.UnsignedInt)
//             {
//                 return null;
//             }
//
//             var array = new Color[accessor.count];
//
//             GetTypeDetails(accessor.componentType, out int componentSize, out float maxValue);
//             bool hasAlpha = accessor.type == vec4;
//
//             var stride = accessor.BufferView.byteStride > 0 ? accessor.BufferView.byteStride : componentSize * (hasAlpha ? 4 : 3);
//             var byteOffset = accessor.BufferView.byteOffset;
//             var bufferData = accessor.BufferView.Buffer.BufferData;
//
//             if (accessor.byteOffset >= 0)
//             {
//                 byteOffset += accessor.byteOffset;
//             }
//
//             for (int i = 0; i < accessor.count; i++)
//             {
//                 if (accessor.componentType == GLTFComponentType.Float)
//                 {
//                     array[i].r = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 0);
//                     array[i].g = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 1);
//                     array[i].b = BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 2);
//                     array[i].a = hasAlpha ? BitConverter.ToSingle(bufferData, byteOffset + i * stride + componentSize * 3) : 1f;
//                 }
//                 else
//                 {
//                     array[i].r = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 0, accessor.componentType) / maxValue;
//                     array[i].g = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 1, accessor.componentType) / maxValue;
//                     array[i].b = GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 2, accessor.componentType) / maxValue;
//                     array[i].a = hasAlpha ? GetDiscreteElement(bufferData, byteOffset + i * stride + componentSize * 3, accessor.componentType) / maxValue : 1f;
//                 }
//             }
//
//             return array;
//         }
//
//         private static void GetTypeDetails(GLTFComponentType type, out int componentSize, out float maxValue)
//         {
//             componentSize = 1;
//             maxValue = byte.MaxValue;
//
//             switch (type)
//             {
//                 case GLTFComponentType.Byte:
//                     componentSize = sizeof(sbyte);
//                     maxValue = sbyte.MaxValue;
//                     break;
//                 case GLTFComponentType.UnsignedByte:
//                     componentSize = sizeof(byte);
//                     maxValue = byte.MaxValue;
//                     break;
//                 case GLTFComponentType.Short:
//                     componentSize = sizeof(short);
//                     maxValue = short.MaxValue;
//                     break;
//                 case GLTFComponentType.UnsignedShort:
//                     componentSize = sizeof(ushort);
//                     maxValue = ushort.MaxValue;
//                     break;
//                 case GLTFComponentType.UnsignedInt:
//                     componentSize = sizeof(uint);
//                     maxValue = uint.MaxValue;
//                     break;
//                 case GLTFComponentType.Float:
//                     componentSize = sizeof(float);
//                     maxValue = float.MaxValue;
//                     break;
//                 default:
//                     throw new Exception("Unsupported component type.");
//             }
//         }
//
//         private static int GetDiscreteElement(byte[] data, int offset, GLTFComponentType type)
//         {
//             switch (type)
//             {
//                 case GLTFComponentType.Byte:
//                     return Convert.ToSByte(data[offset]);
//                 case GLTFComponentType.UnsignedByte:
//                     return data[offset];
//                 case GLTFComponentType.Short:
//                     return BitConverter.ToInt16(data, offset);
//                 case GLTFComponentType.UnsignedShort:
//                     return BitConverter.ToUInt16(data, offset);
//                 case GLTFComponentType.UnsignedInt:
//                     return (int)BitConverter.ToUInt32(data, offset);
//                 default:
//                     throw new Exception($"Unsupported type passed in: {type}");
//             }
//         }
//
//         private static uint GetDiscreteUnsignedElement(byte[] data, int offset, GLTFComponentType type)
//         {
//             switch (type)
//             {
//                 case GLTFComponentType.Byte:
//                     return (uint)Convert.ToSByte(data[offset]);
//                 case GLTFComponentType.UnsignedByte:
//                     return data[offset];
//                 case GLTFComponentType.Short:
//                     return (uint)BitConverter.ToInt16(data, offset);
//                 case GLTFComponentType.UnsignedShort:
//                     return BitConverter.ToUInt16(data, offset);
//                 case GLTFComponentType.UnsignedInt:
//                     return BitConverter.ToUInt32(data, offset);
//                 default:
//                     throw new Exception($"Unsupported type passed in: {type}");
//             }
//         }
//     }
// }
