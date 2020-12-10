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

using System;
using System.Collections.Generic;
using GLTFast.Materials;
using UnityEngine;

namespace GLTFast {

    using AlphaMode = Schema.Material.AlphaMode;

    public class ShaderGraphMaterialGenerator : MaterialGenerator {
        
        [Flags]
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

        static readonly int alphaCutoffPropId = Shader.PropertyToID("alphaCutoff");
        static readonly int baseColorFactorPropId = Shader.PropertyToID("baseColorFactor");
        static readonly int baseColorTexturePropId = Shader.PropertyToID("baseColorTexture");
        static readonly int emissiveFactorPropId = Shader.PropertyToID("emissiveFactor");
        static readonly int emissiveTexturePropId = Shader.PropertyToID("emissiveTexture");
        static readonly int glossinessFactorPropId = Shader.PropertyToID("glossinessFactor");
        static readonly int metallicFactorPropId = Shader.PropertyToID("metallicFactor");
        static readonly int metallicRoughnessTexturePropId = Shader.PropertyToID("metallicRoughnessTexture");
        static readonly int normalScalePropId = Shader.PropertyToID("normalScale");
        static readonly int normalTexturePropId = Shader.PropertyToID("normalTexture");
        static readonly int occlusionTexturePropId = Shader.PropertyToID("occlusionTexture");
        static readonly int roughnessFactorPropId = Shader.PropertyToID("roughnessFactor");
        static readonly int specularFactorPropId = Shader.PropertyToID("specularFactor");
        static readonly int specularGlossinessTexturePropId = Shader.PropertyToID("specularGlossinessTexture");

        static Shader[] litShaders = new Shader[(int)ShaderFeature.Max];
        static Shader[] unlitShaders = new Shader[2]; // one- and double-sided

        public override Material GetDefaultMaterial() {
            return GetLitMaterial();
        }

        Material GetLitMaterial( bool metallic = true, bool opaque = true, bool doubleSided=false )
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

        static Material GetUnlitMaterial(bool doubleSided=false)
        {
            int index = doubleSided ? 0 : 1;
            if(unlitShaders[index]==null) {
                var shaderName = doubleSided ? string.Format("{0}{1}",SHADER_UNLIT,"-double") : SHADER_UNLIT;
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

        public override Material GenerateMaterial(
            Schema.Material gltfMaterial,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
        ) {
            Material material;

            MaterialType materialType = MaterialType.Unknown;

            if (gltfMaterial.extensions.KHR_materials_unlit!=null) {
                material = GetUnlitMaterial(gltfMaterial.doubleSided);
                materialType = MaterialType.Unlit;
            } else {
                bool isMetallicRoughness = gltfMaterial.extensions == null ||
                           gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness == null;
                materialType = isMetallicRoughness ? MaterialType.MetallicRoughness : MaterialType.SpecularGlossiness;
                material = GetLitMaterial(
                    isMetallicRoughness,
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
                    material.SetVector(baseColorFactorPropId, specGloss.diffuseColor);
                    material.SetVector(specularFactorPropId, specGloss.specularColor);
                    material.SetFloat(glossinessFactorPropId, specGloss.glossinessFactor);

                    TrySetTexture(specGloss.diffuseTexture,material,baseColorTexturePropId,ref textures,ref schemaImages, ref imageVariants);

                    if (TrySetTexture(specGloss.specularGlossinessTexture,material,specularGlossinessTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                        // material.EnableKeyword();
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness!=null) {
                material.SetVector(baseColorFactorPropId, gltfMaterial.pbrMetallicRoughness.baseColor);

                if (materialType != MaterialType.SpecularGlossiness) {
                    // baseColorTexture can be used by both MetallicRoughness AND Unlit materials
                    TrySetTexture(
                        gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                        material,
                        baseColorTexturePropId,
                        ref textures,
                        ref schemaImages,
                        ref imageVariants
                        );
                }

                if (materialType==MaterialType.MetallicRoughness)
                {
                    material.SetFloat(metallicFactorPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor );
                    material.SetFloat(roughnessFactorPropId, gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                    if(TrySetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,material,metallicRoughnessTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                        // material.EnableKeyword(KW_METALLIC_ROUGHNESS_MAP);
                    }

                    // TODO: When the occlusionTexture equals the metallicRoughnessTexture, we could sample just once instead of twice.
                    // if (!DifferentIndex(gltfMaterial.occlusionTexture,gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture)) {
                    //    ...
                    // }
                }
            }

            if(TrySetTexture(gltfMaterial.normalTexture,material,normalTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                // material.EnableKeyword(KW_NORMALMAP);
                material.SetFloat(normalScalePropId,gltfMaterial.normalTexture.scale);
            }
            
            if(TrySetTexture(gltfMaterial.occlusionTexture,material,occlusionTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(KW_OCCLUSION);
            }

            if(TrySetTexture(gltfMaterial.emissiveTexture,material,emissiveTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(KW_EMISSION);
            }
            
            if(gltfMaterial.alphaModeEnum == AlphaMode.MASK) {
                material.SetFloat(alphaCutoffPropId, gltfMaterial.alphaCutoff);
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest; //2450
            } else if(gltfMaterial.alphaModeEnum == AlphaMode.BLEND) {
                // Disable Alpha testing
                material.SetFloat(alphaCutoffPropId, 0);
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
            } else {
                // Disable Alpha testing
                material.SetFloat(alphaCutoffPropId, 0);
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;  //2000;
            }

            if(gltfMaterial.emissive != Color.black) {
                material.SetColor(emissiveFactorPropId, gltfMaterial.emissive);
                material.EnableKeyword(KW_EMISSION);
            }

            return material;
        }
    }
}
#endif // GLTFAST_SHADER_GRAPH
