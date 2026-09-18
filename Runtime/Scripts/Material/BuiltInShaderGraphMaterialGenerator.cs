// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if GLTFAST_BUILTIN_SHADER_GRAPH

using Unity.Cloud.Gltfast.Objects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using Material = UnityEngine.Material;

namespace Unity.Cloud.Gltfast.Materials {
    [MovedFrom(true, sourceNamespace: "GLTFast.Materials", sourceAssembly: "glTFast")]
    public class BuiltInShaderGraphMaterialGenerator : ShaderGraphMaterialGenerator {

        const string k_SurfaceTypeTransparent = "_BUILTIN_SURFACE_TYPE_TRANSPARENT";

        static readonly int k_CullModePropId = Shader.PropertyToID("_BUILTIN_CullMode");
        static readonly int k_DstBlendPropId = Shader.PropertyToID("_BUILTIN_DstBlend");
        static readonly int k_SrcBlendPropId = Shader.PropertyToID("_BUILTIN_SrcBlend");
        static readonly int k_SurfacePropId = Shader.PropertyToID("_BUILTIN_Surface");
        static readonly int k_ZWritePropId = Shader.PropertyToID("_BUILTIN_ZWrite");

        protected override void SetDoubleSided(Material gltfMaterial, Material material) {
            base.SetDoubleSided(gltfMaterial,material);
            material.SetFloat(k_CullModePropId, (int)CullMode.Off);
        }

        protected override void SetShaderModeBlend(Material gltfMaterial, Material material) {
            material.EnableKeyword(AlphaTestOnKeyword);
            material.EnableKeyword(k_SurfaceTypeTransparent);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetFloat(k_DstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(k_SrcBlendPropId, (int) BlendMode.SrcAlpha);//5
            material.SetFloat(k_SurfacePropId, 1);
            material.SetFloat(k_ZWritePropId, 0);
        }
    }
}

#endif
