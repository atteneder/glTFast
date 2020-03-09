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
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

namespace GLTFast {

    using Schema;

    class PrimitiveCreateContext : PrimitiveCreateContextBase {

        public Mesh mesh;

#if LEGACY_MESH
        /// TODO remove begin
        public Vector3[] positions;
        public Vector3[] normals;
        public Vector2[] uvs0;
        public Vector2[] uvs1;
        public Vector4[] tangents;
        public Color32[] colors32;
        public Color[] colors;
        /// TODO remove end
#else

        const int MAX_STREAM = 4;

        public NativeArray<Vector3> positions;
        public NativeArray<Vector3> normals;
        public NativeArray<Vector2> uvs0;
        public NativeArray<Vector2> uvs1;
        public NativeArray<Vector4> tangents;
        public NativeArray<Color32> colors32;
        public NativeArray<Color> colors;
#endif

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

#if LEGACY_MESH
            bool calculateTangents = needsTangents && uvs0!=null && (topology==MeshTopology.Triangles || topology==MeshTopology.Quads);
            CreatePrimitiveLegacy(msh,calculateNormals,calculateTangents);
#else // #if LEGACY_MESH
            bool calculateTangents = needsTangents && uvs0.IsCreated && (topology==MeshTopology.Triangles || topology==MeshTopology.Quads);

            int vadLen = 1;
            if(uvs0.IsCreated) vadLen++;
            if(uvs1.IsCreated) vadLen++;
            if(normals.IsCreated || calculateNormals)
                vadLen++;
            if(tangents.IsCreated || calculateTangents)
                vadLen++;
            if(colors32.IsCreated || colors.IsCreated)
                vadLen++;

            if(vadLen>MAX_STREAM) {
                // Fall back to simple API
                CreatePrimitiveLegacy(msh,calculateNormals,calculateTangents);
            } else {
                CreatePrimitiveAdvanced(msh,vadLen,calculateNormals,calculateTangents);
            }

#endif // LEGACY_MESH

            Profiler.BeginSample("Dispose");
            Dispose();
            Profiler.EndSample();
            return new Primitive(msh,materials);
        }

#if !LEGACY_MESH
        void CreatePrimitiveAdvanced(UnityEngine.Mesh msh,int vadLen,bool calculateNormals,bool calculateTangents) {

            Profiler.BeginSample("CreatePrimitiveAdvanced");
            Profiler.BeginSample("SetVertexBufferParams");

            MeshUpdateFlags flags = (MeshUpdateFlags)~0;
            var vad = new VertexAttributeDescriptor[vadLen];
            var vadCount = 0;
            vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, vadCount);
            vadCount++;
            if(uvs0.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, vadCount);
                vadCount++;
            }
            if(uvs1.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, vadCount);
                vadCount++;
            }
            if(normals.IsCreated || calculateNormals) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, vadCount);
                vadCount++;
            }
            if(tangents.IsCreated || calculateTangents) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, vadCount);
                vadCount++;
            }
            if(colors32.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4, vadCount);
                vadCount++;
            } else
            if(colors.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, vadCount);
                vadCount++;
            }
#if DEBUG
            UnityEngine.Assertions.Assert.IsTrue( vadLen <= MAX_STREAM, "Too many vertex streams!" );
#endif
            msh.SetVertexBufferParams(positions.Length,vad);
            Profiler.EndSample();

            Profiler.BeginSample("SetVertices");
            vadCount = 0;
            msh.SetVertexBufferData(positions,0,0,positions.Length,vadCount,flags);
            vadCount++;
            Profiler.EndSample();

            if(uvs0.IsCreated) {
                Profiler.BeginSample("SetUVs0");
                msh.SetVertexBufferData(uvs0,0,0,uvs0.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }
            if(uvs1.IsCreated) {
                Profiler.BeginSample("SetUVs1");
                msh.SetVertexBufferData(uvs1,0,0,uvs1.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }
            if(normals.IsCreated) {
                Profiler.BeginSample("SetNormals");
                msh.SetVertexBufferData(normals,0,0,normals.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }

            if(tangents.IsCreated) {
                Profiler.BeginSample("SetTangents");
                msh.SetVertexBufferData(tangents,0,0,tangents.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }

            if(colors32.IsCreated) {
                Profiler.BeginSample("SetColors32");
                msh.SetVertexBufferData(colors32,0,0,colors32.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            } else
            if(colors.IsCreated) {
                Profiler.BeginSample("SetColors");
                msh.SetVertexBufferData(colors,0,0,colors.Length,vadCount,flags);
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

            if(!normals.IsCreated && calculateNormals) {
                Profiler.BeginSample("RecalculateNormals");
                msh.RecalculateNormals();
                Profiler.EndSample();
            }
            if(!tangents.IsCreated && calculateTangents) {
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

            Profiler.EndSample(); // CreatePrimitiveAdvances
        }
#endif // #if !LEGACY_MESH

        void CreatePrimitiveLegacy(UnityEngine.Mesh msh,bool calculateNormals,bool calculateTangents) {

            Profiler.BeginSample("CreatePrimitiveLegacy");

            if( positions.Length > 65536 ) {
#if UNITY_2017_3_OR_NEWER
                msh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                throw new System.Exception("Meshes with more than 65536 vertices are only supported from Unity 2017.3 onwards.");
#endif // UNITY_2017_3_OR_NEWER
            }

            Profiler.BeginSample("SetVertices");
#if LEGACY_MESH
            msh.vertices = positions;
#else
            msh.SetVertices(positions);
#endif
            Profiler.EndSample();

            Profiler.BeginSample("SetIndices");
            msh.subMeshCount = indices.Length;
            for (int i = 0; i < indices.Length; i++) {
                msh.SetIndices(indices[i],topology,i);
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetNormals");
#if LEGACY_MESH
            if(normals!=null) {
                msh.normals = normals;
            } else
#else
            if(normals.IsCreated) {
                msh.SetNormals(normals);
            } else
#endif
            if(calculateNormals) {
                Profiler.BeginSample("RecalculateNormals");
                msh.RecalculateNormals();
                Profiler.EndSample();
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetTangents");
#if LEGACY_MESH
            if(tangents!=null) {
                msh.tangents = tangents;
            } else
#else
            if(tangents.IsCreated) {
                msh.SetTangents(tangents);
            } else
#endif
            if(calculateTangents) {
                Profiler.BeginSample("RecalculateTangents");
                msh.RecalculateTangents();
                Profiler.EndSample();
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetUVs");
#if LEGACY_MESH
            if(uvs0!=null) {
                msh.uv = uvs0;
            }
            if(uvs1!=null) {
                msh.uv2 = uvs1;
            }
#else
            if(uvs0.IsCreated) {
                msh.SetUVs(0,uvs0);
            }
            if(uvs1.IsCreated) {
                msh.SetUVs(1,uvs1);
            }
#endif
            Profiler.EndSample();

            Profiler.BeginSample("SetColor");
#if LEGACY_MESH
            if (colors!=null) {
                msh.colors = colors;
            } else if(colors32!=null) {
                msh.colors32 = colors32;
            }
#else
            if (colors.IsCreated) {
                msh.SetColors(colors);
            } else if(colors32!=null) {
                msh.SetColors(colors32);
            }
#endif
            Profiler.EndSample();

            Profiler.BeginSample("UploadMeshData");
            msh.UploadMeshData(true);
            Profiler.EndSample();

            Profiler.EndSample(); // CreatePrimitiveLegacy
        }

        void Dispose() {
            if(calculatedIndicesHandle.IsAllocated) {
                calculatedIndicesHandle.Free();
            }
        }
    }
}
