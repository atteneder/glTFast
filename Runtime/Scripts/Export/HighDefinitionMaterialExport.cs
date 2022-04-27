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

#if USING_HDRP

using System;
using GLTFast.Logging;
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Export {
	
    using Schema;

    public class HighDefinitionMaterialExport : MaterialExportBase {

        const string k_KeywordNormalMapTangentSpace = "_NORMALMAP_TANGENT_SPACE";
        const string k_KeywordMaskMap = "_MASKMAP"; // HDRP Lit
		    
        static readonly int k_AORemapMax = Shader.PropertyToID("_AORemapMax");
        static readonly int k_AORemapMin = Shader.PropertyToID("_AORemapMin");
        static readonly int k_EmissiveColor = Shader.PropertyToID("_EmissiveColor");
        static readonly int k_EmissionColorMap = Shader.PropertyToID("_EmissiveColorMap");
        static readonly int k_NormalMap = Shader.PropertyToID("_NormalMap");
        static readonly int k_BaseColorMap = Shader.PropertyToID("_BaseColorMap");
        static readonly int k_MaskMap = Shader.PropertyToID("_MaskMap");
        static readonly int k_SmoothnessRemapMax = Shader.PropertyToID("_SmoothnessRemapMax");
        static readonly int k_SmoothnessRemapMin = Shader.PropertyToID("_SmoothnessRemapMin");

        /// <summary>
        /// Converts a Unity material to a glTF material. 
        /// </summary>
        /// <param name="uMaterial">Source material</param>
        /// <param name="material">Resulting material</param>
        /// <param name="gltf">Associated IGltfWriter. Is used for adding images and textures.</param>
        /// <param name="logger">Logger used for reporting</param>
        /// <returns>True if no errors occured, false otherwise</returns>
        public override bool ConvertMaterial(UnityEngine.Material uMaterial, out Material material, IGltfWritable gltf, ICodeLogger logger ) {
            var success = true;
            material = new Material {
                name = uMaterial.name,
                pbrMetallicRoughness = new PbrMetallicRoughness {
                    metallicFactor = 0,
                    roughnessFactor = 1.0f
                }
            };

            SetAlphaModeAndCutoff(uMaterial, material);
            material.doubleSided = IsDoubleSided(uMaterial);

            //
            // Emission
            //
            if (uMaterial.HasProperty(k_EmissiveColor)) {
                var emissionColor = uMaterial.GetColor(k_EmissiveColor);

                // Clamp emissiveColor to 0..1
                var maxFactor = math.max(emissionColor.r,math.max(emissionColor.g,emissionColor.b));
                if (maxFactor > 1f) {
                    emissionColor.r /= maxFactor;
                    emissionColor.g /= maxFactor;
                    emissionColor.b /= maxFactor;
                    // TODO: use maxFactor as emissiveStrength (KHR_materials_emissive_strength)
                }

                material.emissive = emissionColor;
            }

            if (uMaterial.HasProperty(k_EmissionColorMap)) {
                var emissionTex = uMaterial.GetTexture(k_EmissionColorMap);
 
                if (emissionTex != null) {
                    if(emissionTex is Texture2D) {
                        material.emissiveTexture = ExportTextureInfo(emissionTex, gltf);
                        ExportTextureTransform(material.emissiveTexture, uMaterial, k_EmissionColorMap, gltf);
                    } else {
                        logger?.Error(LogCode.TextureInvalidType, "emission", material.name );
                        success = false;
                    }
                }
            }

            //
            // Normal Map
            //
            if (
                uMaterial.HasProperty(k_NormalMap)
                && uMaterial.IsKeywordEnabled(k_KeywordNormalMapTangentSpace)
            )
            {
                var normalTex = uMaterial.GetTexture(k_NormalMap);

                if (normalTex != null) {
                    if(normalTex is Texture2D) {
                        material.normalTexture = ExportNormalTextureInfo(normalTex, uMaterial, gltf);
                        ExportTextureTransform(material.normalTexture, uMaterial, k_NormalMap, gltf);
                    } else {
                        logger?.Error(LogCode.TextureInvalidType, "normal", uMaterial.name );
                        success = false;
                    }
                }
            }

            
            var mainTexProperty = uMaterial.HasProperty(k_BaseColorMap) ? k_BaseColorMap : k_MainTex;
			
            if(IsUnlit(uMaterial)) {
                ExportUnlit(material, uMaterial, mainTexProperty, gltf, logger);
            } else {
                success &= ExportPbrMetallicRoughness(
                    uMaterial,
                    material,
                    gltf,
                    logger
                );
            }
			
            

            return success;
        }
        
        static bool ExportPbrMetallicRoughness(
            UnityEngine.Material uMaterial,
            Material material,
            IGltfWritable gltf,
            ICodeLogger logger
        ) {
            var success = true;
            var pbr = new PbrMetallicRoughness { metallicFactor = 0, roughnessFactor = 1.0f };

            MaskMapImageExport ormImageExport = null;
            if (uMaterial.IsKeywordEnabled(k_KeywordMaskMap) && uMaterial.HasProperty(k_MaskMap)) {
                var maskMap =  uMaterial.GetTexture(k_MaskMap) as Texture2D;
                if (maskMap != null) {
                    ormImageExport = new MaskMapImageExport(maskMap);
                    if (AddImageExport(gltf, ormImageExport, out var ormTextureId)) {
                        
                        // TODO: smartly detect if metallic roughness channels are used and not create the
                        // texture info if not. 
                        pbr.metallicRoughnessTexture = new TextureInfo {
                            index = ormTextureId
                        };
                        ExportTextureTransform(pbr.metallicRoughnessTexture, uMaterial, k_MaskMap, gltf);
                        
                        // TODO: smartly detect if occlusion channel is used and not create the
                        // texture info if not.
                        material.occlusionTexture = new OcclusionTextureInfo {
                            index = ormTextureId
                        };
                        if (uMaterial.HasProperty(k_AORemapMin) ) {
                            var occMin = uMaterial.GetFloat(k_AORemapMin);
                            material.occlusionTexture.strength =  math.clamp(1-occMin,0,1);
                            var occMax = uMaterial.GetFloat(k_AORemapMax);
                            if (occMax < 1f) {
                                // TODO: remap texture values
                                logger?.Warning(LogCode.RemapUnsupported, "AO");
                            }
                        }
                    }
                }
            }
            
            if (uMaterial.HasProperty(k_BaseColor))
            {
                pbr.baseColor = uMaterial.GetColor(k_BaseColor);
            } else
            if (uMaterial.HasProperty(k_Color)) {
                pbr.baseColor = uMaterial.GetColor(k_Color);
            }

            if (uMaterial.HasProperty(k_BaseColorMap)) {
                // TODO if additive particle, render black into alpha
                // TODO use private Material.GetFirstPropertyNameIdByAttribute here, supported from 2020.1+
                var mainTex = uMaterial.GetTexture(k_BaseColorMap);

                if (mainTex) {
                    if(mainTex is Texture2D) {
                        pbr.baseColorTexture = ExportTextureInfo(mainTex, gltf);
                        ExportTextureTransform(pbr.baseColorTexture, uMaterial, k_BaseColorMap, gltf);
                    } else {
                        logger?.Error(LogCode.TextureInvalidType, "main", uMaterial.name );
                        success = false;
                    }
                }
            }

            if (uMaterial.HasProperty(k_Metallic)) {
                pbr.metallicFactor = uMaterial.GetFloat(k_Metallic);
            }

            if (ormImageExport != null && uMaterial.HasProperty(k_SmoothnessRemapMax)) {
                pbr.roughnessFactor = uMaterial.GetFloat(k_SmoothnessRemapMax);
                if (uMaterial.HasProperty(k_SmoothnessRemapMin) && uMaterial.GetFloat(k_SmoothnessRemapMin) > 0) {
                    logger?.Warning(LogCode.RemapUnsupported,"Smoothness");
                }
            } else
            if(uMaterial.HasProperty(k_Smoothness)) {
                pbr.roughnessFactor = 1f - uMaterial.GetFloat(k_Smoothness);
            }

            material.pbrMetallicRoughness = pbr;
            return success;
        }
    }
}

#endif
