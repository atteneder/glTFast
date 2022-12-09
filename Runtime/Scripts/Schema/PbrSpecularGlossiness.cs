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

using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// This extension defines the specular-glossiness material model from
    /// Physically-Based Rendering (PBR).
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Archived/KHR_materials_pbrSpecularGlossiness"/>
    /// </summary>
    [System.Serializable]
    public class PbrSpecularGlossiness
    {

        /// <summary>
        /// Diffuse color red, green, blue and alpha components in linear space.
        /// </summary>
        public float[] diffuseFactor = { 1, 1, 1, 1 };

        /// <summary>
        /// Diffuse color in linear space.
        /// </summary>
        public Color DiffuseColor =>
            new Color(
                diffuseFactor[0],
                diffuseFactor[1],
                diffuseFactor[2],
                diffuseFactor[3]
            );

        /// <summary>
        /// Diffuse color texture info.
        /// </summary>
        public TextureInfo diffuseTexture;

        /// <summary>
        /// Specular color red, green and blue components in linear space.
        /// </summary>
        public float[] specularFactor = { 1, 1, 1 };

        /// <summary>
        /// Specular color in linear space.
        /// </summary>
        public Color SpecularColor =>
            new Color(
                specularFactor[0],
                specularFactor[1],
                specularFactor[2]
            );

        /// <summary>
        /// The glossiness or smoothness of the material.
        /// </summary>
        public float glossinessFactor = 1;

        /// <summary>
        /// The specular-glossiness texture.
        /// </summary>
        public TextureInfo specularGlossinessTexture;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.Close();
            throw new System.NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }
}
