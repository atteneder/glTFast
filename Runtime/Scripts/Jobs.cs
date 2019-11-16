#if !GLTFAST_NO_JOB
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace GLTFast.Jobs {

    static class Constants {
        public const float UINT16_MAX = 65535f;
    }
    public unsafe struct GetIndicesUInt8Job : IJob  {
            
        [ReadOnly]
        public int count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute()
        {
            for (var i = 0; i < count; i++)
            {
                result[i] = input[i];
            }
        }
    }

    public unsafe struct GetIndicesUInt16Job : IJob  {
        
        [ReadOnly]
        public int count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute()
        {
            for (var i = 0; i < count; i++)
            {
                result[i] = input[i];
            }
        }
    }

    public unsafe struct GetIndicesUInt32Job : IJob  {

        [ReadOnly]
        public int count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt32* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute()
        {
            for (var i = 0; i < count; i++)
            {
                result[i] = (int) input[i];
            }
        }
    }

    public unsafe struct GetUVsUInt8Job : IJob  {

        [ReadOnly]
        public int count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute()
        {
            for (var i = 0; i < count; i++)
            {
                result[i].x = input[i*2] / 255f;
                result[i].y = input[i*2+1] / 255f;
            }
        }
    }

    public unsafe struct GetUVsUInt16Job : IJob  {

        [ReadOnly]
        public int count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute()
        {
            for (var i = 0; i < count; i++)
            {
                result[i].x = input[i*2] / Constants.UINT16_MAX;
                result[i].y = input[i*2+1] / Constants.UINT16_MAX;
            }
        }
    }

    public unsafe struct GetUVsFloatJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute() {
            System.Buffer.MemoryCopy(
                input,
                result,
                count*8,
                count*8
            );
        }
    }

    /// Untested!
    public unsafe struct GetUVsUInt8InterleavedJob : IJob  {

        [ReadOnly]
        public int count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute()
        {
            byte* off = input;
            for (var i = 0; i < count; i++)
            {
                result[i].x = *off / 255f;
                result[i].y = *(off+1) / 255f;
                off += byteStride;
            }
        }
    }

    /// Untested!
    public unsafe struct GetUVsUInt16InterleavedJob : IJob  {

        [ReadOnly]
        public int count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute()
        {
            byte* off = input;
            for (var i = 0; i < count; i++)
            {
                System.UInt16* uv = (System.UInt16*) off;
                result[i].x = *uv / Constants.UINT16_MAX;
                result[i].y = *(uv+1) / Constants.UINT16_MAX;
                off += byteStride;
            }
        }
    }

    public unsafe struct GetColorsVec3FloatJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Color* result;

        public void Execute() {
            for (var i = 0; i < count; i++)
            {
                result[i].r = input[i * 3];
                result[i].g = input[i * 3 + 1];
                result[i].b = input[i * 3 + 2];
                
                // TODO: try this partial memcopy and compare performance.
                // System.Buffer.MemoryCopy(
                //     input+(i*3),
                //     result+i,
                //     12,
                //     12
                // );

                result[i].a = 1.0f;
            }
        }
    }

    public unsafe struct GetColorsVec3UInt8Job : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Color32* result;

        public void Execute() {
            for (var i = 0; i < count; i++)
            {
                result[i].r = input[i * 3];
                result[i].g = input[i * 3 + 1];
                result[i].b = input[i * 3 + 2];

                // TODO: try this partial memcopy and compare performance.
                // System.Buffer.MemoryCopy(
                //     input+(i*3),
                //     result+i,
                //     3,
                //     3
                // );
                
                result[i].a = 255;
            }
        }
    }

    public unsafe struct GetColorsVec3UInt16Job : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Color* result;

        public void Execute() {
            for (var i = 0; i < count; i++)
            {
                result[i].r = input[i * 3] / Constants.UINT16_MAX;
                result[i].g = input[i * 3 + 1] / Constants.UINT16_MAX;
                result[i].b = input[i * 3 + 2] / Constants.UINT16_MAX;
                result[i].a = 1.0f;
            }
        }
    }

    public unsafe struct GetColorsVec4UInt16Job : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Color* result;

        public void Execute() {
            for (var i = 0; i < count; i++)
            {
                result[i].r = input[i * 4] / Constants.UINT16_MAX;
                result[i].g = input[i * 4 + 1] / Constants.UINT16_MAX;
                result[i].b = input[i * 4 + 2] / Constants.UINT16_MAX;
                result[i].a = input[i * 4 + 3] / Constants.UINT16_MAX;
            }
        }
    }

    public unsafe struct MemCopyJob : IJob {

        [ReadOnly]
        public long bufferSize;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* result;

        public void Execute() {
            System.Buffer.MemoryCopy(
                input,
                result,
                bufferSize,
                bufferSize
            );
        }
    }

    /// Untested!
    public unsafe struct GetVector2sInterleavedJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute() {
            Vector2* resultV = result;
            byte* off = input;
            for (int i = 0; i < count; i++)
            {
                System.Buffer.MemoryCopy(
                    off,
                    resultV,
                    8,
                    8
                );
                off += byteStride;
                resultV += 1;
            }
        }
    }

    public unsafe struct GetVector3sInterleavedJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public void Execute() {
            Vector3* resultV = result;
            byte* off = input;
            for (int i = 0; i < count; i++)
            {
                System.Buffer.MemoryCopy(
                    off,
                    resultV,
                    12,
                    12
                );
                off += byteStride;
                resultV += 1;
            }
        }
    }

    /// Untested!
    public unsafe struct GetVector4sInterleavedJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector4* result;

        public void Execute() {
            Vector4* resultV = result;
            byte* off = input;
            for (int i = 0; i < count; i++)
            {
                System.Buffer.MemoryCopy(
                    off,
                    resultV,
                    16,
                    16
                );
                off += byteStride;
                resultV += 1;
            }
        }
    }
}
#endif