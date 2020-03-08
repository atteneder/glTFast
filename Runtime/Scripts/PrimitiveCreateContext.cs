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

using UnityEngine;
#if !UNITY_2019_3_OR_NEWER
#define LEGACY_MESH
#endif

using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

namespace GLTFast {

    using Schema;

    class PrimitiveCreateContext : PrimitiveCreateContextBase {

        public Mesh mesh;

        /// TODO remove begin
        public Vector3[] positions;
        public Vector3[] normals;
        public Vector2[] uvs0;
#if LEGACY_MESH
        public Vector2[] uvs1;
#endif
        public Vector4[] tangents;
        public Color32[] colors32;
        public Color[] colors;
        /// TODO remove end

        public JobHandle jobHandle;
        public int[][] indices;

        public GCHandle calculatedIndicesHandle;

        public MeshTopology topology;

        public override bool IsCompleted {
            get {
                return jobHandle.IsCompleted;
            }  
        }

        public override Primitive? CreatePrimitive() {
            Profiler.BeginSample("CreatePrimitive");
            Profiler.BeginSample("Job Complete");
            jobHandle.Complete();
            Profiler.EndSample();
            var msh = new UnityEngine.Mesh();
            msh.name = mesh.name;

            bool calculateNormals = needsNormals && ( topology==MeshTopology.Triangles || topology==MeshTopology.Quads );
            bool calculateTangents = needsTangents && uvs0!=null && (topology==MeshTopology.Triangles || topology==MeshTopology.Quads);

#if LEGACY_MESH
            if( positions.Length > 65536 ) {
#if UNITY_2017_3_OR_NEWER
                msh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                throw new System.Exception("Meshes with more than 65536 vertices are only supported from Unity 2017.3 onwards.");
#endif // UNITY_2017_3_OR_NEWER
            }

            Profiler.BeginSample("SetVertices");
            msh.vertices = positions;
            Profiler.EndSample();

            Profiler.BeginSample("SetIndices");
            msh.subMeshCount = indices.Length;
            for (int i = 0; i < indices.Length; i++) {
                msh.SetIndices(indices[i],topology,i);
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetNormals");
            if(normals!=null) {
                msh.normals = normals;
            } else
            if(calculateNormals) {
                Profiler.BeginSample("RecalculateNormals");
                msh.RecalculateNormals();
                Profiler.EndSample();
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetTangents");
            if(tangents!=null) {
                msh.tangents = tangents;
            } else
            if(calculateTangents) {
                Profiler.BeginSample("RecalculateTangents");
                msh.RecalculateTangents();
                Profiler.EndSample();
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetUVs");
            if(uvs0!=null) {
                msh.uv = uvs0;
            }
            if(uvs1!=null) {
                msh.uv2 = uvs1;
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetColor");
            if (colors!=null) {
                msh.colors = colors;
            } else if(colors32!=null) {
                msh.colors32 = colors32;
            }
            Profiler.EndSample();

            Profiler.BeginSample("UploadMeshData");
            msh.UploadMeshData(true);
            Profiler.EndSample();

#else // #if LEGACY_MESH

            Profiler.BeginSample("PrepareMesh");
            MeshUpdateFlags flags = (MeshUpdateFlags)~0;
            int vadLen = 1;
            if(uvs0!=null) vadLen++;
            // if(uvs1!=null) vadLen++;
            if(normals!=null || calculateNormals)
                vadLen++;
            if(tangents!=null || calculateTangents)
                vadLen++;

            var vad = new VertexAttributeDescriptor[vadLen];
            var vadCount = 0;
            vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, vadCount);
            vadCount++;
            if(uvs0!=null) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, vadCount);
                vadCount++;
            }
            // if(uvs1!=null) {
            //     vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, vadCount);
            //     vadCount++;
            // }
            if(normals!=null || calculateNormals) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, vadCount);
                vadCount++;
            }
            if(tangents!=null || calculateTangents) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, vadCount);
                vadCount++;
            }

            msh.SetVertexBufferParams(positions.Length,vad);
            Profiler.EndSample();

            Profiler.BeginSample("SetVertices");
            vadCount = 0;
            msh.SetVertexBufferData(positions,0,0,positions.Length,vadCount,flags);
            vadCount++;
            Profiler.EndSample();

            if(uvs0!=null) {
                Profiler.BeginSample("SetUVs0");
                msh.SetVertexBufferData(uvs0,0,0,uvs0.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }
            // if(uvs1!=null) {
            //     Profiler.BeginSample("SetUVs1");
            //     msh.SetVertexBufferData(uvs1,0,0,uvs1.Length,vadCount,flags);
            //     vadCount++;
            //     Profiler.EndSample();
            // }
            if(normals!=null) {
                Profiler.BeginSample("SetNormals");
                msh.SetVertexBufferData(normals,0,0,normals.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }

            if(tangents!=null) {
                Profiler.BeginSample("SetTangents");
                msh.SetVertexBufferData(tangents,0,0,tangents.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }

            Profiler.BeginSample("SetIndices");
            int indexCount = 0;
            for (int i = 0; i < indices.Length; i++) {
                indexCount += indices[i].Length;
            }
            msh.SetIndexBufferParams(indexCount,IndexFormat.UInt32); //TODO: UInt16 maybe?
            msh.subMeshCount = indices.Length;
            indexCount = 0;
            for (int i = 0; i < indices.Length; i++) {
                Profiler.BeginSample("SetIndexBufferData");
                msh.SetIndexBufferData(indices[i],0,indexCount,indices[i].Length,flags);
                Profiler.EndSample();
                Profiler.BeginSample("SetSubMesh");
                msh.SetSubMesh(i,new SubMeshDescriptor(indexCount,indices[i].Length,topology),flags);
                Profiler.EndSample();
                indexCount += indices[i].Length;
            }
            Profiler.EndSample();

            if(normals==null && calculateNormals) {
                Profiler.BeginSample("RecalculateNormals");
                msh.RecalculateNormals();
                Profiler.EndSample();
            }
            if(tangents==null && calculateTangents) {
                Profiler.BeginSample("RecalculateTangents");
                msh.RecalculateTangents();
                Profiler.EndSample();
            }

            Profiler.BeginSample("RecalculateBounds");
            msh.RecalculateBounds(); // TODO: make optional! maybe calculate bounds in Job.
            Profiler.EndSample();

            Profiler.BeginSample("UploadMeshData");
            msh.UploadMeshData(true);
            Profiler.EndSample();

#endif // LEGACY_MESH

            Profiler.BeginSample("Dispose");
            Dispose();
            Profiler.EndSample();
            Profiler.EndSample();
            return new Primitive(msh,materials);
        }

        void Dispose() {
            if(calculatedIndicesHandle.IsAllocated) {
                calculatedIndicesHandle.Free();
            }
        }
    }
}
