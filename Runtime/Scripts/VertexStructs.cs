using System.Runtime.InteropServices;

namespace GLTFast.Vertex
{
    using Unity.Mathematics;

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNormTan {
        public float3 pos;
        public float3 nrm;
        public float3 tan;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNorm {
        public float3 pos;
        public float3 nrm;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPos {
        public float3 pos;
    }
}
