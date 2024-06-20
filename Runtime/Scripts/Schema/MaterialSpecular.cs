// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Schema
{
    /// <summary>
    /// This extension allows configuring the specular reflection.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_specular"/>
    [Serializable]
    public class MaterialSpecular
    {
        /// <summary>
        /// The strength of the specular reflection.
        /// </summary>
        public float specularFactor = 1f;

        /// <summary>
        /// A texture that defines the strength of the specular reflection, stored in the alpha (A) channel.
        /// This will be multiplied by specularFactor.
        /// </summary>
        public TextureInfo specularTexture;

        /// <summary>
        /// The F0 color of the specular reflection (linear RGB).
        /// </summary>
        public float[] specularColorFactor = { 1, 1, 1 };

        /// <inheritdoc cref="specularColorFactor"/>
        public Color SpecularColor
        {
            get =>
                new Color(
                    specularColorFactor[0],
                    specularColorFactor[1],
                    specularColorFactor[2]
                );
            set
            {
                specularColorFactor = new[] { value.r, value.g, value.b };
            }
        }

        /// <summary>
        /// A texture that defines the F0 color of the specular reflection, stored in the RGB channels and encoded in
        /// sRGB. This texture will be multiplied by specularColorFactor.
        /// </summary>
        public TextureInfo specularColorTexture;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (math.abs(specularFactor - 1f) > Constants.epsilon)
            {
                writer.AddProperty("specularFactor", specularFactor);
            }
            if (specularTexture != null)
            {
                writer.AddProperty("specularTexture");
                specularTexture.GltfSerialize(writer);
            }
            if (specularColorFactor != null && specularColorFactor.Length > 2 && (
                    math.abs(specularColorFactor[0] - 1f) > Constants.epsilon ||
                    math.abs(specularColorFactor[1] - 1f) > Constants.epsilon ||
                    math.abs(specularColorFactor[2] - 1f) > Constants.epsilon
                ))
            {
                writer.AddArrayProperty("specularColorFactor", specularColorFactor);
            }
            if (specularColorTexture != null)
            {
                writer.AddProperty("specularColorTexture");
                specularColorTexture.GltfSerialize(writer);
            }
            writer.Close();
        }
    }
}
