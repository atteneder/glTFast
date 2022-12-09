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

#if UNITY_SHADER_GRAPH_12_OR_NEWER && GLTFAST_BUILTIN_SHADER_GRAPH

using UnityEngine;
using UnityEngine.Rendering;

namespace GLTFast.Materials {
    public class BuiltInShaderGraphMaterialGenerator : ShaderGraphMaterialGenerator {

        const string k_SurfaceTypeTransparent = "_BUILTIN_SURFACE_TYPE_TRANSPARENT";

        static readonly int k_CullModePropId = Shader.PropertyToID("_BUILTIN_CullMode");
        static readonly int k_DstBlendPropId = Shader.PropertyToID("_BUILTIN_DstBlend");
        static readonly int k_SrcBlendPropId = Shader.PropertyToID("_BUILTIN_SrcBlend");
        static readonly int k_SurfacePropId = Shader.PropertyToID("_BUILTIN_Surface");
        static readonly int k_ZWritePropId = Shader.PropertyToID("_BUILTIN_ZWrite");

        protected override void SetDoubleSided(Schema.Material gltfMaterial, Material material) {
            base.SetDoubleSided(gltfMaterial,material);
            material.SetFloat(k_CullModePropId, (int)CullMode.Off);
        }

        protected override void SetShaderModeBlend(Schema.Material gltfMaterial, Material material) {
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
