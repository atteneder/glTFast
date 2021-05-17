// Copyright 2020-2021 Andreas Atteneder
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

using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Draco;

namespace GLTFast {

    class PrimitiveDracoCreateContext : PrimitiveCreateContextBase {

        DracoMeshLoader draco;
        Task<Mesh> dracoTask;

        public override bool IsCompleted => dracoTask!=null && dracoTask.IsCompleted;

        public void StartDecode(NativeSlice<byte> data, int weightsAttributeId, int jointsAttributeId) {
            draco = new DracoMeshLoader();
            dracoTask = draco.ConvertDracoMeshToUnity(data,needsNormals,needsTangents,weightsAttributeId,jointsAttributeId);
        }
        
        public override Primitive? CreatePrimitive() {

            var mesh = dracoTask.Result;
            dracoTask.Dispose();

            if (mesh == null) {
                return null;
            }

#if GLTFAST_KEEP_MESH_DATA
            Profiler.BeginSample("UploadMeshData");
            mesh.UploadMeshData(false);
            Profiler.EndSample();
#else
            /// Don't upload explicitely. Unity takes care of upload on demand/deferred

            // Profiler.BeginSample("UploadMeshData");
            // mesh.UploadMeshData(true);
            // Profiler.EndSample();
#endif

            return new Primitive(mesh,materials);
        }
    }
}
#endif // DRACO_UNITY
