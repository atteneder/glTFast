// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace GLTFast.Schema
{
    /// <summary>
    /// KHR_materials_variants extension.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants">KHR_materials_variants extension</seealso>
    [Serializable]
    public class MaterialsVariantsRootExtension
    {
        /// <summary>
        /// Collection of material variants
        /// </summary>
        public List<MaterialsVariant> variants;

        /// <inheritdoc cref="RootExtensions.JsonUtilityCleanup"/>
        public bool JsonUtilityCleanup()
        {
            return variants != null;
        }

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddArray("variants");
            foreach (var variant in variants)
            {
                variant.GltfSerialize(writer);
            }
            writer.CloseArray();
            writer.Close();
        }
    }

    /// <summary>
    /// Named materials variant.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants">KHR_materials_variants extension</seealso>
    [Serializable]
    public class MaterialsVariant : NamedObject
    {
        internal void GltfSerialize(JsonWriter writer)
        {
            GltfSerializeName(writer);
        }
    }

    /// <summary>
    /// Mesh primitive level KHR_materials_variants extension.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants">KHR_materials_variants extension</seealso>
    [Serializable]
    public class MaterialsVariantsMeshPrimitiveExtension
    {
        /// <summary>
        /// List of material to variants mapping.
        /// </summary>
        public List<MaterialVariantsMapping> mappings;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddArray("mappings");
            foreach (var mapping in mappings)
            {
                mapping.GltfSerialize(writer);
            }
            writer.CloseArray();
            writer.Close();
        }
    }

    /// <summary>
    /// Maps a material index to one or more materials variants indices.
    /// </summary>
    [Serializable]
    public class MaterialVariantsMapping
    {
        /// <summary>Material index.</summary>
        public int material;

        /// <summary>Materials variants indices.</summary>
        public int[] variants;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddProperty("material", material);
            writer.AddArrayProperty("variants", variants);
            writer.Close();
        }
    }
}
