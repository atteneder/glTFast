// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// KHR_materials_variants extension.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants">KHR_materials_variants extension</seealso>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MaterialsVariantsRootExtension
    {
        /// <summary>
        /// Collection of material variants
        /// </summary>
        [JsonPropertyName("variants")]
        public List<MaterialsVariant> Variants { get; set; }
    }

    /// <summary>
    /// Named materials variant.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants">KHR_materials_variants extension</seealso>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MaterialsVariant : NamedObject { }

    /// <summary>
    /// Mesh primitive level KHR_materials_variants extension.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants">KHR_materials_variants extension</seealso>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MaterialsVariantsMeshPrimitiveExtension
    {
        /// <summary>
        /// List of material to variants mapping.
        /// </summary>
        [JsonPropertyName("mappings")]
        public List<MaterialVariantsMapping> Mappings { get; set; }

        /// <summary>
        /// Retrieves the index of the material that corresponds to a material variant.
        /// If there's no match for this variant index, it returns false. In this case the default material has to be
        /// applied.
        /// </summary>
        /// <param name="variantIndex">glTF materials variant index.</param>
        /// <param name="materialIndex">glTF material index.</param>
        /// <returns>True if there's a matching mapping with the provided variant index. False otherwise.</returns>
        public bool TryGetMaterialIndex(int variantIndex, out int materialIndex)
        {
            foreach (var mapping in Mappings)
            {
                if (mapping.Material is not { } material)
                {
                    continue;
                }
                foreach (var i in mapping.Variants)
                {
                    if (variantIndex == i)
                    {
                        materialIndex = material;
                        return true;
                    }
                }
            }

            materialIndex = -1;
            return false;
        }
    }

    /// <summary>
    /// Maps a material index to one or more materials variants indices.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MaterialVariantsMapping
    {
        /// <summary>Material index.</summary>
        [JsonPropertyName("material")]
        public int? Material { get; set; }

        /// <summary>Materials variants indices.</summary>
        [JsonPropertyName("variants")]
        public List<int> Variants { get; set; }
    }
}
