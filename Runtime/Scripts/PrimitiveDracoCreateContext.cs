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

            Profiler.BeginSample("UploadMeshData");
            mesh.UploadMeshData(true);
            Profiler.EndSample();

            return new Primitive(mesh,materials);
        }
    }
}
#endif // DRACO_UNITY
