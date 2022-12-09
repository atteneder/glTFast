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
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// A set of parameter values that are used to define the metallic-roughness
    /// material model from Physically-Based Rendering (PBR) methodology.
    /// </summary>
    [Serializable]
    public class PbrMetallicRoughness
    {

        /// <summary>
        /// The RGBA components of the base color of the material.
        /// The fourth component (A) is the opacity of the material.
        /// These values are linear.
        /// </summary>
        public float[] baseColorFactor = { 1, 1, 1, 1 };

        /// <summary>
        /// Base color of the material in linear color space.
        /// </summary>
        public Color BaseColor
        {
            get =>
                new Color(
                    baseColorFactor[0],
                    baseColorFactor[1],
                    baseColorFactor[2],
                    baseColorFactor[3]
                );
            set
            {
                baseColorFactor = new[] { value.r, value.g, value.b, value.a };
            }
        }

        /// <summary>
        /// The base color texture.
        /// This texture contains RGB(A) components in sRGB color space.
        /// The first three components (RGB) specify the base color of the material.
        /// If the fourth component (A) is present, it represents the opacity of the
        /// material. Otherwise, an opacity of 1.0 is assumed.
        /// </summary>
        public TextureInfo baseColorTexture;

        /// <summary>
        /// The metalness of the material.
        /// A value of 1.0 means the material is a metal.
        /// A value of 0.0 means the material is a dielectric.
        /// Values in between are for blending between metals and dielectrics such as
        /// dirty metallic surfaces.
        /// This value is linear.
        /// </summary>
        public float metallicFactor = 1;

        /// <summary>
        /// The roughness of the material.
        /// A value of 1.0 means the material is completely rough.
        /// A value of 0.0 means the material is completely smooth.
        /// This value is linear.
        /// </summary>
        public float roughnessFactor = 1;

        /// <summary>
        /// The metallic-roughness texture has two components.
        /// The first component (R) contains the metallic-ness of the material.
        /// The second component (G) contains the roughness of the material.
        /// These values are linear.
        /// If the third component (B) and/or the fourth component (A) are present,
        /// they are ignored.
        /// </summary>
        public TextureInfo metallicRoughnessTexture;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (baseColorFactor != null && (
                math.abs(baseColorFactor[0] - 1f) > Constants.epsilon ||
                math.abs(baseColorFactor[1] - 1f) > Constants.epsilon ||
                math.abs(baseColorFactor[2] - 1f) > Constants.epsilon ||
                math.abs(baseColorFactor[3] - 1f) > Constants.epsilon
                ))
            {
                writer.AddArrayProperty("baseColorFactor", baseColorFactor);
            }

            if (metallicFactor < 1f)
            {
                writer.AddProperty("metallicFactor", metallicFactor);
            }
            if (roughnessFactor < 1f)
            {
                writer.AddProperty("roughnessFactor", roughnessFactor);
            }
            if (baseColorTexture != null)
            {
                writer.AddProperty("baseColorTexture");
                baseColorTexture.GltfSerialize(writer);
            }
            if (metallicRoughnessTexture != null)
            {
                writer.AddProperty("metallicRoughnessTexture");
                metallicRoughnessTexture.GltfSerialize(writer);
            }

            writer.Close();
        }
    }
}
