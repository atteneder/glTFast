using UnityEngine;
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
        public Vector2[] uvs1;
        public Vector4[] tangents;
        public Color32[] colors32;
        public Color[] colors;
        /// TODO remove end

        public JobHandle jobHandle;
        public int[] indices;

        public GCHandle[] gcHandles;

        public MeshTopology topology;

        public override bool IsCompleted {
            get {
                return jobHandle.IsCompleted;
            }  
        }

        public override Primitive? CreatePrimitive() {
            Profiler.BeginSample("CreatePrimitive");
            jobHandle.Complete();
            var msh = new UnityEngine.Mesh();
            if( positions.Length > 65536 ) {
#if UNITY_2017_3_OR_NEWER
                msh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                throw new System.Exception("Meshes with more than 65536 vertices are only supported from Unity 2017.3 onwards.");
#endif
            }
            msh.name = mesh.name;
            msh.vertices = positions;

            msh.SetIndices(indices,topology,0);

            if(uvs0!=null) {
                msh.uv = uvs0;
            }
            if(uvs1!=null) {
                msh.uv2 = uvs1;
            }
            if(normals!=null) {
                msh.normals = normals;
            } else {
                msh.RecalculateNormals();
            }
            if (colors!=null) {
                msh.colors = colors;
            } else if(colors32!=null) {
                msh.colors32 = colors32;
            }
            if(tangents!=null) {
                msh.tangents = tangents;
            } else {
                // TODO: Improvement idea: by only calculating tangents, if they are actually needed
                msh.RecalculateTangents();
            }
            // primitives[c.primtiveIndex] = new Primitive(msh,c.primitive.material);
            // resources.Add(msh);

            Dispose();
            Profiler.EndSample();
            return new Primitive(msh,primitive.material);
        }

        void Dispose() {
            if(gcHandles!=null) {
                for(int i=0;i<gcHandles.Length;i++) {
                    gcHandles[i].Free();
                }
            }
        }
    }
} 