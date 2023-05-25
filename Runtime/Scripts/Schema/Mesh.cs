// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast.Schema
{

    /// <summary>
    /// A set of primitives to be rendered. Its global transform is defined by
    /// a node that references it.
    /// </summary>
    [Serializable]
    public class Mesh : NamedObject, ICloneable
    {

        /// <summary>
        /// An array of primitives, each defining geometry to be rendered with
        /// a material.
        /// <minItems>1</minItems>
        /// </summary>
        public MeshPrimitive[] primitives;

        /// <summary>
        /// Array of weights to be applied to the Morph Targets.
        /// <minItems>0</minItems>
        /// </summary>
        public float[] weights;

        /// <inheritdoc cref="MeshExtras"/>
        public MeshExtras extras;

        /// <summary>
        /// Clones the Mesh object
        /// </summary>
        /// <returns>Member-wise clone</returns>
        public object Clone()
        {
            var clone = (Mesh)MemberwiseClone();
            if (primitives != null)
            {
                clone.primitives = new MeshPrimitive[primitives.Length];
                for (var i = 0; i < primitives.Length; i++)
                {
                    clone.primitives[i] = (MeshPrimitive)primitives[i].Clone();
                }
            }
            return clone;
        }

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeRoot(writer);
            if (primitives != null)
            {
                writer.AddArray("primitives");
                foreach (var primitive in primitives)
                {
                    primitive.GltfSerialize(writer);
                }
                writer.CloseArray();
            }

            if (weights != null)
            {
                writer.AddArrayProperty("weights", weights);
            }

            if (extras != null)
            {
                writer.AddProperty("extras");
                extras.GltfSerialize(writer);
                writer.Close();
            }
            writer.Close();
        }
    }

    /// <summary>
    /// Mesh specific extra data.
    /// </summary>
    [Serializable]
    public class MeshExtras
    {

        /// <summary>
        /// Morph targets' names
        /// </summary>
        public string[] targetNames;

        internal void GltfSerialize(JsonWriter writer)
        {
            if (targetNames != null)
            {
                writer.AddArrayProperty("targetNames", targetNames);
            }
        }
    }
}
