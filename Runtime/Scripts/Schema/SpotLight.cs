// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Mathematics;

namespace GLTFast.Schema
{
    /// <summary>
    /// glTF spot light properties
    /// </summary>
    [Serializable]
    public class SpotLight
    {

        /// <summary>
        /// Angle, in radians, from centre of spotlight where falloff begins
        /// Must be greater than or equal to 0 and less than outerConeAngle
        /// </summary>
        public float innerConeAngle;

        /// <summary>
        /// Angle, in radians, from centre of spotlight where falloff ends.
        /// Must be greater than innerConeAngle and less than or equal to
        /// PI / 2.0.
        /// </summary>
        public float outerConeAngle = math.PI / 4f;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddProperty("innerConeAngle", innerConeAngle);
            writer.AddProperty("outerConeAngle", outerConeAngle);
            writer.Close();
        }
    }
}
