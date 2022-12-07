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
