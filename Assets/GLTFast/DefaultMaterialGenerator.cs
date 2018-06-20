using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace GLTFast {

    using Materials;
    using AlphaMode = Schema.Material.AlphaMode;

    public class DefaultMaterialGenerator : IMaterialGenerator {

        Material defaultMaterial;

        public UnityEngine.Material GetDefaultMaterial() {
            if(defaultMaterial==null) {
                defaultMaterial = Resources.Load<UnityEngine.Material>("Material");
            }
            return defaultMaterial;
        }

        public UnityEngine.Material GenerateMaterial( Schema.Material gltfMaterial, Schema.Texture[] textures, Texture2D[] images, List<UnityEngine.Object> additionalResources ) {
            var material = Material.Instantiate<Material>( GetDefaultMaterial() );
            material.name = gltfMaterial.name;

            if(gltfMaterial.pbrMetallicRoughness!=null) {
				material.color = gltfMaterial.pbrMetallicRoughness.baseColor;
				material.SetFloat(StandardShaderHelper.metallicPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor );
				material.SetFloat(StandardShaderHelper.glossinessPropId, 1-gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                var mainTxt = GetTexture(gltfMaterial.pbrMetallicRoughness.baseColorTexture,textures,images);
                if(mainTxt!=null) {
                    material.mainTexture = mainTxt;
                }

                var metallicRoughnessTxt = GetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,textures,images);
                if(metallicRoughnessTxt!=null) {
					               
					Profiler.BeginSample("ConvertMetallicRoughnessTexture");
					// todo: Avoid this conversion by switching to a shader that accepts the given layout.
					Debug.LogWarning("Convert MetallicRoughnessTexture structure to fit Unity Standard Shader (slow operation).");
					var newmrt = new UnityEngine.Texture2D(metallicRoughnessTxt.width, metallicRoughnessTxt.height);
#if DEBUG
					newmrt.name = string.Format("{0}_metal_smooth", metallicRoughnessTxt.name);
#endif
					var buf = metallicRoughnessTxt.GetPixels32();               
					for (int i = 0; i < buf.Length;i++ ) {
						// TODO: Reassure given space (linear) is correct (no gamma conversion needed).
						var color = buf[i];                  
						color.a = (byte) (255 - color.g);
						color.r = color.g = color.b;                  
						buf[i] = color;
					}
					newmrt.SetPixels32(buf);
					newmrt.Apply();
					Profiler.EndSample();

					material.SetTexture( StandardShaderHelper.metallicGlossMapPropId, newmrt );
                    material.EnableKeyword("_METALLICGLOSSMAP");

                    additionalResources.Add(newmrt);
                }
            }

            var normalTxt = GetTexture(gltfMaterial.normalTexture,textures,images);
            if(normalTxt!=null) {
                material.SetTexture( StandardShaderHelper.bumpMapPropId, normalTxt);
                material.EnableKeyword("_NORMALMAP");
            }
			
            var occlusionTxt = GetTexture(gltfMaterial.occlusionTexture,textures,images);
            if(occlusionTxt !=null) {

				Profiler.BeginSample("ConvertOcclusionTexture");
                // todo: Avoid this conversion by switching to a shader that accepts the given layout.
				Debug.LogWarning("Convert OcclusionTexture structure to fit Unity Standard Shader (slow operation).");
				var newOcclusionTxt = new UnityEngine.Texture2D(occlusionTxt.width, occlusionTxt.height);
#if DEBUG
				newOcclusionTxt.name = string.Format("{0}_occlusion", occlusionTxt.name);
#endif
				var buf = occlusionTxt.GetPixels32();
                for (int i = 0; i < buf.Length; i++)
                {
                    var color = buf[i];
					color.g = color.b = color.r;
					color.a = 1;
                    buf[i] = color;
                }
                newOcclusionTxt.SetPixels32(buf);
                newOcclusionTxt.Apply();
                Profiler.EndSample();

				material.SetTexture( StandardShaderHelper.occlusionMapPropId, newOcclusionTxt );

                additionalResources.Add(newOcclusionTxt);
            }
			
            var emmissiveTxt = GetTexture(gltfMaterial.emissiveTexture,textures,images);
            if(emmissiveTxt!=null) {
                material.SetTexture( StandardShaderHelper.emissionMapPropId, emmissiveTxt);
                material.EnableKeyword("_EMISSION");
            }
            
            if(gltfMaterial.alphaModeEnum == AlphaMode.MASK) {
                material.SetFloat(StandardShaderHelper.cutoffPropId, gltfMaterial.alphaCutoff);
				StandardShaderHelper.SetAlphaModeMask( material );
            } else if(gltfMaterial.alphaModeEnum == AlphaMode.BLEND) {
                StandardShaderHelper.SetAlphaModeBlend( material );
            }

            if(gltfMaterial.emissive != Color.black) {
				material.SetColor("_EmissionColor", gltfMaterial.emissive);
				material.EnableKeyword("_EMISSION");
            }

            if(gltfMaterial.doubleSided) {
				Debug.LogWarning("Double sided shading is not supported!");
            }

            return material;
        }

        static Texture2D GetTexture(Schema.TextureInfo textureInfo, Schema.Texture[] textures, Texture2D[] images )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int bcTextureIndex = textureInfo.index;
                if (textures != null && textures.Length > bcTextureIndex)
                {
                    var txt = textures[bcTextureIndex];
                    if (images != null && images.Length > txt.source)
                    {
                        return images[txt.source];
                    }
                    else
                    {
                        Debug.LogErrorFormat("Image #{0} not found", txt.source);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Texture #{0} not found", bcTextureIndex);
                }
            }
            return null;
        }
    }
}
