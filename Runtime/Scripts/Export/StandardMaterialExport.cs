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
using UnityEngine;
using UnityEngine.Rendering;

namespace GLTFast.Export {
	
	using Schema;

    public static class StandardMaterialExport {
	    
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

	    internal static Material ConvertMaterial(UnityEngine.Material uMaterial, IGltfWritable gltf ) {
            var material = new Material {
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
					var emissionTex = uMaterial.GetTexture(k_EmissionMap);

					// if (emissionTex != null) {
					// 	if(emissionTex is Texture2D) {
					// 		material.emissiveTexture = ExportTextureInfo(emissionTex, TextureMapType.Emission);
     //                        ExportTextureTransform(material.EmissiveTexture, uMaterial, "_EmissionMap");
					// 	} else {
					// 		Debug.LogErrorFormat("Can't export a {0} emissive texture in material {1}", emissionTex.GetType(), uMaterial.name);
					// 	}
     //                }
				}
			}
			if (
                uMaterial.HasProperty(k_BumpMap)
                && (uMaterial.IsKeywordEnabled("_NORMALMAP")
                || uMaterial.IsKeywordEnabled("_BUMPMAP"))
                )
            {
				var normalTex = uMaterial.GetTexture(k_BumpMap);

				if (normalTex != null) {
					// if(normalTex is Texture2D) {
					// 	material.normalTexture = ExportNormalTextureInfo(normalTex, TextureMapType.Bump, uMaterial);
					// 	ExportTextureTransform(material.NormalTexture, uMaterial, k_BumpMap);
					// } else {
					// 	Debug.LogErrorFormat("Can't export a {0} normal texture in material {1}", normalTex.GetType(), uMaterial.name);
					// }
				}
			}

			if (uMaterial.HasProperty(k_OcclusionMap)) {
				var occTex = uMaterial.GetTexture(k_OcclusionMap);
				if (occTex != null) {
					// if(occTex is Texture2D) {
					// 	material.occlusionTexture = ExportOcclusionTextureInfo(occTex, TextureMapType.Occlusion, uMaterial);
					// 	ExportTextureTransform(material.OcclusionTexture, uMaterial, "_OcclusionMap");
					// } else {
					// 	Debug.LogErrorFormat("Can't export a {0} occlusion texture in material {1}", occTex.GetType(), uMaterial.name);
					// }
				}
			}
			if(IsUnlit(uMaterial)) {
                // ExportUnlit( material, uMaterial );
			}
			else if (IsPBRMetallicRoughness(uMaterial))
			{
				material.pbrMetallicRoughness = ExportPBRMetallicRoughness(uMaterial, gltf);
			}
			else if (IsPBRSpecularGlossiness(uMaterial))
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

                    // ExportTextureTransform(material.pbrMetallicRoughness.BaseColorTexture, uMaterial, "_MainTex");
                }
                if (uMaterial.HasProperty(k_TintColor)) {
	                //particles use _TintColor instead of _Color
	                material.pbrMetallicRoughness = material.pbrMetallicRoughness ?? new PbrMetallicRoughness { metallicFactor = 0, roughnessFactor = 1.0f };

	                material.pbrMetallicRoughness.baseColor = uMaterial.GetColor(k_TintColor);
                }
                material.doubleSided = true;
            }
			return material;
        }

        static bool IsUnlit(UnityEngine.Material material) {
            return material.shader.name.ToLowerInvariant().Contains("unlit");
        }
        
        static bool IsPBRMetallicRoughness(UnityEngine.Material material) {
	        return material.HasProperty("_Metallic") && (material.HasProperty("_MetallicGlossMap") || material.HasProperty(k_Glossiness));
        }

        static bool IsPBRSpecularGlossiness(UnityEngine.Material material) {
	        return material.HasProperty("_SpecColor") && material.HasProperty("_SpecGlossMap");
        }
        
        static PbrMetallicRoughness ExportPBRMetallicRoughness(UnityEngine.Material material, IGltfWritable gltf)
		{
			var pbr = new PbrMetallicRoughness() { metallicFactor = 0, roughnessFactor = 1.0f };

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

                pbr.baseColor = material.GetColor(k_TintColor);
            }

            if (material.HasProperty("_MainTex") || material.HasProperty("_BaseMap")) {
	            // TODO if additive particle, render black into alpha
				// TODO use private Material.GetFirstPropertyNameIdByAttribute here, supported from 2020.1+
				var mainTexProperty = material.HasProperty(k_BaseMap) ? k_BaseMap : k_MainTex;
				var mainTex = material.GetTexture(mainTexProperty);

				if (mainTex) {
					if(mainTex is Texture2D) {
						pbr.baseColorTexture = ExportTextureInfo(mainTex, TextureMapType.Main, gltf);
						ExportTextureTransform(pbr.baseColorTexture, material, mainTexProperty, gltf);
					} else {
						Debug.LogErrorFormat("Can't export a {0} base texture in material {1}", mainTex.GetType(), material.name);
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
						Debug.LogErrorFormat("Can't export a {0} metallic smoothness texture in material {1}", mrTex.GetType(), material.name);
					}
				}
			}

			return pbr;
		}
        
        static TextureInfo ExportTextureInfo( UnityEngine.Texture texture, TextureMapType textureMapType, IGltfWritable gltf) {
	        var imageId = gltf.AddImage(texture);
	        var textureId = gltf.AddTexture(imageId);
	        var info = new TextureInfo {
		        index = textureId,
		        // texCoord = 0 // TODO: figure out which UV set was used
	        };
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
			        scale = new float[] { scale.x, scale.y },
			        offset = new float[] { offset.x, offset.y }
		        };
	        }
        }
    }
}
