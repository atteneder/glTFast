// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if DRACO_UNITY

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Draco;
using GLTFast.Schema;
using UnityEngine.Rendering;
using Mesh = UnityEngine.Mesh;

namespace GLTFast {

    class PrimitiveDracoCreateContext : PrimitiveCreateContextBase {

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

        public void StartDecode(NativeSlice<byte> data, Attributes dracoAttributes)
        {
            var flags = DecodeSettings.ConvertSpace;
            if (m_NeedsTangents)
            {
                flags |= DecodeSettings.RequireNormalsAndTangents;
            } else
            if (m_NeedsNormals)
            {
                flags |= DecodeSettings.RequireNormals;
            }
            if (morphTargetsContext != null)
            {
                flags |= DecodeSettings.ForceUnityVertexLayout;
            }

            m_DracoTask = DracoDecoder.DecodeMesh(data, flags, GenerateAttributeIdMap(dracoAttributes));
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

        static Dictionary<VertexAttribute, int> GenerateAttributeIdMap(Attributes attributes)
        {
            var result = new Dictionary<VertexAttribute, int>();
            if (attributes.POSITION >= 0)
                result[VertexAttribute.Position] = attributes.POSITION;
            if (attributes.NORMAL >= 0)
                result[VertexAttribute.Normal] = attributes.NORMAL;
            if (attributes.TANGENT >= 0)
                result[VertexAttribute.Tangent] = attributes.TANGENT;
            if (attributes.COLOR_0 >= 0)
                result[VertexAttribute.Color] = attributes.COLOR_0;
            if (attributes.TEXCOORD_0 >= 0)
                result[VertexAttribute.TexCoord0] = attributes.TEXCOORD_0;
            if (attributes.TEXCOORD_1 >= 0)
                result[VertexAttribute.TexCoord1] = attributes.TEXCOORD_1;
            if (attributes.TEXCOORD_2 >= 0)
                result[VertexAttribute.TexCoord2] = attributes.TEXCOORD_2;
            if (attributes.TEXCOORD_3 >= 0)
                result[VertexAttribute.TexCoord3] = attributes.TEXCOORD_3;
            if (attributes.TEXCOORD_4 >= 0)
                result[VertexAttribute.TexCoord4] = attributes.TEXCOORD_4;
            if (attributes.TEXCOORD_5 >= 0)
                result[VertexAttribute.TexCoord5] = attributes.TEXCOORD_5;
            if (attributes.TEXCOORD_6 >= 0)
                result[VertexAttribute.TexCoord6] = attributes.TEXCOORD_6;
            if (attributes.TEXCOORD_7 >= 0)
                result[VertexAttribute.TexCoord7] = attributes.TEXCOORD_7;
            if (attributes.WEIGHTS_0 >= 0)
                result[VertexAttribute.BlendWeight] = attributes.WEIGHTS_0;
            if (attributes.JOINTS_0 >= 0)
                result[VertexAttribute.BlendIndices] = attributes.JOINTS_0;
            return result;
        }
    }
}
#endif // DRACO_UNITY
