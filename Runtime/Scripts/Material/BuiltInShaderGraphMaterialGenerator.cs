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

#if UNITY_SHADER_GRAPH_12_OR_NEWER && !GLTFAST_FORCE_BUILTIN_SHADERS

using UnityEngine;
using UnityEngine.Rendering;

namespace GLTFast.Materials {
    public class BuiltInShaderGraphMaterialGenerator : ShaderGraphMaterialGenerator {

        static readonly int k_CullModePropId = Shader.PropertyToID("_BUILTIN_CullMode");
        
        protected override void SetDoubleSided(Schema.Material gltfMaterial, Material material) {
            base.SetDoubleSided(gltfMaterial,material);
            material.SetFloat(k_CullModePropId, (int)CullMode.Off);
        }
    }
}

#endif
