// Copyright 2020-2022 Andreas Atteneder
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

namespace GLTFast.Schema {

    [System.Serializable]
    public class Node : RootChild {

        /// <summary>
        /// The indices of this node's children.
        /// </summary>
        public uint[] children;

        /// <summary>
        /// The index of the mesh in this node.
        /// </summary>
        public int mesh = -1;

        /// <summary>
        /// A floating-point 4x4 transformation matrix stored in column-major order.
        /// </summary>
        public float[] matrix;

        /// <summary>
        /// The node's unit quaternion rotation in the order (x, y, z, w),
        /// where w is the scalar.
        /// </summary>
        public float[] rotation;

        /// <summary>
        /// The node's non-uniform scale.
        /// </summary>
        public float[] scale;

        /// <summary>
        /// The node's translation.
        /// </summary>
        public float[] translation;

        /// <summary>
        /// The weights of the instantiated Morph Target.
        /// Number of elements must match number of Morph Targets of used mesh.
        /// </summary>
        //public double[] weights;

        /// <summary>
        /// </summary>
        public int skin = -1;

        /// <summary>
        /// Camera index
        /// </summary>
        public int camera = -1;

        public NodeExtensions extensions;
        
        internal void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            GltfSerializeRoot(writer);

            if (children != null) {
                writer.AddArrayProperty("children", children);
            }

            if (mesh >= 0) {
                writer.AddProperty("mesh", mesh);
            }
            
            if (translation!=null) {
                writer.AddArrayProperty("translation", translation);
            }
            
            if (rotation!=null) {
                writer.AddArrayProperty("rotation", rotation);
            }
            
            if (scale!=null) {
                writer.AddArrayProperty("scale", scale);
            }
            
            if (matrix!=null) {
                writer.AddArrayProperty("matrix", matrix);
            }
            
            if (skin >= 0) {
                writer.AddProperty("skin", skin);
            }
            
            if (camera >= 0) {
                writer.AddProperty("camera", skin);
            }

            if (extensions != null) {
                extensions.GltfSerialize(writer);
            }
            writer.Close();
        }
    }
    
    [System.Serializable]
    public class NodeExtensions {
        public MeshGpuInstancing EXT_mesh_gpu_instancing;
        // Whenever an extension is added, the JsonParser
        // (specifically step four of JsonParser.ParseJson)
        // needs to be updated!

        internal void GltfSerialize(JsonWriter writer) {
            if (EXT_mesh_gpu_instancing != null) {
                writer.AddProperty("EXT_mesh_gpu_instancing");
                EXT_mesh_gpu_instancing.GltfSerialize(writer);
            }
        }
    }
}