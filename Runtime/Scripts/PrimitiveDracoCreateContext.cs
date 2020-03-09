#if DRACO_UNITY

using UnityEngine;
using UnityEngine.Profiling;
using Unity.Jobs;
using Unity.Collections;
using IntPtr = System.IntPtr;

namespace GLTFast {

    using Schema;

    class PrimitiveDracoCreateContext : PrimitiveCreateContextBase {
        public JobHandle jobHandle;
        public NativeArray<int> dracoResult;
        public NativeArray<IntPtr> dracoPtr;

        public override bool IsCompleted {
            get {
                return jobHandle.IsCompleted;
            }  
        }

        public override Primitive? CreatePrimitive() {
            jobHandle.Complete();
            int result = dracoResult[0];
            IntPtr dracoMesh = dracoPtr[0];

            dracoResult.Dispose();
            dracoPtr.Dispose();

            if (result <= 0) {
                Debug.LogError ("Failed: Decoding error.");
                return null;
            }

            Profiler.BeginSample("DracoMeshLoader.CreateMesh");
            bool hasTexcoords;
            bool hasNormals;
            var mesh = DracoMeshLoader.CreateMesh(dracoMesh, out hasNormals, out hasTexcoords);
            Profiler.EndSample();

            if(needsNormals && !hasNormals) {
                Profiler.BeginSample("Draco.RecalculateNormals");
                // TODO: Make optional. Only calculate if actually needed
                mesh.RecalculateNormals();
                Profiler.EndSample();
            }
            if(needsTangents && hasTexcoords) {
                Profiler.BeginSample("Draco.RecalculateTangents");
                // TODO: Make optional. Only calculate if actually needed
                mesh.RecalculateTangents();
                Profiler.EndSample();
            }

            return new Primitive(mesh,materials);
        }
    }
}
#endif // DRACO_UNITY
