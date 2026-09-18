// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using AOT;
using Unity.Burst;
using Unity.Cloud.Gltfast.Objects;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Cloud.Gltfast.Jobs
{
    [BurstCompile]
    static unsafe class CachedFunction
    {
        public delegate int GetIndexDelegate(void* baseAddress, int index);
        public delegate void GetFloat3Delegate(float3* destination, void* src);

        // Cached function pointers
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueInt8Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueUInt8Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueInt16Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueUInt16Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueUInt32Method;

        static FunctionPointer<GetFloat3Delegate> s_GetFloat3FloatMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int8Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt8Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int16Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt16Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt32Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int8NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt8NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int16NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt16NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt32NormalizedMethod;

        /// <summary>
        /// Returns Burst compatible function that retrieves an index value
        /// </summary>
        /// <param name="format">Data type of index</param>
        /// <returns>Burst Function Pointer to correct conversion function</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static FunctionPointer<GetIndexDelegate> GetIndexConverter(AccessorDataType format)
        {
            switch (format)
            {
                case AccessorDataType.UnsignedByte:
                    if (!s_GetIndexValueUInt8Method.IsCreated)
                    {
                        s_GetIndexValueUInt8Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt8);
                    }
                    return s_GetIndexValueUInt8Method;
                case AccessorDataType.Byte:
                    if (!s_GetIndexValueInt8Method.IsCreated)
                    {
                        s_GetIndexValueInt8Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueInt8);
                    }
                    return s_GetIndexValueInt8Method;
                case AccessorDataType.UnsignedShort:
                    if (!s_GetIndexValueUInt16Method.IsCreated)
                    {
                        s_GetIndexValueUInt16Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt16);
                    }
                    return s_GetIndexValueUInt16Method;
                case AccessorDataType.Short:
                    if (!s_GetIndexValueInt16Method.IsCreated)
                    {
                        s_GetIndexValueInt16Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueInt16);
                    }
                    return s_GetIndexValueInt16Method;
                case AccessorDataType.UnsignedInt:
                    if (!s_GetIndexValueUInt32Method.IsCreated)
                    {
                        s_GetIndexValueUInt32Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt32);
                    }
                    return s_GetIndexValueUInt32Method;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public static FunctionPointer<GetFloat3Delegate> GetPositionConverter(
            AccessorDataType format,
            bool normalized
        )
        {
            if (normalized)
            {
                switch (format)
                {
                    case AccessorDataType.Float:
                        // Floats cannot be normalized.
                        // Fall back to non-normalized below
                        break;
                    case AccessorDataType.Byte:
                        if (!s_GetFloat3Int8NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3Int8NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int8Normalized);
                        }
                        return s_GetFloat3Int8NormalizedMethod;
                    case AccessorDataType.UnsignedByte:
                        if (!s_GetFloat3UInt8NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3UInt8NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt8Normalized);
                        }
                        return s_GetFloat3UInt8NormalizedMethod;
                    case AccessorDataType.Short:
                        if (!s_GetFloat3Int16NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3Int16NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int16Normalized);
                        }
                        return s_GetFloat3Int16NormalizedMethod;
                    case AccessorDataType.UnsignedShort:
                        if (!s_GetFloat3UInt16NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3UInt16NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt16Normalized);
                        }
                        return s_GetFloat3UInt16NormalizedMethod;
                    case AccessorDataType.UnsignedInt:
                        if (!s_GetFloat3UInt32NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3UInt32NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt32Normalized);
                        }
                        return s_GetFloat3UInt32NormalizedMethod;
                }
            }
            switch (format)
            {
                case AccessorDataType.Float:
                    if (!s_GetFloat3FloatMethod.IsCreated)
                    {
                        s_GetFloat3FloatMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Float);
                    }
                    return s_GetFloat3FloatMethod;
                case AccessorDataType.Byte:
                    if (!s_GetFloat3Int8Method.IsCreated)
                    {
                        s_GetFloat3Int8Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int8);
                    }
                    return s_GetFloat3Int8Method;
                case AccessorDataType.UnsignedByte:
                    if (!s_GetFloat3UInt8Method.IsCreated)
                    {
                        s_GetFloat3UInt8Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt8);
                    }
                    return s_GetFloat3UInt8Method;
                case AccessorDataType.Short:
                    if (!s_GetFloat3Int16Method.IsCreated)
                    {
                        s_GetFloat3Int16Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int16);
                    }
                    return s_GetFloat3Int16Method;
                case AccessorDataType.UnsignedShort:
                    if (!s_GetFloat3UInt16Method.IsCreated)
                    {
                        s_GetFloat3UInt16Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt16);
                    }
                    return s_GetFloat3UInt16Method;
                case AccessorDataType.UnsignedInt:
                    if (!s_GetFloat3UInt32Method.IsCreated)
                    {
                        s_GetFloat3UInt32Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt32);
                    }
                    return s_GetFloat3UInt32Method;
            }
            throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt8(void* baseAddress, int index)
        {
            return *((byte*)baseAddress + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueInt8(void* baseAddress, int index)
        {
            return *(((sbyte*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt16(void* baseAddress, int index)
        {
            return *(((ushort*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueInt16(void* baseAddress, int index)
        {
            return *(((short*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt32(void* baseAddress, int index)
        {
            return (int)*(((uint*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Float(float3* destination, void* src)
        {
            destination->x = -*(float*)src;
            destination->y = *((float*)src + 1);
            destination->z = *((float*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int8(float3* destination, void* src)
        {
            destination->x = -*(sbyte*)src;
            destination->y = *((sbyte*)src + 1);
            destination->z = *((sbyte*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt8(float3* destination, void* src)
        {
            destination->x = -*(byte*)src;
            destination->y = *((byte*)src + 1);
            destination->z = *((byte*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int16(float3* destination, void* src)
        {
            destination->x = -*(short*)src;
            destination->y = *((short*)src + 1);
            destination->z = *((short*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt16(float3* destination, void* src)
        {
            destination->x = -*(ushort*)src;
            destination->y = *((ushort*)src + 1);
            destination->z = *((ushort*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt32(float3* destination, void* src)
        {
            destination->x = -*(uint*)src;
            destination->y = *((uint*)src + 1);
            destination->z = *((uint*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int8Normalized(float3* destination, void* src)
        {
            destination->x = -math.max(*(sbyte*)src / 127f, -1);
            destination->y = math.max(*((sbyte*)src + 1) / 127f, -1);
            destination->z = math.max(*((sbyte*)src + 2) / 127f, -1);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt8Normalized(float3* destination, void* src)
        {
            destination->x = -*(byte*)src / 255f;
            destination->y = *((byte*)src + 1) / 255f;
            destination->z = *((byte*)src + 2) / 255f;
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int16Normalized(float3* destination, void* src)
        {
            destination->x = -math.max(*(short*)src / (float)short.MaxValue, -1f);
            destination->y = math.max(*((short*)src + 1) / (float)short.MaxValue, -1f);
            destination->z = math.max(*((short*)src + 2) / (float)short.MaxValue, -1f);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt16Normalized(float3* destination, void* src)
        {
            destination->x = -*(ushort*)src / (float)ushort.MaxValue;
            destination->y = *((ushort*)src + 1) / (float)ushort.MaxValue;
            destination->z = *((ushort*)src + 2) / (float)ushort.MaxValue;
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt32Normalized(float3* destination, void* src)
        {
            destination->x = -*(uint*)src / (float)uint.MaxValue;
            destination->y = *((uint*)src + 1) / (float)uint.MaxValue;
            destination->z = *((uint*)src + 2) / (float)uint.MaxValue;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsOnLoad()
        {
            // Reset cached function pointers
            s_GetIndexValueInt8Method = default;
            s_GetIndexValueUInt8Method = default;
            s_GetIndexValueInt16Method = default;
            s_GetIndexValueUInt16Method = default;
            s_GetIndexValueUInt32Method = default;
            s_GetFloat3FloatMethod = default;
            s_GetFloat3Int8Method = default;
            s_GetFloat3UInt8Method = default;
            s_GetFloat3Int16Method = default;
            s_GetFloat3UInt16Method = default;
            s_GetFloat3UInt32Method = default;
            s_GetFloat3Int8NormalizedMethod = default;
            s_GetFloat3UInt8NormalizedMethod = default;
            s_GetFloat3Int16NormalizedMethod = default;
            s_GetFloat3UInt16NormalizedMethod = default;
            s_GetFloat3UInt32NormalizedMethod = default;
        }
#endif
    }
}
