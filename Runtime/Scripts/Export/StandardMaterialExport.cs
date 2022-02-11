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

using System;
using GLTFast.Materials;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
#if USING_URP
using UnityEngine.Rendering.Universal;
#endif
#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace GLTFast.Export {
	
	using Schema;

    public static class StandardMaterialExport {

	    const string k_KeywordBumpMap = "_BUMPMAP";
	    const string k_KeywordMetallicGlossMap = "_METALLICGLOSSMAP"; // Built-In Standard
#if USING_URP || USING_HDRP
	    const string k_KeywordMetallicSpecGlossMap = "_METALLICSPECGLOSSMAP"; // URP Lit
#endif
	    const string k_KeywordSmoothnessTextureAlbedoChannelA = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";
		    
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
		            var emissionColor = uMaterial.GetColor(k_EmissionColor);

		            // Clamp emissionColor to 0..1
		            var maxFactor = math.max(emissionColor.r,math.max(emissionColor.g,emissionColor.b));
		            if (maxFactor > 1f) {
			            emissionColor.r /= maxFactor;
			            emissionColor.g /= maxFactor;
			            emissionColor.b /= maxFactor;
			            // TODO: use maxFactor as emissiveStrength (KHR_materials_emissive_strength)
		            }

		            material.emissive = emissionColor;
	            }

	            if (uMaterial.HasProperty(k_EmissionMap)) {
		            var emissionTex = uMaterial.GetTexture(k_EmissionMap);
     
		            if (emissionTex != null) {
			            if(emissionTex is Texture2D) {
				            material.emissiveTexture = ExportTextureInfo(emissionTex, gltf);
				            ExportTextureTransform(material.emissiveTexture, uMaterial, k_EmissionMap, gltf);
			            } else {
				            logger?.Error(LogCode.TextureInvalidType, "emission", material.name );
				            success = false;
			            }
		            }
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
						material.normalTexture = ExportNormalTextureInfo(normalTex, uMaterial, gltf);
						ExportTextureTransform(material.normalTexture, uMaterial, k_BumpMap, gltf);
					} else {
						logger?.Error(LogCode.TextureInvalidType, "normal", uMaterial.name );
						success = false;
					}
				}
			}

			var isPbrMetallicRoughness = IsPbrMetallicRoughness(uMaterial);
			var needsMetalRoughTexture = 
				isPbrMetallicRoughness &&
				(
					HasMetallicGlossMap(uMaterial)
					|| uMaterial.IsKeywordEnabled(k_KeywordSmoothnessTextureAlbedoChannelA)
				);

			OrmImageExport ormImageExport = null;
			var mainTexProperty = uMaterial.HasProperty(k_BaseMap) ? k_BaseMap : k_MainTex;
			
			if (needsMetalRoughTexture) {
				ormImageExport = new OrmImageExport();
			}
			if(IsUnlit(uMaterial)) {
                ExportUnlit(material, uMaterial, mainTexProperty, gltf, logger);
			}
			else if (isPbrMetallicRoughness)
			{
				success &= ExportPbrMetallicRoughness(
					uMaterial,
					material,
					mainTexProperty,
					ormImageExport,
					gltf,
					logger
					);
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
					baseColorTexture = mainTex==null ? null : ExportTextureInfo( mainTex, gltf)
				};
			}
			else if (uMaterial.HasProperty(k_ColorTexture))
			{
				var mainTex = uMaterial.GetTexture(k_ColorTexture);
				material.pbrMetallicRoughness = new PbrMetallicRoughness {
					baseColor = uMaterial.HasProperty(k_BaseColor)
						? uMaterial.GetColor(k_BaseColor)
						: Color.white,
					baseColorTexture = mainTex==null ? null : ExportTextureInfo(mainTex, gltf)
				};
			}
            else if (uMaterial.HasProperty(k_MainTex)) //else export main texture
            {
                var mainTex = uMaterial.GetTexture(k_MainTex);

                if (mainTex != null) {
                    material.pbrMetallicRoughness = new PbrMetallicRoughness {
	                    metallicFactor = 0, roughnessFactor = 1.0f,
	                    baseColorTexture = ExportTextureInfo(mainTex, gltf)
                    };

                    // ExportTextureTransform(material.pbrMetallicRoughness.baseColorTexture, uMaterial, k_MainTex);
                }
                if (uMaterial.HasProperty(k_TintColor)) {
	                //particles use _TintColor instead of _Color
	                material.pbrMetallicRoughness = material.pbrMetallicRoughness ?? new PbrMetallicRoughness { metallicFactor = 0, roughnessFactor = 1.0f };

	                material.pbrMetallicRoughness.baseColor = uMaterial.GetColor(k_TintColor);
                }
                material.doubleSided = true;
            }
			
			if (uMaterial.HasProperty(k_OcclusionMap)) {
				var occTex = uMaterial.GetTexture(k_OcclusionMap);
				if (occTex != null) {
					if(occTex is Texture2D occTex2d) {
						if (ormImageExport == null) {
							material.occlusionTexture = ExportOcclusionTextureInfo(occTex2d, uMaterial, gltf);
						}
						else {
							material.occlusionTexture = new OcclusionTextureInfo();
							ormImageExport.SetOcclusionTexture(occTex2d);
						}
						ExportTextureTransform(
							material.occlusionTexture,
							uMaterial,
							mainTexProperty, // Standard and Lit re-use main texture transform
							gltf
							);
					} else {
						logger?.Error(LogCode.TextureInvalidType, "occlusion", material.name );
						success = false;
					}
				}
			}

			if (ormImageExport != null && material.pbrMetallicRoughness != null) {
				if (AddImageExport(gltf, ormImageExport, out var ormTextureId)) {
					if (material.pbrMetallicRoughness.metallicRoughnessTexture != null) {
						material.pbrMetallicRoughness.metallicRoughnessTexture.index = ormTextureId;
						ExportTextureTransform(material.pbrMetallicRoughness.metallicRoughnessTexture, uMaterial, k_MetallicGlossMap, gltf);
					}

					if (ormImageExport.hasOcclusion) {
						material.occlusionTexture.index = ormTextureId;
					}
				}
				else {
					Debug.LogError("TODO: logger.?");
				}
			}

			if (material.occlusionTexture != null) {
				if (uMaterial.HasProperty(MaterialGenerator.occlusionStrengthPropId)) {
					material.occlusionTexture.strength = uMaterial.GetFloat(MaterialGenerator.occlusionStrengthPropId);
				}
			}

			return success;
        }

        static bool IsUnlit(UnityEngine.Material material) {
            return material.shader.name.ToLowerInvariant().Contains("unlit");
        }
        
        static bool IsPbrMetallicRoughness(UnityEngine.Material material) {
	        return material.HasProperty(MaterialGenerator.metallicPropId) && (material.HasProperty(k_MetallicGlossMap) || material.HasProperty(k_Glossiness));
        }

        static bool IsPbrSpecularGlossiness(UnityEngine.Material material) {
	        return material.HasProperty(MaterialGenerator.specColorPropId) && material.HasProperty(MaterialGenerator.specGlossMapPropId);
        }
        
        static bool ExportPbrMetallicRoughness(
	        UnityEngine.Material uMaterial,
	        Material material,
	        int mainTexProperty,
	        OrmImageExport ormImageExport,
	        IGltfWritable gltf,
	        ICodeLogger logger
	        ) {
	        var success = true;
			var pbr = new PbrMetallicRoughness { metallicFactor = 0, roughnessFactor = 1.0f };

			var hasAlphaSmoothness = uMaterial.IsKeywordEnabled(k_KeywordSmoothnessTextureAlbedoChannelA);
			
			if (uMaterial.HasProperty(k_BaseColor))
			{
				pbr.baseColor = uMaterial.GetColor(k_BaseColor);
			} else
			if (uMaterial.HasProperty(k_Color)) {
				pbr.baseColor = uMaterial.GetColor(k_Color);
			}

            if (uMaterial.HasProperty(k_TintColor)) {
	            //particles use _TintColor instead of _Color
	            float white = 1;
                if (uMaterial.HasProperty(k_Color))
                {
                    var c = uMaterial.GetColor(k_Color);
                    white = (c.r + c.g + c.b) / 3.0f; //multiply alpha by overall whiteness of TintColor
                }

                pbr.baseColor = uMaterial.GetColor(k_TintColor) * white;
            }
            
            if (uMaterial.HasProperty(k_MainTex) || uMaterial.HasProperty(k_BaseMap)) {
	            // TODO if additive particle, render black into alpha
				// TODO use private Material.GetFirstPropertyNameIdByAttribute here, supported from 2020.1+
				var mainTex = uMaterial.GetTexture(mainTexProperty);

				if (mainTex) {
					if(mainTex is Texture2D) {
						pbr.baseColorTexture = ExportTextureInfo(
							mainTex,
							gltf,
							// Force RGB for the baseColor, so that the alpha (which is smoothness)
							// is not used for alpha-opacity
							hasAlphaSmoothness
								? ImageExportBase.Format.Jpg
								: ImageExportBase.Format.Unknown
							);
						ExportTextureTransform(pbr.baseColorTexture, uMaterial, mainTexProperty, gltf);
					} else {
						logger?.Error(LogCode.TextureInvalidType, "main", uMaterial.name );
						success = false;
					}
				}
			}

			if (uMaterial.HasProperty(k_Metallic) && !uMaterial.IsKeywordEnabled(k_KeywordMetallicGlossMap)) {
				pbr.metallicFactor = uMaterial.GetFloat(k_Metallic);
			}

			if (uMaterial.HasProperty(k_Glossiness) || uMaterial.HasProperty(k_Smoothness)) {
				var smoothnessPropId = uMaterial.HasProperty(k_Smoothness) ?  k_Smoothness : k_Glossiness;
				var metallicGlossMap = uMaterial.GetTexture(k_MetallicGlossMap);
				var smoothness = uMaterial.GetFloat(smoothnessPropId);
				pbr.roughnessFactor = (metallicGlossMap!=null || hasAlphaSmoothness) && uMaterial.HasProperty(k_GlossMapScale)
					? uMaterial.GetFloat(k_GlossMapScale)
					: 1f - smoothness;
			}

			if (uMaterial.HasProperty(k_MetallicGlossMap)) {
				var mrTex = uMaterial.GetTexture(k_MetallicGlossMap);
				if (mrTex != null) {
					if(mrTex is Texture2D mrTex2d) {
						pbr.metallicRoughnessTexture ??= new TextureInfo();
						ormImageExport.SetMetalGlossTexture(mrTex2d);
						if (HasMetallicGlossMap(uMaterial))
							pbr.metallicFactor = 1.0f;
						ExportTextureTransform(pbr.metallicRoughnessTexture, uMaterial, k_MetallicGlossMap, gltf);
					} else {
						logger?.Error(LogCode.TextureInvalidType, "metallic/gloss", uMaterial.name );
						success = false;
					}
				}
			}

			if (uMaterial.IsKeywordEnabled(k_KeywordSmoothnessTextureAlbedoChannelA)) {
				var smoothnessTex = uMaterial.GetTexture(mainTexProperty) as Texture2D;
				if (smoothnessTex != null) {
					pbr.metallicRoughnessTexture ??= new TextureInfo();
					ormImageExport.SetSmoothnessTexture(smoothnessTex);
					ExportTextureTransform(pbr.metallicRoughnessTexture, uMaterial, mainTexProperty, gltf);
				}
			}

			material.pbrMetallicRoughness = pbr;
			return success;
		}

        static bool HasMetallicGlossMap(UnityEngine.Material uMaterial) {
	        return uMaterial.IsKeywordEnabled(k_KeywordMetallicGlossMap) // Built-In Standard
#if USING_URP || USING_HDRP
		        || uMaterial.IsKeywordEnabled(k_KeywordMetallicSpecGlossMap) // URP Lit
#endif
		        ;
        }
        
        static void ExportUnlit(Material material, UnityEngine.Material uMaterial, int mainTexProperty, IGltfWritable gltf, ICodeLogger logger){

	        gltf.RegisterExtensionUsage(Extension.MaterialsUnlit);
	        material.extensions = material.extensions ?? new MaterialExtension();
	        material.extensions.KHR_materials_unlit = new MaterialUnlit();
	        
	        var pbr = material.pbrMetallicRoughness ?? new PbrMetallicRoughness();

	        if (uMaterial.HasProperty(k_Color)) {
		        pbr.baseColor = uMaterial.GetColor(k_Color);
	        }

	        if (uMaterial.HasProperty(mainTexProperty)) {
		        var mainTex = uMaterial.GetTexture(mainTexProperty);
		        if (mainTex != null) {
			        if(mainTex is Texture2D) {
				        pbr.baseColorTexture = ExportTextureInfo(mainTex, gltf);
				        ExportTextureTransform(pbr.baseColorTexture, uMaterial, mainTexProperty, gltf);
			        } else {
				        logger?.Error(LogCode.TextureInvalidType, "main", material.name );
			        }
		        }
	        }

	        material.pbrMetallicRoughness = pbr;
        }
        
        static TextureInfo ExportTextureInfo( UnityEngine.Texture texture, IGltfWritable gltf, ImageExportBase.Format format = ImageExportBase.Format.Unknown) {
	        var texture2d = texture as Texture2D;
	        if (texture2d == null) {
		        return null;
	        }
	        var imageExport = new ImageExport(texture2d, format);
	        if (AddImageExport(gltf, imageExport, out var textureId)) {
		        return new TextureInfo {
			        index = textureId,
			        // texCoord = 0 // TODO: figure out which UV set was used
		        };
	        }
	        return null;
        }
        
        static NormalTextureInfo ExportNormalTextureInfo(
	        UnityEngine.Texture texture,
	        UnityEngine.Material material,
	        IGltfWritable gltf
	        )
        {
	        var texture2d = texture as Texture2D;
	        if (texture2d == null) {
		        return null;
	        }
	        var imageExport = new NormalImageExport(texture2d);
	        if (AddImageExport(gltf, imageExport, out var textureId)) {
		        var info = new NormalTextureInfo {
			        index = textureId,
			        // texCoord = 0 // TODO: figure out which UV set was used
		        };

		        if (material.HasProperty(MaterialGenerator.bumpScalePropId)) {
			        info.scale = material.GetFloat(MaterialGenerator.bumpScalePropId);
		        }
				return info;
	        }
	        return null;
        }
        
        static OcclusionTextureInfo ExportOcclusionTextureInfo(
	        UnityEngine.Texture texture,
	        UnityEngine.Material material,
	        IGltfWritable gltf
	        )
        {
	        var texture2d = texture as Texture2D;
	        if (texture2d == null) {
		        return null;
	        }
	        var imageExport = new ImageExport(texture2d);
	        if (AddImageExport(gltf, imageExport, out var textureId)) {
		        return new OcclusionTextureInfo {
			        index = textureId
		        };
	        }
	        return null;
        }

        /// <summary>
        /// Adds an ImageExport to the glTF.
        /// No conversions or channel swizzling 
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="imageExport"></param>
        /// <param name="textureId"></param>
        /// <returns>glTF texture ID</returns>
        static bool AddImageExport(IGltfWritable gltf, ImageExportBase imageExport, out int textureId) {
	        var imageId = gltf.AddImage(imageExport);
	        if (imageId < 0) {
		        textureId = -1;
		        return false;
	        }

	        var samplerId = gltf.AddSampler(imageExport.filterMode, imageExport.wrapModeU, imageExport.wrapModeV);
	        textureId = gltf.AddTexture(imageId,samplerId);
	        return true;
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
