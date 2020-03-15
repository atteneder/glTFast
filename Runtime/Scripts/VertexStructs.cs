using System.Runtime.InteropServices;

namespace GLTFast.Vertex
{
#if BURST
    using Unity.Mathematics;
#else
    using UnityEngine;
#endif

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNormTan {
#if BURST
        public float3 pos;
        public float3 nrm;
        public float3 tan;
#else
        public Vector3 pos;
        public Vector3 nrm;
        public Vector4 tan;
#endif
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNorm {
#if BURST
        public float3 pos;
        public float3 nrm;
#else
        public Vector3 pos;
        public Vector3 nrm;
#endif
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPos {
#if BURST
        public float3 pos;
#else
        public Vector3 pos;
#endif
    }
}
