using UnityEngine;
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

            var msh = DracoMeshLoader.CreateMesh(dracoMesh);

            return new Primitive(msh,primitive.material);
        }
    }
} 