// Copyright 2020 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

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

    public unsafe struct CreateIndicesJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        [ReadOnly]
        public bool lineLoop;

        public void Execute(int i)
        {
            result[i] = i;
        }
    }

    public unsafe struct CreateIndicesFlippedJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = i - 2*(i%3-1);
        }
    }


    public unsafe struct GetIndicesUInt8Job : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = input[i];
        }
    }

    public unsafe struct GetIndicesUInt8FlippedJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i*3] = input[i*3];
            result[i*3+2] = input[i*3+1];
            result[i*3+1] = input[i*3+2];
        }
    }

    public unsafe struct GetIndicesUInt16FlippedJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i*3] = input[i*3];
            result[i*3+2] = input[i*3+1];
            result[i*3+1] = input[i*3+2];
        }
    }

    public unsafe struct GetIndicesUInt16Job : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = input[i];
        }
    }

    public unsafe struct GetIndicesUInt32Job : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt32* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = (int)input[i];
        }
    }

    public unsafe struct GetIndicesUInt32FlippedJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt32* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i*3] = (int)input[i*3];
            result[i*3+2] = (int)input[i*3+1];
            result[i*3+1] = (int)input[i*3+2];
        }
    }

    public unsafe struct GetUVsUInt8Job : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            result[i].x = input[i*2];
            result[i].y = 1 - input[i*2+1];
        }
    }

    public unsafe struct GetUVsUInt8NormalizedJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            result[i].x = input[i*2] / 255f;
            result[i].y = 1 - input[i*2+1] / 255f;
        }
    }

    public unsafe struct GetUVsUInt16NormalizedJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            result[i].x = input[i*2] / Constants.UINT16_MAX;
            result[i].y = 1 - input[i*2+1] / Constants.UINT16_MAX;
        }
    }

    public unsafe struct GetUVsUInt16Job : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            result[i].x = input[i*2];
            result[i].y = 1 - input[i*2+1];
        }
    }

    public unsafe struct GetUVsFloatJob : IJobParallelFor {
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i) {
            result[i].x = ((float*)input)[i*2];
            result[i].y = 1-((float*)input)[i*2+1];
        }
    }

    /// Untested!
    public unsafe struct GetUVsUInt8InterleavedJob : IJobParallelFor  {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + inputByteStride*i;
            *resultV = *off;
            *(resultV+1) = 1 - *(off+1);
        }
    }

    /// Untested!
    public unsafe struct GetUVsUInt8InterleavedNormalizedJob : IJobParallelFor  {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + inputByteStride*i;
            *resultV = *off / 255f;
            *(resultV+1) = 1 - *(off+1) / 255f;
        }
    }

    /// Untested!
    public unsafe struct GetUVsUInt16InterleavedJob : IJobParallelFor  {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            System.UInt16* uv = (System.UInt16*) (input + inputByteStride*i);
            *resultV = *uv;
            *(resultV+1) = 1 - *(uv+1);
        }
    }

    /// Untested!
    public unsafe struct GetUVsUInt16InterleavedNormalizedJob : IJobParallelFor  {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            System.UInt16* uv = (System.UInt16*) (input + inputByteStride*i);
            *resultV = *uv / Constants.UINT16_MAX;
            *(resultV+1) = 1 - *(uv+1) / Constants.UINT16_MAX;
        }
    }

    /// Untested!
    public unsafe struct GetUVsInt16InterleavedJob : IJobParallelFor  {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.Int16* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            System.Int16* uv = (System.Int16*) (input + inputByteStride*i);
            *resultV = *uv;
            *(resultV+1) = 1 - *(uv+1);
        }
    }

    /// Untested!
    public unsafe struct GetUVsInt16InterleavedNormalizedJob : IJobParallelFor  {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.Int16* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            System.Int16* uv = (System.Int16*) (input + inputByteStride*i);
            *resultV = Mathf.Max( *uv / Constants.INT16_MAX, -1.0f);
            *(resultV) = 1 - Mathf.Max( *(uv+1) / Constants.INT16_MAX, -1.0f);
        }
    }

    /// Untested!
    public unsafe struct GetUVsInt8InterleavedJob : IJobParallelFor  {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            sbyte* off = input + inputByteStride*i;
            *resultV = *off;
            *(resultV) = 1 - *(off+1);
        }
    }

    /// Untested!
    public unsafe struct GetUVsInt8InterleavedNormalizedJob : IJobParallelFor  {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i)
        {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            sbyte* off = input + inputByteStride*i;
            *resultV = Mathf.Max( *off / 127f, -1.0f);
            *(resultV) = 1 - Mathf.Max( *(off+1) / 127f, -1.0f);
        }
    }

    public unsafe struct GetColorsVec3FloatJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Color* result;

        public void Execute(int i) {
            result[i].r = input[i * 3];
            result[i].g = input[i * 3 + 1];
            result[i].b = input[i * 3 + 2];
            result[i].a = 1.0f;
        }
    }

    public unsafe struct GetColorsVec3UInt8Job : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Color32* result;

        public void Execute(int i) {
            result[i].r = input[i * 3];
            result[i].g = input[i * 3 + 1];
            result[i].b = input[i * 3 + 2];
            result[i].a = 255;
        }
    }

    public unsafe struct GetColorsVec3UInt16Job : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Color* result;

        public void Execute(int i) {
            result[i].r = input[i * 3] / Constants.UINT16_MAX;
            result[i].g = input[i * 3 + 1] / Constants.UINT16_MAX;
            result[i].b = input[i * 3 + 2] / Constants.UINT16_MAX;
            result[i].a = 1.0f;
        }
    }

    public unsafe struct GetColorsVec4UInt16Job : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Color* result;

        public void Execute(int i) {
            result[i].r = input[i * 4] / Constants.UINT16_MAX;
            result[i].g = input[i * 4 + 1] / Constants.UINT16_MAX;
            result[i].b = input[i * 4 + 2] / Constants.UINT16_MAX;
            result[i].a = input[i * 4 + 3] / Constants.UINT16_MAX;
        }
    }

#if !COPY_LEGACY
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

#else

    public unsafe struct MemCopyLegacyJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* result;

        public void Execute(int i) {
            result[i] = input[i];
        }
    }
#endif

    public unsafe struct GetVector3sJob : IJobParallelFor {
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* result;

        public void Execute(int i) {
            int ti = i*3;
            result[ti] = input[ti];
            result[ti+1] = input[ti+1];
            result[ti+2] = -input[ti+2];
        }
    }

    public unsafe struct GetVector4sJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* result;

        public void Execute(int i) {
            result[i*4] = -input[i*4];
            result[i*4+1] = input[i*4+1];
            result[i*4+2] = -input[i*4+2];
            result[i*4+3] = input[i*4+3];
        }
    }

    /// Untested!
    public unsafe struct GetVector2sInterleavedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector2* result;

        public void Execute(int i) {
            Vector2* resultV = (Vector2*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + (i*inputByteStride);
            *resultV = *(Vector2*)off;
            (*resultV).y = 1-(*resultV).y;
        }
    }

    public unsafe struct GetVector3sInterleavedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;
        
        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public void Execute(int i) {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + i*inputByteStride;
            *((Vector2*)resultV) = *((Vector2*)off);
            *(resultV+2) = -*(((float*)off)+2);
        }
    }

    /// Untested!
    public unsafe struct GetVector4sInterleavedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector4* result;

        public void Execute(int i) {
            Vector4* resultV = (Vector4*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + (i*inputByteStride);
            *resultV = *((Vector4*)off);
            (*resultV).x *= -1;
            (*resultV).z *= -1;
        }
    }

    /// Untested!
    public unsafe struct GetVector4sInt16NormalizedInterleavedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.Int16* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector4* result;

        public void Execute(int i) {
            Vector4* resultV = (Vector4*) (((byte*)result) + (i*outputByteStride));
            System.Int16* off = (System.Int16*) (((byte*)input) + (i*inputByteStride));

            Vector4 tmp;
            tmp.x = -Mathf.Max( *off / Constants.INT16_MAX, -1f );
            tmp.y = Mathf.Max( *(off+1) / Constants.INT16_MAX, -1f );
            tmp.z = -Mathf.Max( *(off+2) / Constants.INT16_MAX, -1f );
            tmp.w = Mathf.Max( *(off+3) / Constants.INT16_MAX, -1f );
            tmp.Normalize();
            *resultV = tmp;
        }
    }

    /// Untested!
    public unsafe struct GetVector4sInt8NormalizedInterleavedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector4* result;

        public void Execute(int i) {
            Vector4* resultV = (Vector4*) (((byte*)result) + (i*outputByteStride));
            sbyte* off = input + (i*inputByteStride);

            Vector4 tmp;
            tmp.x = -Mathf.Max( *off / 127f, -1f );
            tmp.y = Mathf.Max( *(off+1) / 127f, -1f );
            tmp.z = -Mathf.Max( *(off+2) / 127f, -1f );
            tmp.w = Mathf.Max( *(off+3) / 127f, -1f );
            tmp.Normalize();
            *resultV = tmp;
        }
    }

    public unsafe struct GetUInt16PositionsJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public void Execute(int i) {
            // TODO: evaluate if the new mesh API supports uint16 positions and remove this Job
            result[i].x = input[i*3];
            result[i].y = input[i*3+1];
            result[i].z = -input[i*3+2];
        }
    }

    public unsafe struct GetUInt16PositionsNormalizedJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public System.UInt16* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public void Execute(int i) {
            // TODO: evaluate if the new mesh API supports uint16 positions and remove this Job
            Vector3 tmp;
            tmp.x = input[i*3] / Constants.UINT16_MAX;
            tmp.y = input[i*3+1] / Constants.UINT16_MAX;
            tmp.z = -input[i*3+2] / Constants.UINT16_MAX;
            tmp.Normalize();
            result[i] = tmp;
        }
    }

    public unsafe struct GetUInt16PositionsInterleavedJob : IJobParallelFor
    {
        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;
        
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public void Execute(int i) {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + (inputByteStride*i);
            *resultV = *((System.UInt16*)off);
            *(resultV+1) = *(((System.UInt16*)off)+1);
            *(resultV+2) = -*(((System.UInt16*)off)+2);
        }
    }

    public unsafe struct GetUInt16PositionsInterleavedNormalizedJob : IJobParallelFor
    {
        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;
        
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public void Execute(int i) {
            Vector3* resultV = (Vector3*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + (inputByteStride*i);
            Vector3 tmp;
            tmp.x = *(((System.UInt16*)off)) / Constants.UINT16_MAX;
            tmp.y = *(((System.UInt16*)off)+1) / Constants.UINT16_MAX;
            tmp.z = -*(((System.UInt16*)off)+2) / Constants.UINT16_MAX;
            tmp.Normalize();
            *resultV = tmp;
        }
    }

    public unsafe struct GetVector3FromInt16InterleavedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;
        

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        [ReadOnly]
        public int outputByteStride;

        public void Execute(int i) {
            var resultV = (float*)  (((byte*)result) + (i*outputByteStride));
            byte* off = input + (i*inputByteStride);
            *resultV = *(((System.Int16*)off));
            *(resultV+1) = *(((System.Int16*)off)+1);
            *(resultV+2) = -*(((System.Int16*)off)+2);
        }
    }

    public unsafe struct GetVector3FromInt16InterleavedNormalizedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public void Execute(int i) {
            Vector3* resultV = (Vector3*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + (i*inputByteStride);

            Vector3 tmp;
            tmp.x = Mathf.Max( *(((System.Int16*)off)) / Constants.INT16_MAX, -1.0f);
            tmp.y = Mathf.Max( *(((System.Int16*)off)+1) / Constants.INT16_MAX, -1.0f);
            tmp.z = -Mathf.Max( *(((System.Int16*)off)+2) / Constants.INT16_MAX, -1.0f);
            tmp.Normalize();
            *resultV = tmp;
        }
    }

    public unsafe struct GetVector3FromSByteInterleavedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;
        
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public unsafe void Setup(int inputByteStride, sbyte* src, int outputByteStride, Vector3* dst) {
            this.inputByteStride = inputByteStride;
            this.input = src;
            this.outputByteStride = outputByteStride;
            this.result = dst;
        }

        public void Execute(int i) {
            float* resultV = (float*) (((byte*)result) + (i*outputByteStride));
            sbyte* off = input + (inputByteStride*i);

            *resultV = *off;
            *(resultV+1) = *(off+1);
            *(resultV+2) = -*(off+2);
        }
    }

    public unsafe struct GetVector3FromSByteInterleavedNormalizedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public unsafe void Setup(int byteStride, sbyte* src, int outputByteStride, Vector3* dst) {
            this.inputByteStride = byteStride;
            this.input = src;
            this.outputByteStride = outputByteStride;
            this.result = dst;
        }

        public void Execute(int i) {
            Vector3* resultV = (Vector3*) (((byte*)result) + (i*outputByteStride));
            sbyte* off = input + (inputByteStride*i);

            Vector3 tmp;
            tmp.x = Mathf.Max(-1,*off/127f);
            tmp.y = Mathf.Max(-1,*(off+1)/127f);
            tmp.z = -Mathf.Max(-1,*(off+2)/127f);
            tmp.Normalize();
            *resultV = tmp;
        }
    }

    public unsafe struct GetVector3FromByteInterleavedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;
        
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public unsafe void Setup(int byteStride, byte* src, int outputByteStride, Vector3* dst) {
            this.inputByteStride = byteStride;
            this.input = src;
            this.outputByteStride = outputByteStride;
            this.result = dst;
        }

        public void Execute(int i) {
            byte* off = input + (i*inputByteStride);
            var resultV = (Vector3*) (((byte*) result) + (i*outputByteStride));
            resultV[i].x = *off;
            resultV[i].y = *(off+1);
            resultV[i].z = -*(off+2);
        }
    }

    public unsafe struct GetVector3FromByteInterleavedNormalizedJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public Vector3* result;

        public unsafe void Setup(int byteStride, byte* src, int outputByteStride, Vector3* dst) {
            this.inputByteStride = byteStride;
            this.input = src;
            this.outputByteStride = outputByteStride;
            this.result = dst;
        }

        public void Execute(int i) {
            Vector3* resultV = (Vector3*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + (i*inputByteStride);

            Vector3 tmp;
            tmp.x = Mathf.Max(-1,*off/255f);
            tmp.y = Mathf.Max(-1,*(off+1)/255f);
            tmp.z = -Mathf.Max(-1,*(off+2)/255f);
            tmp.Normalize();
            *resultV = tmp;
        }
    }
}
