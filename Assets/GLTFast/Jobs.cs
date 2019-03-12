#if !GLTFAST_NO_JOB
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace GLTFast.Jobs {

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
}
#endif