// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// This extension defines the specular-glossiness material model from
    /// Physically-Based Rendering (PBR).
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Archived/KHR_materials_pbrSpecularGlossiness"/>
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
