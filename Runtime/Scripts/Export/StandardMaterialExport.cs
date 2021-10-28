// Copyright 2020-2021 Andreas Atteneder
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

using System;
using GLTFast.Materials;
using UnityEngine;
using UnityEngine.Rendering;

namespace GLTFast.Export {
	
	using Schema;

    public static class StandardMaterialExport {

	    const string k_KeywordBumpMap = "_BUMPMAP";
	    
	    static readonly int k_Cutoff = Shader.PropertyToID("_Cutoff");
	    static readonly int k_Cull = Shader.PropertyToID("_Cull");
	    static readonly int k_EmissionColor = Shader.PropertyToID("_EmissionColor");
	    static readonly int k_EmissionMap = Shader.PropertyToID("_EmissionMap");
	    static readonly int k_BumpMap = Shader.PropertyToID("_BumpMap");
	    static readonly int k_OcclusionMap = Shader.PropertyToID("_OcclusionMap");
	    static readonly int k_BaseMap = Shader.PropertyToID("_BaseMap");
	    static readonly int k_ColorTexture = Shader.PropertyToID("_ColorTexture");
	    static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");
	    static readonly int k_MainTex = Shader.PropertyToID("_MainTex");
	    static readonly int k_TintColor = Shader.PropertyToID("_TintColor");
	    static readonly int k_Color = Shader.PropertyToID("_Color");
	    static readonly int k_Metallic = Shader.PropertyToID("_Metallic");
	    static readonly int k_MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");
	    static readonly int k_Smoothness = Shader.PropertyToID("_Smoothness");
	    static readonly int k_Glossiness = Shader.PropertyToID("_Glossiness");
	    static readonly int k_GlossMapScale = Shader.PropertyToID("_GlossMapScale");
		    
	    enum TextureMapType {
		    Main,
		    Bump,
		    SpecGloss,
		    Emission,
		    MetallicGloss,
		    Light,
		    Occlusion
	    }

	    /// <summary>
	    /// Converts a Unity material to a glTF material. 
	    /// </summary>
	    /// <param name="uMaterial">Source material</param>
	    /// <param name="material">Resulting material</param>
	    /// <param name="gltf">Associated IGltfWriter. Is used for adding images and textures.</param>
	    /// <param name="logger">Logger used for reporting</param>
	    /// <returns>True if no errors occured, false otherwise</returns>
	    internal static bool ConvertMaterial(UnityEngine.Material uMaterial, out Material material, IGltfWritable gltf, ICodeLogger logger ) {
		    var success = true;
            material = new Material {
                name = uMaterial.name,
                pbrMetallicRoughness = new PbrMetallicRoughness {
	                metallicFactor = 0,
	                roughnessFactor = 1.0f
                }
            };

            switch (uMaterial.GetTag("RenderType", false, ""))
            {
                case "TransparentCutout":
                    if (uMaterial.HasProperty(k_Cutoff))
                    {
                        material.alphaCutoff = uMaterial.GetFloat(k_Cutoff);
                    }
                    material.alphaModeEnum = Material.AlphaMode.MASK;
                    break;
                case "Transparent":
                case "Fade":
                    material.alphaModeEnum = Material.AlphaMode.BLEND;
                    break;
                default:
                    material.alphaModeEnum = Material.AlphaMode.OPAQUE;
                    break;
            }
            
            material.doubleSided = uMaterial.HasProperty(k_Cull) &&
				uMaterial.GetInt(k_Cull) == (int) CullMode.Off;

			if(uMaterial.IsKeywordEnabled("_EMISSION")) {
				if (uMaterial.HasProperty(k_EmissionColor)) {
					material.emissive = uMaterial.GetColor(k_EmissionColor);
				}

				if (uMaterial.HasProperty(k_EmissionMap)) {
					// var emissionTex = uMaterial.GetTexture(k_EmissionMap);
     //
					// if (emissionTex != null) {
					// 	if(emissionTex is Texture2D) {
					// 		material.emissiveTexture = ExportTextureInfo(emissionTex, TextureMapType.Emission);
     //                        ExportTextureTransform(material.EmissiveTexture, uMaterial, "_EmissionMap");
					// 	} else {
					// 		logger?.Error(LogCode.TextureInvalidType, "emission", material.name );
					//		success = false;
					// 	}
					//                }
				}
			}
			if (
                uMaterial.HasProperty(k_BumpMap)
                && (uMaterial.IsKeywordEnabled( Materials.Constants.kwNormalMap)
                || uMaterial.IsKeywordEnabled(k_KeywordBumpMap))
                )
            {
				var normalTex = uMaterial.GetTexture(k_BumpMap);

				if (normalTex != null) {
					if(normalTex is Texture2D) {
						material.normalTexture = ExportNormalTextureInfo(normalTex, TextureMapType.Bump, uMaterial, gltf);
						ExportTextureTransform(material.normalTexture, uMaterial, k_BumpMap, gltf);
					} else {
						logger?.Error(LogCode.TextureInvalidType, "normal", uMaterial.name );
						success = false;
					}
				}
			}

			if (uMaterial.HasProperty(k_OcclusionMap)) {
				var occTex = uMaterial.GetTexture(k_OcclusionMap);
				if (occTex != null) {
					// if(occTex is Texture2D) {
					// 	material.occlusionTexture = ExportOcclusionTextureInfo(occTex, TextureMapType.Occlusion, uMaterial);
					// 	ExportTextureTransform(material.OcclusionTexture, uMaterial, "_OcclusionMap");
					// } else {
					// 	logger?.Error(LogCode.TextureInvalidType, "occlusion", material.name );
					//  success = false;
					// }
				}
			}
			if(IsUnlit(uMaterial)) {
                ExportUnlit(material, uMaterial, gltf, logger);
			}
			else if (IsPbrMetallicRoughness(uMaterial))
			{
				success &= ExportPbrMetallicRoughness(uMaterial, out material.pbrMetallicRoughness, gltf, logger);
			}
			else if (IsPbrSpecularGlossiness(uMaterial))
			{
				// ExportPBRSpecularGlossiness(material, uMaterial);
			}
			else if (uMaterial.HasProperty(k_BaseMap))
			{
				var mainTex = uMaterial.GetTexture(k_BaseMap);
				material.pbrMetallicRoughness = new PbrMetallicRoughness {
					baseColor = uMaterial.HasProperty(k_BaseColor)
						? uMaterial.GetColor(k_BaseColor)
						: Color.white,
					baseColorTexture = mainTex==null ? null : ExportTextureInfo( mainTex, TextureMapType.Main, gltf)
				};
			}
			else if (uMaterial.HasProperty(k_ColorTexture))
			{
				var mainTex = uMaterial.GetTexture(k_ColorTexture);
				material.pbrMetallicRoughness = new PbrMetallicRoughness {
					baseColor = uMaterial.HasProperty(k_BaseColor)
						? uMaterial.GetColor(k_BaseColor)
						: Color.white,
					baseColorTexture = mainTex==null ? null : ExportTextureInfo(mainTex, TextureMapType.Main, gltf)
				};
			}
            else if (uMaterial.HasProperty(k_MainTex)) //else export main texture
            {
                var mainTex = uMaterial.GetTexture(k_MainTex);

                if (mainTex != null) {
                    material.pbrMetallicRoughness = new PbrMetallicRoughness {
	                    metallicFactor = 0, roughnessFactor = 1.0f,
	                    baseColorTexture = ExportTextureInfo(mainTex, TextureMapType.Main, gltf)
                    };

                    // ExportTextureTransform(material.pbrMetallicRoughness.baseColorTexture, uMaterial, "_MainTex");
                }
                if (uMaterial.HasProperty(k_TintColor)) {
	                //particles use _TintColor instead of _Color
	                material.pbrMetallicRoughness = material.pbrMetallicRoughness ?? new PbrMetallicRoughness { metallicFactor = 0, roughnessFactor = 1.0f };

	                material.pbrMetallicRoughness.baseColor = uMaterial.GetColor(k_TintColor);
                }
                material.doubleSided = true;
            }
			return success;
        }

        static bool IsUnlit(UnityEngine.Material material) {
            return material.shader.name.ToLowerInvariant().Contains("unlit");
        }
        
        static bool IsPbrMetallicRoughness(UnityEngine.Material material) {
	        return material.HasProperty("_Metallic") && (material.HasProperty("_MetallicGlossMap") || material.HasProperty(k_Glossiness));
        }

        static bool IsPbrSpecularGlossiness(UnityEngine.Material material) {
	        return material.HasProperty("_SpecColor") && material.HasProperty("_SpecGlossMap");
        }
        
        static bool ExportPbrMetallicRoughness(UnityEngine.Material material, out PbrMetallicRoughness pbr, IGltfWritable gltf, ICodeLogger logger) {
	        var success = true;
			pbr = new PbrMetallicRoughness { metallicFactor = 0, roughnessFactor = 1.0f };

			if (material.HasProperty(k_BaseColor))
			{
				pbr.baseColor = material.GetColor(k_BaseColor);
			} else
			if (material.HasProperty(k_Color)) {
				pbr.baseColor = material.GetColor(k_Color);
			}

            if (material.HasProperty(k_TintColor)) {
	            //particles use _TintColor instead of _Color
	            float white = 1;
                if (material.HasProperty(k_Color))
                {
                    var c = material.GetColor(k_Color);
                    white = (c.r + c.g + c.b) / 3.0f; //multiply alpha by overall whiteness of TintColor
                }

                pbr.baseColor = material.GetColor(k_TintColor) * white;
            }

            if (material.HasProperty(k_MainTex) || material.HasProperty("_BaseMap")) {
	            // TODO if additive particle, render black into alpha
				// TODO use private Material.GetFirstPropertyNameIdByAttribute here, supported from 2020.1+
				var mainTexProperty = material.HasProperty(k_BaseMap) ? k_BaseMap : k_MainTex;
				var mainTex = material.GetTexture(mainTexProperty);

				if (mainTex) {
					if(mainTex is Texture2D) {
						pbr.baseColorTexture = ExportTextureInfo(mainTex, TextureMapType.Main, gltf);
						ExportTextureTransform(pbr.baseColorTexture, material, mainTexProperty, gltf);
					} else {
						logger?.Error(LogCode.TextureInvalidType, "main", material.name );
						success = false;
					}
				}
			}

			if (material.HasProperty(k_Metallic) && !material.IsKeywordEnabled("_METALLICGLOSSMAP")) {
				pbr.metallicFactor = material.GetFloat(k_Metallic);
			}

			if (material.HasProperty(k_Glossiness) || material.HasProperty(k_Smoothness)) {
				var smoothnessPropertyName = material.HasProperty(k_Smoothness) ?  k_Smoothness : k_Glossiness;
				var metallicGlossMap = material.GetTexture(k_MetallicGlossMap);
				float smoothness = material.GetFloat(smoothnessPropertyName);
				// legacy workaround: the UnityGLTF shaders misuse k_Glossiness as roughness but don't have a keyword for it.
				if (material.shader.name.Equals("GLTF/PbrMetallicRoughness", StringComparison.Ordinal)) {
					smoothness = 1 - smoothness;
				}
				pbr.roughnessFactor = (metallicGlossMap!=null && material.HasProperty(k_GlossMapScale))
					? material.GetFloat(k_GlossMapScale)
					: 1f - smoothness;
			}

			if (material.HasProperty(k_MetallicGlossMap)) {
				var mrTex = material.GetTexture(k_MetallicGlossMap);

				if (mrTex != null) {
					if(mrTex is Texture2D) {
						// pbr.metallicRoughnessTexture = ExportTextureInfo(mrTex, TextureMapType.MetallicGloss);
						// if (material.IsKeywordEnabled("_METALLICGLOSSMAP"))
						// 	pbr.metallicFactor = 1.0f;
						// ExportTextureTransform(pbr.MetallicRoughnessTexture, material, k_MetallicGlossMap);
					} else {
						logger?.Error(LogCode.TextureInvalidType, "metallic/gloss", material.name );
						success = false;
					}
				}
			}

			return success;
		}
        
        static void ExportUnlit(Material material, UnityEngine.Material uMaterial, IGltfWritable gltf, ICodeLogger logger){

	        gltf.RegisterExtensionUsage(Extension.MaterialsUnlit);
	        material.extensions = material.extensions ?? new MaterialExtension();
	        material.extensions.KHR_materials_unlit = new MaterialUnlit();
	        
	        var pbr = material.pbrMetallicRoughness ?? new PbrMetallicRoughness();

	        if (uMaterial.HasProperty(k_Color)) {
		        pbr.baseColor = uMaterial.GetColor(k_Color);
	        }

	        if (uMaterial.HasProperty(k_MainTex)) {
		        var mainTex = uMaterial.GetTexture(k_MainTex);
		        if (mainTex != null) {
			        if(mainTex is Texture2D) {
				        pbr.baseColorTexture = ExportTextureInfo(mainTex, TextureMapType.Main, gltf);
				        ExportTextureTransform(pbr.baseColorTexture, uMaterial, k_MainTex, gltf);
			        } else {
				        logger?.Error(LogCode.TextureInvalidType, "main", material.name );
			        }
		        }
	        }

	        material.pbrMetallicRoughness = pbr;
        }
        
        static TextureInfo ExportTextureInfo( UnityEngine.Texture texture, TextureMapType textureMapType, IGltfWritable gltf) {
	        var imageId = gltf.AddImage(texture);
	        if (imageId < 0) {
		        return null;
	        }
	        var textureId = gltf.AddTexture(imageId);
	        var info = new TextureInfo {
		        index = textureId,
		        // texCoord = 0 // TODO: figure out which UV set was used
	        };
	        return info;
        }
        
        static NormalTextureInfo ExportNormalTextureInfo(
	        UnityEngine.Texture texture,
	        TextureMapType textureMapType,
	        UnityEngine.Material material,
	        IGltfWritable gltf
	        )
        {
	        var imageId = gltf.AddImage(texture);
	        if (imageId < 0) {
		        return null;
	        }
	        var textureId = gltf.AddTexture(imageId);
	        var info = new NormalTextureInfo {
		        index = textureId,
		        // texCoord = 0 // TODO: figure out which UV set was used
	        };

	        if (material.HasProperty(MaterialGenerator.bumpScalePropId)) {
		        info.scale = material.GetFloat(MaterialGenerator.bumpMapPropId);
	        }

	        return info;
        }
        
        static void ExportTextureTransform(TextureInfo def, UnityEngine.Material mat, int texPropertyId, IGltfWritable gltf) {
	        var offset = mat.GetTextureOffset(texPropertyId);
	        var scale = mat.GetTextureScale(texPropertyId);

	        // Counter measure for Unity/glTF texture coordinate difference
	        // TODO: Offer UV conversion as alternative
	        offset.y = 1 - offset.x;
	        scale.y *= -1;

	        if (offset != Vector2.zero || scale != Vector2.one) {
		        gltf.RegisterExtensionUsage(Extension.TextureTransform);
		        def.extensions = def.extensions ?? new TextureInfoExtension();
		        def.extensions.KHR_texture_transform = new TextureTransform {
			        scale = new[] { scale.x, scale.y },
			        offset = new[] { offset.x, offset.y }
		        };
	        }
        }
    }
}
