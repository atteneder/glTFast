// Copyright 2020 Andreas Atteneder
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

#if GLTFAST_SHADER_GRAPH

using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {

    using Materials;
   
    using AlphaMode = Schema.Material.AlphaMode;

    public class ShaderGraphMaterialGenerator : MaterialGenerator {

        enum ShaderFeature {
            None = 0x0,
            Metallic = 0x1,
            Opaque = 0x2,
            DoubleSided = 0x4,
            All = 0x7,
            Max = 0x8
        }
        
        const string SHADER_UNLIT = "Shader Graphs/glTF-unlit-opaque";

        // Keywords
        const string KW_OCCLUSION = "OCCLUSION";
        const string KW_EMISSION = "EMISSION";

        static readonly int baseColorTexturePropId = Shader.PropertyToID("baseColorTexture");
        public static readonly int baseColorTextureRotationScalePropId = Shader.PropertyToID("baseColorTextureRotationScale");
        static readonly int baseColorFactorPropId = Shader.PropertyToID("baseColorFactor");
        static readonly int roughnessFactorPropId = Shader.PropertyToID("roughnessFactor");
        static readonly int metallicFactorPropId = Shader.PropertyToID("metallicFactor");
        static readonly int ormTexturePropId = Shader.PropertyToID("ormTexture");
        static readonly int normalScalePropId = Shader.PropertyToID("normalScale");
        static readonly int normalTexturePropId = Shader.PropertyToID("normalTexture");
        static readonly int emissiveFactorPropId = Shader.PropertyToID("emissiveFactor");
        static readonly int emissiveTexturePropId = Shader.PropertyToID("emissiveTexture");
        static readonly int specularFactorPropId = Shader.PropertyToID("specularFactor");
        static readonly int glossinessFactorPropId = Shader.PropertyToID("glossinessFactor");
        static readonly int specularGlossinessTexturePropId = Shader.PropertyToID("specularGlossinessTexture");

        private Shader[] litShaders = new Shader[(int)ShaderFeature.Max];
        private Shader[] unlitShaders = new Shader[2]; // one- and double-sided

        public override UnityEngine.Material GetDefaultMaterial() {
            return GetLitMaterial();
        }

        UnityEngine.Material GetLitMaterial( bool metallic = true, bool opaque = true, bool doubleSided=false )
        {
            ShaderFeature sf = ShaderFeature.None;
            if (metallic) sf |= ShaderFeature.Metallic;
            if (opaque) sf |= ShaderFeature.Opaque;
            if (doubleSided) sf |= ShaderFeature.DoubleSided;
            int index = (int)sf;

            if(litShaders[index]==null)
            {
                var shaderName = string.Format(
                    "Shader Graphs/glTF-{0}-{1}{2}",
                    metallic ? "metallic" : "specular",
                    opaque ? "opaque" : "transparent",
                    doubleSided ? "-double" : ""
                );
                litShaders[index] = FindShader(shaderName);
            }
            if(litShaders[index]==null) {
                return null;
            }
            var mat = new Material(litShaders[index]);
#if UNITY_EDITOR
            mat.doubleSidedGI = doubleSided; 
#endif
            return mat;
        }

        UnityEngine.Material GetUnlitMaterial(bool doubleSided=false)
        {
            int index = doubleSided ? 0 : 1;
            if(unlitShaders[index]==null) {
                var shaderName = doubleSided ? string.Format("{0}{2}",SHADER_UNLIT,"-double") : SHADER_UNLIT;
                unlitShaders[index] = FindShader(shaderName);
            }
            if(unlitShaders[index]==null) {
                return null;
            }
            var mat = new Material(unlitShaders[index]);
#if UNITY_EDITOR
            mat.doubleSidedGI = doubleSided;
#endif
            return mat;
        }

        public override UnityEngine.Material GenerateMaterial(
            Schema.Material gltfMaterial,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
        ) {
            UnityEngine.Material material;

            bool metallic = false;

            if (gltfMaterial.extensions.KHR_materials_unlit!=null) {
                material = GetUnlitMaterial(gltfMaterial.doubleSided);
            } else {
                metallic = gltfMaterial.extensions == null ||
                           gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness == null;
                material = GetLitMaterial(
                    metallic,
                    gltfMaterial.alphaModeEnum==AlphaMode.OPAQUE, 
                    gltfMaterial.doubleSided
                    );
            }

            if(material==null) return null;

            material.name = gltfMaterial.name;

            //added support for KHR_materials_pbrSpecularGlossiness
            if (gltfMaterial.extensions != null) {
                Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null) {
                    material.SetVector(baseColorFactorPropId, specGloss.diffuseColor.gamma);
                    material.SetVector(specularFactorPropId, specGloss.specularColor);
                    material.SetFloat(glossinessFactorPropId, specGloss.glossinessFactor);

                    TrySetTexture(specGloss.diffuseTexture,material,baseColorTexturePropId,ref textures,ref schemaImages, ref imageVariants);

                    if (TrySetTexture(specGloss.specularGlossinessTexture,material,specularGlossinessTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                        // material.EnableKeyword(StandardShaderHelper.KW_METALLICSPECGLOSSMAP);
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness!=null) {
                material.SetVector(baseColorFactorPropId, gltfMaterial.pbrMetallicRoughness.baseColor.gamma);

                TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                    material,
                    baseColorTexturePropId,
                    ref textures,
                    ref schemaImages,
                    ref imageVariants
                    );

                if (metallic)
                {
                    material.SetFloat(metallicFactorPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor );
                    material.SetFloat(roughnessFactorPropId, gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                    if(TrySetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,material,ormTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                        // material.EnableKeyword(StandardShaderHelper.KW_METALLICSPECGLOSSMAP);
                    }

                    if (DifferentIndex(gltfMaterial.occlusionTexture,gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture)) {
                        // Could be avoided by providing two texture slots
                        // But that would mean two samplers instead of one :/
                        Debug.LogError("Inconsistent textures O!=RM");
                    }
                }
            }

            if(TrySetTexture(gltfMaterial.normalTexture,material,normalTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(StandardShaderHelper.KW_NORMALMAP);
                material.SetFloat(normalScalePropId,gltfMaterial.normalTexture.scale);
            }
            
            if(TrySetTexture(gltfMaterial.occlusionTexture,material,ormTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(KW_OCCLUSION);
            }

            if(TrySetTexture(gltfMaterial.emissiveTexture,material,emissiveTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(KW_EMISSION);
            }
            
            if(gltfMaterial.alphaModeEnum == AlphaMode.MASK) {
                StandardShaderHelper.SetAlphaModeMask(material, gltfMaterial.alphaCutoff);
            } else if(gltfMaterial.alphaModeEnum == AlphaMode.BLEND) {
                StandardShaderHelper.SetAlphaModeBlend(material);
            } else {
                StandardShaderHelper.SetOpaqueMode(material);
            }

            if(gltfMaterial.emissive != Color.black) {
                material.SetColor(emissiveFactorPropId, gltfMaterial.emissive.gamma);
                material.EnableKeyword(KW_EMISSION);
            }

            return material;
        }
    }
}
#endif // GLTFAST_SHADER_GRAPH
