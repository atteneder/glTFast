// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_SHADER_GRAPH

using System;
using Unity.Cloud.Gltfast.Materials;
using Unity.Cloud.Gltfast.Objects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using Material = UnityEngine.Material;

namespace Unity.Cloud.Gltfast.Export
{
    /// <summary>
    /// Converts Unity Materials that use a glTFast Built-In shader to glTF materials
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Export", sourceAssembly: "glTFast.Export")]
    public class GltfShaderGraphMaterialExporter : GltfMaterialExporter
    {
        protected override bool IsDoubleSided(Material material)
        {
            if (TryGetValue(material, MaterialProperty.Cull, out int cull))
            {
                return cull == (int)CullMode.Off;
            }
            return false;
        }

        protected override AlphaMode GetAlphaMode(Material material)
        {
            if (TryGetValue(material, MaterialProperty.AlphaClip, out int alphaClip)
                && alphaClip == 1)
            {
                return AlphaMode.Mask;
            }
            if (TryGetValue(material, MaterialProperty.Surface, out int surface))
            {
                return surface == 0
                    ? AlphaMode.Opaque
                    : AlphaMode.Blend;
            }
            return AlphaMode.Opaque;
        }

        protected override float GetAlphaCutoff(Material material)
        {
            return material.GetFloat(MaterialProperty.AlphaCutoff);
        }
    }
}
#endif // UNITY_SHADER_GRAPH
