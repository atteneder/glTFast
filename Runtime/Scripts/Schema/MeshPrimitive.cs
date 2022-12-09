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

using System;

namespace GLTFast.Schema
{

    /// <summary>
    /// The topology type of primitives to render
    /// <seealso href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#_mesh_primitive_mode"/>
    /// </summary>
    public enum DrawMode
    {
        /// <summary>Points</summary>
        Points = 0,
        /// <summary>Lines</summary>
        Lines = 1,
        /// <summary>Line loop</summary>
        LineLoop = 2,
        /// <summary>Line strip</summary>
        LineStrip = 3,
        /// <summary>Triangles</summary>
        Triangles = 4,
        /// <summary>Triangle strip</summary>
        TriangleStrip = 5,
        /// <summary>Triangle fan</summary>
        TriangleFan = 6
    }

    /// <summary>
    /// Geometry to be rendered with the given material.
    /// </summary>
    [Serializable]
    public class MeshPrimitive : ICloneable
    {

        /// <summary>
        /// A dictionary object, where each key corresponds to mesh attribute semantic
        /// and each value is the index of the accessor containing attribute's data.
        /// </summary>
        public Attributes attributes;

        /// <summary>
        /// The index of the accessor that contains mesh indices.
        /// When this is not defined, the primitives should be rendered without indices
        /// using `drawArrays()`. When defined, the accessor must contain indices:
        /// the `bufferView` referenced by the accessor must have a `target` equal
        /// to 34963 (ELEMENT_ARRAY_BUFFER); a `byteStride` that is tightly packed,
        /// i.e., 0 or the byte size of `componentType` in bytes;
        /// `componentType` must be 5121 (UNSIGNED_BYTE), 5123 (UNSIGNED_SHORT)
        /// or 5125 (UNSIGNED_INT), the latter is only allowed
        /// when `OES_element_index_uint` extension is used; `type` must be `\"SCALAR\"`.
        /// </summary>
        public int indices = -1;

        /// <summary>
        /// The index of the material to apply to this primitive when rendering.
        /// </summary>
        public int material = -1;

        /// <summary>
        /// The type of primitives to render. All valid values correspond to WebGL enums.
        /// </summary>
        public DrawMode mode = DrawMode.Triangles;

        /// <summary>
        /// An array of Morph Targets, each  Morph Target is a dictionary mapping
        /// attributes to their deviations
        /// in the Morph Target (index of the accessor containing the attribute
        /// displacements' data).
        /// </summary>
        public MorphTarget[] targets;

        /// <inheritdoc cref="MeshPrimitiveExtensions"/>
        public MeshPrimitiveExtensions extensions;

#if DRACO_UNITY
        public bool IsDracoCompressed => extensions!=null && extensions.KHR_draco_mesh_compression != null;
#endif

        /// <summary>
        /// Primitives are considered equal if their attributes and morph targets (if existing)
        /// are equal. This is practical when clustering primitives of a mesh together,
        /// that end up in a single Unity Mesh.
        /// </summary>
        /// <param name="obj">Object to compare against</param>
        /// <returns>True if attributes and morph targets are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var b = (MeshPrimitive)obj;

            if (attributes.Equals(b.attributes))
            {
                if (targets == null)
                {
                    return b.targets == null;
                }
                if (b.targets != null && targets.Length == b.targets.Length)
                {
                    for (var i = 0; i < targets.Length; i++)
                    {
                        if (!targets[i].Equals(b.targets[i])) return false;
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int hash = 13;
            hash = hash * 7 + attributes.GetHashCode();
            if (targets != null)
            {
                hash = hash * 7 + targets.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Clones the object
        /// </summary>
        /// <returns>Member-wise clone</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (attributes != null)
            {
                writer.AddProperty("attributes");
                attributes.GltfSerialize(writer);
            }
            if (indices >= 0)
            {
                writer.AddProperty("indices", indices);
            }
            if (material >= 0)
            {
                writer.AddProperty("material", material);
            }
            if (mode != DrawMode.Triangles)
            {
                writer.AddProperty("mode", (int)mode);
            }
            if (targets != null)
            {
                writer.AddArray("targets");
                foreach (var target in targets)
                {
                    target.GltfSerialize(writer);
                }
                writer.CloseArray();
            }
            if (extensions != null)
            {
                extensions.GltfSerialize(writer);
            }
            writer.Close();
        }
    }

    /// <summary>
    /// Mesh vertex attribute collection. Each property value is the index of
    /// the accessor containing attribute’s data.
    /// </summary>
    [Serializable]
    public class Attributes
    {

        // Names are identical to glTF specified property names, that's why
        // inconsistent names are ignored.
        // ReSharper disable InconsistentNaming

        /// <summary>Vertex position accessor index.</summary>
        public int POSITION = -1;
        /// <summary>Vertex normals accessor index.</summary>
        public int NORMAL = -1;
        /// <summary>Vertex tangents accessor index.</summary>
        public int TANGENT = -1;
        /// <summary>Texture coordinates accessor index.</summary>
        public int TEXCOORD_0 = -1;
        /// <summary>Texture coordinates accessor index (second UV set).</summary>
        public int TEXCOORD_1 = -1;
        /// <summary>Texture coordinates accessor index (third UV set).</summary>
        public int TEXCOORD_2 = -1;
        /// <summary>Texture coordinates accessor index (fourth UV set).</summary>
        public int TEXCOORD_3 = -1;
        /// <summary>Texture coordinates accessor index (fifth UV set).</summary>
        public int TEXCOORD_4 = -1;
        /// <summary>Texture coordinates accessor index (sixth UV set).</summary>
        public int TEXCOORD_5 = -1;
        /// <summary>Texture coordinates accessor index (seventh UV set).</summary>
        public int TEXCOORD_6 = -1;
        /// <summary>Texture coordinates accessor index (eighth UV set).</summary>
        public int TEXCOORD_7 = -1;
        /// <summary>Texture coordinates accessor index (ninth UV set).</summary>
        public int TEXCOORD_8 = -1;
        /// <summary>Vertex color accessor index.</summary>
        public int COLOR_0 = -1;
        /// <summary>Bone joints accessor index.</summary>
        public int JOINTS_0 = -1;
        /// <summary>Bone weights accessor index.</summary>
        public int WEIGHTS_0 = -1;

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var b = (Attributes)obj;
            return POSITION == b.POSITION
                && NORMAL == b.NORMAL
                && TANGENT == b.TANGENT
                && TEXCOORD_0 == b.TEXCOORD_0
                && TEXCOORD_1 == b.TEXCOORD_1
                && TEXCOORD_2 == b.TEXCOORD_2
                && TEXCOORD_3 == b.TEXCOORD_3
                && TEXCOORD_4 == b.TEXCOORD_4
                && TEXCOORD_5 == b.TEXCOORD_5
                && TEXCOORD_6 == b.TEXCOORD_6
                && TEXCOORD_7 == b.TEXCOORD_7
                && COLOR_0 == b.COLOR_0
                ;
        }

        /// <summary>
        /// Default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int hash = 13;
            hash = hash * 7 + POSITION.GetHashCode();
            hash = hash * 7 + NORMAL.GetHashCode();
            hash = hash * 7 + TANGENT.GetHashCode();
            hash = hash * 7 + TEXCOORD_0.GetHashCode();
            hash = hash * 7 + TEXCOORD_1.GetHashCode();
            hash = hash * 7 + COLOR_0.GetHashCode();
            return hash;
        }

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (POSITION >= 0) writer.AddProperty("POSITION", POSITION);
            if (NORMAL >= 0) writer.AddProperty("NORMAL", NORMAL);
            if (TANGENT >= 0) writer.AddProperty("TANGENT", TANGENT);
            if (TEXCOORD_0 >= 0) writer.AddProperty("TEXCOORD_0", TEXCOORD_0);
            if (TEXCOORD_1 >= 0) writer.AddProperty("TEXCOORD_1", TEXCOORD_1);
            if (TEXCOORD_2 >= 0) writer.AddProperty("TEXCOORD_2", TEXCOORD_2);
            if (TEXCOORD_3 >= 0) writer.AddProperty("TEXCOORD_3", TEXCOORD_3);
            if (TEXCOORD_4 >= 0) writer.AddProperty("TEXCOORD_4", TEXCOORD_4);
            if (TEXCOORD_5 >= 0) writer.AddProperty("TEXCOORD_5", TEXCOORD_5);
            if (TEXCOORD_6 >= 0) writer.AddProperty("TEXCOORD_6", TEXCOORD_6);
            if (TEXCOORD_7 >= 0) writer.AddProperty("TEXCOORD_7", TEXCOORD_7);
            if (COLOR_0 >= 0) writer.AddProperty("COLOR_0", COLOR_0);
            if (JOINTS_0 >= 0) writer.AddProperty("JOINTS_0", JOINTS_0);
            if (WEIGHTS_0 >= 0) writer.AddProperty("WEIGHTS_0", WEIGHTS_0);
            writer.Close();
        }
    }

    /// <summary>
    /// Mesh primitive extensions
    /// </summary>
    [Serializable]
    public class MeshPrimitiveExtensions
    {
#if DRACO_UNITY
        // ReSharper disable once InconsistentNaming
        public MeshPrimitiveDracoExtension KHR_draco_mesh_compression;
#endif

        internal void GltfSerialize(JsonWriter writer)
        {
#if DRACO_UNITY
            if (KHR_draco_mesh_compression != null) {
                writer.AddProperty("KHR_draco_mesh_compression");
                KHR_draco_mesh_compression.GltfSerialize(writer);
            }
#endif
        }
    }

#if DRACO_UNITY
    [Serializable]
    public class MeshPrimitiveDracoExtension {
        public int bufferView;
        public Attributes attributes;

        internal void GltfSerialize(JsonWriter writer) {
            throw new NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }
#endif

    /// <summary>
    /// Morph target (blend shape)
    /// </summary>
    [Serializable]
    public class MorphTarget
    {

        // Names are identical to glTF specified property names, that's why
        // inconsistent names are ignored.
        // ReSharper disable InconsistentNaming

        /// <summary>Vertex position deviation accessor index.</summary>
        public int POSITION = -1;
        /// <summary>Vertex normal deviation accessor index.</summary>
        public int NORMAL = -1;
        /// <summary>Vertex tangent deviation accessor index.</summary>
        public int TANGENT = -1;

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var b = (MorphTarget)obj;
            return POSITION == b.POSITION
                && NORMAL == b.NORMAL
                && TANGENT == b.TANGENT
                ;
        }

        /// <summary>
        /// Default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int hash = 13;
            hash = hash * 7 + POSITION.GetHashCode();
            hash = hash * 7 + NORMAL.GetHashCode();
            hash = hash * 7 + TANGENT.GetHashCode();
            return hash;
        }

        internal void GltfSerialize(JsonWriter writer)
        {
            if (POSITION >= 0) writer.AddProperty("POSITION", POSITION);
            if (NORMAL >= 0) writer.AddProperty("NORMAL", NORMAL);
            if (TANGENT >= 0) writer.AddProperty("TANGENT", TANGENT);
        }
    }
}
