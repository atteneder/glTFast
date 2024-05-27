// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_SHADER_GRAPH

using System;
using GLTFast.Materials;
using GLTFast.Schema;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Material = UnityEngine.Material;

namespace GLTFast.Export
{
    /// <summary>
    /// Converts Unity Materials that use a glTFast Built-In shader to glTF materials
    /// </summary>
    public class GltfShaderGraphMaterialExporter : GltfMaterialExporter
    {
        protected override bool IsDoubleSided(Material material)
        {
#if !USING_URP || USING_URP_12_OR_NEWER
            if (TryGetValue(material, MaterialProperty.Cull, out int cull))
            {
                return cull == (int)CullMode.Off;
            }
            return false;
#else
            // Legacy URP shader graphs have postfix "-double" in their name if they are double-sided.
            var shaderName = material.shader.name;

            return shaderName.EndsWith("-double", StringComparison.InvariantCulture);
#endif
        }

        protected override MaterialBase.AlphaMode GetAlphaMode(Material material)
        {
#if !USING_URP || USING_URP_12_OR_NEWER
            if (TryGetValue(material, MaterialProperty.AlphaClip, out int alphaClip)
                && alphaClip == 1)
            {
                return MaterialBase.AlphaMode.Mask;
            }
            if (TryGetValue(material, MaterialProperty.Surface, out int surface))
            {
                return surface == 0
                    ? MaterialBase.AlphaMode.Opaque
                    : MaterialBase.AlphaMode.Blend;
            }
#else
            // Legacy URP shader graphs have postfix "-Blend" or "-Blend-double" in their name if they are blended.
            var shaderName = material.shader.name;

            var startIndex = math.max(0, shaderName.Length - 14);
            if (shaderName.LastIndexOf("-Blend", startIndex, StringComparison.InvariantCulture) >= 0
                || shaderName.LastIndexOf("-Premultiply", startIndex, StringComparison.InvariantCulture) >= 0)
            {
                return MaterialBase.AlphaMode.Blend;
            }
#endif

            return MaterialBase.AlphaMode.Opaque;
        }

        protected override float GetAlphaCutoff(Material material)
        {
            return material.GetFloat(MaterialProperty.AlphaCutoff);
        }
    }
}
#endif // UNITY_SHADER_GRAPH
