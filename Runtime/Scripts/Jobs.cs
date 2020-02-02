#if NET_LEGACY || NET_2_0 || NET_2_0_SUBSET
#warning Consider using .NET 4.x equivalent scripting runtime version or upgrading Unity 2019.1 or newer for better performance
#define COPY_LEGACY
#endif

using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace GLTFast.Jobs {

    static class Constants {
        public const float UINT16_MAX = 65535f;
        public const float INT16_MAX = 32767f;
    }

    public unsafe struct CreateIndicesJob : IJob  {
            
        [ReadOnly]
        public int count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        [ReadOnly]
        public bool lineLoop;

        public void Execute()
        {
            for (var i = 0; i < count; i++) {
                result[i] = i;
            }
            if(lineLoop) {
                result[count-1] = 0;
            }
        }
    }

    public unsafe struct CreateIndicesFlippedJob : IJob  {
            
        [ReadOnly]
        public int count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute()
        {
            for (var i = 0; i < count; i++) {
                result[i] = i - 2*(i%3-1);
            }
        }
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
            int triCount = count/3;
            for (var i = 0; i < triCount; i++) {
                result[i*3] = input[i*3];
                result[i*3+2] = input[i*3+1];
                result[i*3+1] = input[i*3+2];
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
            int triCount = count/3;
            for (var i = 0; i < triCount; i++) {
                result[i*3] = input[i*3];
                result[i*3+2] = input[i*3+1];
                result[i*3+1] = input[i*3+2];
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
            int triCount = count/3;
            for (var i = 0; i < triCount; i++) {
                result[i*3] = (int)input[i*3];
                result[i*3+2] = (int)input[i*3+1];
                result[i*3+1] = (int)input[i*3+2];
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

        [ReadOnly]
        public bool normalize;

        public void Execute()
        {
            if(normalize) {
                for (var i = 0; i < count; i++)
                {
                    result[i].x = input[i*2] / 255f;
                    result[i].y = 1 - input[i*2+1] / 255f;
                }
            } else {
                for (var i = 0; i < count; i++)
                {
                    result[i].x = input[i*2];
                    result[i].y = 1 - input[i*2+1];
                }
            }
        }
    }

    public unsafe struct GetUVsUInt16NormalizedJob : IJob  {

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
                result[i].y = 1 - input[i*2+1] / Constants.UINT16_MAX;
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
                result[i].x = input[i*2];
                result[i].y = 1 - input[i*2+1];
            }
        }
    }

    public unsafe struct GetUVsFloatJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute() {
            for (int i = 0; i < count; i++) {
                result[i].x = ((float*)input)[i*2];
                result[i].y = 1-((float*)input)[i*2+1];
            }
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

        [ReadOnly]
        public bool normalize;

        public void Execute()
        {
            byte* off = input;
            if(normalize) {
                for (var i = 0; i < count; i++)
                {
                    result[i].x = *off / 255f;
                    result[i].y = 1 - *(off+1) / 255f;
                    off += byteStride;
                }
            } else {
                for (var i = 0; i < count; i++)
                {
                    result[i].x = *off;
                    result[i].y = 1 - *(off+1);
                    off += byteStride;
                }
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

        [ReadOnly]
        public bool normalize;

        public void Execute()
        {
            byte* off = input;
            if(normalize) {
                for (var i = 0; i < count; i++)
                {
                    System.UInt16* uv = (System.UInt16*) off;
                    result[i].x = *uv / Constants.UINT16_MAX;
                    result[i].y = 1 - *(uv+1) / Constants.UINT16_MAX;
                    off += byteStride;
                }
            } else {
                for (var i = 0; i < count; i++)
                {
                    System.UInt16* uv = (System.UInt16*) off;
                    result[i].x = *uv;
                    result[i].y = 1 - *(uv+1);
                    off += byteStride;
                }
            }
        }
    }

    /// Untested!
    public unsafe struct GetUVsInt16InterleavedJob : IJob  {

        [ReadOnly]
        public int count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.Int16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        [ReadOnly]
        public bool normalize;

        public void Execute()
        {
            byte* off = (byte*) input;
            if(normalize) {
                for (var i = 0; i < count; i++)
                {
                    System.Int16* uv = (System.Int16*) off;
                    result[i].x = Mathf.Max( *uv / Constants.INT16_MAX, -1.0f);
                    result[i].y = 1 - Mathf.Max( *(uv+1) / Constants.INT16_MAX, -1.0f);
                    off += byteStride;
                }
            } else {
                for (var i = 0; i < count; i++)
                {
                    System.Int16* uv = (System.Int16*) off;
                    result[i].x = *uv;
                    result[i].y = 1 - *(uv+1);
                    off += byteStride;
                }
            }
        }
    }

    /// Untested!
    public unsafe struct GetUVsInt8InterleavedJob : IJob  {

        [ReadOnly]
        public int count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        [ReadOnly]
        public bool normalize;

        public void Execute()
        {
            sbyte* off = input;
            if(normalize) {
                for (var i = 0; i < count; i++)
                {
                    result[i].x = Mathf.Max( *off / 127f, -1.0f);
                    result[i].y = 1 - Mathf.Max( *(off+1) / 127f, -1.0f);
                    off += byteStride;
                }
            } else {
                for (var i = 0; i < count; i++)
                {
                    result[i].x = *off;
                    result[i].y = 1 - *(off+1);
                    off += byteStride;
                }
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
#if COPY_LEGACY
            for (int i = 0; i < bufferSize; i++)
            {
                ((byte*)result)[i] = ((byte*)input)[i];
            }
#else
            System.Buffer.MemoryCopy(
                input,
                result,
                bufferSize,
                bufferSize
            );
#endif
        }
    }

    public unsafe struct GetVector3sJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* result;

        public void Execute() {
#if COPY_LEGACY
            for (int i = 0; i < count; i++)
            {
                ((Vector3*)result)[i] = ((Vector3*)input)[i];
                result[i*3+2] *= -1;
            }
#else
            System.Buffer.MemoryCopy(
                input,
                result,
                count*12,
                count*12
            );
            for (int i = 0; i < count; i++)
            {
                result[i*3+2] = -input[i*3+2];
            }
#endif
        }
    }

    public unsafe struct GetVector4sJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* result;

        public void Execute() {
            for (int i = 0; i < count; i++)
            {
                result[i*4] = -input[i*4];
                result[i*4+1] = input[i*4+1];
                result[i*4+2] = -input[i*4+2];
                result[i*4+3] = input[i*4+3];
            }
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
                *resultV = *(Vector2*)off;
                (*resultV).y = 1-(*resultV).y;
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
                *(Vector2*)resultV = *(Vector2*)off;
                *(((float*)resultV)+2) = -*(((float*)off)+2);
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
                *resultV = *(Vector4*)off;
                (*resultV).x *= -1;
                (*resultV).z *= -1;
                off += byteStride;
                resultV += 1;
            }
        }
    }

    /// Untested!
    public unsafe struct GetVector4sInt16NormalizedInterleavedJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.Int16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector4* result;

        public void Execute() {
            Vector4* resultV = result;
            byte* off = (byte*)input;
            for (int i = 0; i < count; i++)
            {
                Vector4 tmp;
                tmp.x = -Mathf.Max( *(System.Int16*)off / Constants.INT16_MAX, -1f );
                tmp.y = Mathf.Max( *( ((System.Int16*)off) +1 ) / Constants.INT16_MAX, -1f );
                tmp.z = -Mathf.Max( *( ((System.Int16*)off) +2 ) / Constants.INT16_MAX, -1f );
                tmp.w = Mathf.Max( *( ((System.Int16*)off) +3 ) / Constants.INT16_MAX, -1f );
                tmp.Normalize();
                *resultV = tmp;
                off += byteStride;
                resultV += 1;
            }
        }
    }

    /// Untested!
    public unsafe struct GetVector4sInt8NormalizedInterleavedJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector4* result;

        public void Execute() {
            Vector4* resultV = result;
            byte* off = (byte*)input;
            for (int i = 0; i < count; i++)
            {
                Vector4 tmp;
                tmp.x = -Mathf.Max( *(sbyte*)off / 127f, -1f );
                tmp.y = Mathf.Max( *( ((sbyte*)off) +1 ) / 127f, -1f );
                tmp.z = -Mathf.Max( *( ((sbyte*)off) +2 ) / 127f, -1f );
                tmp.w = Mathf.Max( *( ((sbyte*)off) +3 ) / 127f, -1f );
                tmp.Normalize();
                *resultV = tmp;
                off += byteStride;
                resultV += 1;
            }
        }
    }

    public unsafe struct GetUInt16PositionsJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        [ReadOnly]
        public bool normalize;

        public void Execute() {
            // TODO: evaluate if the new mesh API supports uint16 positions and remove this Job
            if(normalize) {
                for (int i = 0; i < count; i++)
                {
                    Vector3 tmp;
                    tmp.x = input[i*3] / Constants.UINT16_MAX;
                    tmp.y = input[i*3+1] / Constants.UINT16_MAX;
                    tmp.z = -input[i*3+2] / Constants.UINT16_MAX;
                    tmp.Normalize();
                    result[i] = tmp;
                }
            } else {
                float* resFloat = (float*) result;
                for (int i = 0; i < count; i++)
                {
                    resFloat[i*3] = input[i*3];
                    resFloat[i*3+1] = input[i*3+1];
                    resFloat[i*3+2] = -input[i*3+2];
                }
            }
        }
    }

    public unsafe struct GetUInt16PositionsInterleavedJob : IJob {

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

        [ReadOnly]
        public bool normalize;

        public void Execute() {
            Vector3* resultV = result;
            byte* off = input;
            if(normalize) {
                for (int i = 0; i < count; i++)
                { 
                    Vector3 tmp;
                    tmp.x = *(((System.UInt16*)off)) / Constants.UINT16_MAX;
                    tmp.y = *(((System.UInt16*)off)+1) / Constants.UINT16_MAX;
                    tmp.z = -*(((System.UInt16*)off)+2) / Constants.UINT16_MAX;
                    tmp.Normalize();
                    *resultV = tmp;
                    off += byteStride;
                    resultV += 1;
                }
            } else {
                for (int i = 0; i < count; i++)
                {
                    *(((float*)resultV)) = *(((System.UInt16*)off));
                    *(((float*)resultV)+1) = *(((System.UInt16*)off)+1);
                    *(((float*)resultV)+2) = -*(((System.UInt16*)off)+2);
                    off += byteStride;
                    resultV += 1;
                }
            }
        }
    }

    public unsafe struct GetVector3FromInt16InterleavedJob : IJob {

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

        [ReadOnly]
        public bool normalize;

        public void Execute() {
            Vector3* resultV = result;
            byte* off = input;
            if(normalize) {
                for (int i = 0; i < count; i++)
                { 
                    Vector3 tmp;
                    tmp.x = Mathf.Max( *(((System.Int16*)off)) / Constants.INT16_MAX, -1.0f);
                    tmp.y = Mathf.Max( *(((System.Int16*)off)+1) / Constants.INT16_MAX, -1.0f);
                    tmp.z = -Mathf.Max( *(((System.Int16*)off)+2) / Constants.INT16_MAX, -1.0f);
                    tmp.Normalize();
                    *resultV = tmp;
                    off += byteStride;
                    resultV += 1;
                }
            } else {
                for (int i = 0; i < count; i++)
                {
                    *(((float*)resultV)) = *(((System.Int16*)off));
                    *(((float*)resultV)+1) = *(((System.Int16*)off)+1);
                    *(((float*)resultV)+2) = -*(((System.Int16*)off)+2);
                    off += byteStride;
                    resultV += 1;
                }
            }
        }
    }

    public unsafe struct GetVector3FromSByteInterleavedJob : IJob {

        [ReadOnly]
        public long count;

        [ReadOnly]
        public int byteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        [ReadOnly]
        public bool normalize;

        public unsafe void Setup(int count,int byteStride, sbyte* src, Vector3* dst, bool normalize = false) {
            this.count = count;
            this.byteStride = byteStride;
            this.input = src;
            this.result = dst;
            this.normalize = normalize;
        }

        public void Execute() {
            Vector3* resultV = result;
            sbyte* off = input;

            if(normalize) {
                for (int i = 0; i < count; i++)
                {
                    Vector3 tmp;
                    tmp.x = Mathf.Max(-1,*off/127f);
                    tmp.y = Mathf.Max(-1,*(off+1)/127f);
                    tmp.z = -Mathf.Max(-1,*(off+2)/127f);
                    tmp.Normalize();
                    *resultV = tmp;
                    off += byteStride;
                    resultV += 1;
                }
            } else {
                for (int i = 0; i < count; i++)
                {
                    *(((float*)resultV)) = *(((sbyte*)off));
                    *(((float*)resultV)+1) = *(((sbyte*)off)+1);
                    *(((float*)resultV)+2) = -*(((sbyte*)off)+2);
                    off += byteStride;
                    resultV += 1;
                }
            }
        }
    }

    public unsafe struct GetVector3FromByteInterleavedJob : IJob {

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

        [ReadOnly]
        public bool normalize;

        public unsafe void Setup(int count,int byteStride, byte* src, Vector3* dst, bool normalize = false) {
            this.count = count;
            this.byteStride = byteStride;
            this.input = src;
            this.result = dst;
            this.normalize = normalize;
        }

        public void Execute() {
            Vector3* resultV = result;
            byte* off = input;

            if(normalize) {
                for (int i = 0; i < count; i++)
                {
                    Vector3 tmp;
                    tmp.x = Mathf.Max(-1,*off/255f);
                    tmp.y = Mathf.Max(-1,*(off+1)/255f);
                    tmp.z = -Mathf.Max(-1,*(off+2)/255f);
                    tmp.Normalize();
                    *resultV = tmp;
                    off += byteStride;
                    resultV += 1;
                }
            } else {
                for (int i = 0; i < count; i++)
                {
                    *(((float*)resultV)) = *(((byte*)off));
                    *(((float*)resultV)+1) = *(((byte*)off)+1);
                    *(((float*)resultV)+2) = -*(((byte*)off)+2);
                    off += byteStride;
                    resultV += 1;
                }
            }
        }
    }
}
