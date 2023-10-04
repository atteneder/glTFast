// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if DRACO_UNITY

using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Draco;
using UnityEngine.Rendering;

namespace GLTFast {

    class PrimitiveDracoCreateContext : PrimitiveCreateContextBase {

        DracoMeshLoader m_Draco;
        Task<Mesh> m_DracoTask;
        Bounds? m_Bounds;

        bool m_NeedsNormals;
        bool m_NeedsTangents;

        public override bool IsCompleted => m_DracoTask!=null && m_DracoTask.IsCompleted;

        public PrimitiveDracoCreateContext(
            int meshIndex,
            int primitiveIndex,
            int subMeshCount,
            bool needsNormals,
            bool needsTangents,
            string meshName,
            Bounds? bounds
            )
            : base(meshIndex, primitiveIndex, subMeshCount, meshName)
        {
            m_NeedsNormals = needsNormals;
            m_NeedsTangents = needsTangents;
            m_Bounds = bounds;
        }

        public void StartDecode(NativeSlice<byte> data, int weightsAttributeId, int jointsAttributeId) {
            m_Draco = new DracoMeshLoader();
            m_DracoTask = m_Draco.ConvertDracoMeshToUnity(
                data,
                m_NeedsNormals,
                m_NeedsTangents,
                weightsAttributeId,
                jointsAttributeId,
                morphTargetsContext!=null
                );
        }

        public override async Task<MeshResult?> CreatePrimitive() {

            var mesh = m_DracoTask.Result;
            m_DracoTask.Dispose();

            if (mesh == null) {
                return null;
            }

            mesh.name = m_MeshName;

            if (m_Bounds.HasValue) {
                mesh.bounds = m_Bounds.Value;

                // Setting the sub-meshes' bounds to the overall bounds
                // Calculating the actual sub-mesh bounds (by iterating the verts referenced
                // by the sub-mesh indices) would be slow. Also, hardly any glTFs re-use
                // the same vertex buffer across primitives of a node (which is the
                // only way a mesh can have sub-meshes)
                for (var i = 0; i < mesh.subMeshCount; i++) {
                    var subMeshDescriptor = mesh.GetSubMesh(i);
                    subMeshDescriptor.bounds = m_Bounds.Value;
                    mesh.SetSubMesh(i, subMeshDescriptor, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds );
                }
            } else {
                mesh.RecalculateBounds();
            }

            if (morphTargetsContext != null) {
                await morphTargetsContext.ApplyOnMeshAndDispose(mesh);
            }

#if GLTFAST_KEEP_MESH_DATA
            UnityEngine.Profiling.Profiler.BeginSample("UploadMeshData");
            mesh.UploadMeshData(false);
            UnityEngine.Profiling.Profiler.EndSample();
#else
            // Don't upload explicitly. Unity takes care of upload on demand/deferred

            // Profiler.BeginSample("UploadMeshData");
            // mesh.UploadMeshData(true);
            // Profiler.EndSample();
#endif

            return new MeshResult(
                MeshIndex,
                new []{0}, // With Draco, only single primitive meshes are supported
                m_Materials,
                mesh
                );
        }
    }
}
#endif // DRACO_UNITY
