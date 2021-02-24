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

#if USING_URP || USING_HDRP
#define GLTFAST_SHADER_GRAPH
#else
#define GLTFAST_BUILTIN_RP
#endif

using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
#if USING_URP
using UnityEngine.Rendering.Universal;
#endif
#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace GLTFast.Materials {

    public abstract class MaterialGenerator : IMaterialGenerator {
        protected enum MaterialType {
            // Unknown,
            MetallicRoughness,
            SpecularGlossiness,
            Unlit,
        }
        
        public const string KW_UV_ROTATION = "_UV_ROTATION";
        
        public static readonly int bumpMapPropId = Shader.PropertyToID("_BumpMap");
        public static readonly int bumpScalePropId = Shader.PropertyToID("_BumpScale");
        public static readonly int cutoffPropId = Shader.PropertyToID("_Cutoff");
        public static readonly int emissionColorPropId = Shader.PropertyToID("_EmissionColor");
        public static readonly int emissionMapPropId = Shader.PropertyToID("_EmissionMap");
        public static readonly int mainTexPropId = Shader.PropertyToID("_MainTex");
        public static readonly int mainTexRotation = Shader.PropertyToID("_MainTexRotation");
        public static readonly int mainTexScaleTransform = Shader.PropertyToID("_MainTex_ST");
        public static readonly int metallicPropId = Shader.PropertyToID("_Metallic");
        public static readonly int occlusionMapPropId = Shader.PropertyToID("_OcclusionMap");
        public static readonly int occlusionStrengthPropId = Shader.PropertyToID("_OcclusionStrength");
        public static readonly int specColorPropId = Shader.PropertyToID("_SpecColor");
        public static readonly int specGlossMapPropId = Shader.PropertyToID("_SpecGlossMap");

        const string ERROR_MULTI_UVS = "Multiple UV sets are not supported!";
        
        static IMaterialGenerator defaultMaterialGenerator;
        
        public static IMaterialGenerator GetDefaultMaterialGenerator() {

            if (defaultMaterialGenerator != null) return defaultMaterialGenerator;

            // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
            if (GraphicsSettings.renderPipelineAsset != null) {
#if USING_URP
                if (GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset urpAsset) {
                    defaultMaterialGenerator = new UniveralRPMaterialGenerator(urpAsset);
                    return defaultMaterialGenerator;
                }
#endif
#if USING_HDRP
                if (GraphicsSettings.renderPipelineAsset is HDRenderPipelineAsset) {
                    defaultMaterialGenerator = new HighDefinitionRPMaterialGenerator();
                    return defaultMaterialGenerator;
                }
#endif
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError("glTFast: Unknown Render Pipeline");
            }
#if GLTFAST_BUILTIN_RP || UNITY_EDITOR
            defaultMaterialGenerator = new BuiltInMaterialGenerator();
            return defaultMaterialGenerator;
#else
            throw new System.Exception("Could not determine default MaterialGenerator");
#endif
        }

        public abstract UnityEngine.Material GetDefaultMaterial();

        public abstract UnityEngine.Material GenerateMaterial(
            Schema.Material gltfMaterial,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
        );

        protected static Shader FindShader(string shaderName) {
            var shader = Shader.Find(shaderName);
            if(shader==null) {
                Debug.LogErrorFormat(
                    "Shader \"{0}\" is missing. Make sure to include it in the build (see https://github.com/atteneder/glTFast/blob/main/Documentation%7E/glTFast.md#materials-and-shader-variants )",
                    shaderName
                    );
            }
            return shader;
        }

        protected static bool TrySetTexture(
            Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int propertyId,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
            )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int bcTextureIndex = textureInfo.index;
                if (textures != null && textures.Length > bcTextureIndex)
                {
                    var txt = textures[bcTextureIndex];
                    var imageIndex = txt.GetImageIndex();

                    Texture2D img = null;
                    if( imageVariants!=null
                        && imageIndex >= 0
                        && imageVariants.Length > imageIndex
                        && imageVariants[imageIndex]!=null
                        && imageVariants[imageIndex].TryGetValue(txt.sampler,out img)
                        )
                    {
                        if(textureInfo.texCoord!=0) {
                            Debug.LogError(ERROR_MULTI_UVS);
                        }
                        material.SetTexture(propertyId,img);
                        var isKtx = txt.isKtx;
                        TrySetTextureTransform(textureInfo,material,propertyId,isKtx);
                        return true;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Image #{0} not found", imageIndex);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Texture #{0} not found", bcTextureIndex);
                }
            }
            return false;
        }
        
        protected static bool DifferentIndex(Schema.TextureInfo a, Schema.TextureInfo b) {
            return a != null && b != null && a.index>=0 && b.index>=0 && a.index != b.index;
        }

        private static void TrySetTextureTransform(
            Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int propertyId,
            bool flipY = false
            )
        {
            // Scale (x,y) and Transform (z,w)
            float4 textureST = new float4(
                1,1,// scale
                0,0 // transform
                );

            if(textureInfo.extensions != null && textureInfo.extensions.KHR_texture_transform!=null) {
                var tt = textureInfo.extensions.KHR_texture_transform;
                if(tt.texCoord!=0) {
                    Debug.LogError(ERROR_MULTI_UVS);
                }

                float cos = 1;
                float sin = 0;

                if(tt.offset!=null) {
                    textureST.z = tt.offset[0];
                    textureST.w = 1-tt.offset[1];
                }
                if(tt.scale!=null) {
                    textureST.x = tt.scale[0];
                    textureST.y = tt.scale[1];
                }
                if(tt.rotation!=0) {
                    cos = math.cos(tt.rotation);
                    sin = math.sin(tt.rotation);

                    var newRot = new Vector2(textureST.x * sin, textureST.y * -sin );
                    material.SetVector(mainTexRotation, newRot);
                    textureST.x *= cos;
                    textureST.y *= cos;

                    material.EnableKeyword(KW_UV_ROTATION);
                    textureST.z -= newRot.y; // move offset to move rotation point (horizontally) 
                }

                textureST.w -= textureST.y * cos; // move offset to move flip axis point (vertically)
            }

            if(flipY) {
                textureST.z = 1-textureST.z; // flip offset in Y
                textureST.y = -textureST.y; // flip scale in Y
            }
            
            if(material.HasProperty(mainTexPropId)) {
                material.SetTextureOffset(mainTexPropId, textureST.zw);
                material.SetTextureScale(mainTexPropId, textureST.xy);
            }
            material.SetTextureOffset(propertyId, textureST.zw);
            material.SetTextureScale(propertyId, textureST.xy);
            material.SetVector(mainTexScaleTransform, textureST);
        }
        
        /// <summary>
        /// Approximates Transmission material effect for Render Pipelines / Shaders where filtering the
        /// backbuffer is not possible.
        /// </summary>
        /// <param name="transmission">glTF transmission extension data</param>
        /// <param name="baseColorLinear">BaseColor reference. Alpha will be altered according to transmission</param>
        /// <returns>True when the transmission can be approximated with Premultiply mode. False if blending is better</returns>
        protected static bool TransmissionWorkaroundShaderMode(Transmission transmission, ref Color baseColorLinear) {
            var min = Mathf.Min(Mathf.Min(baseColorLinear.r, baseColorLinear.g), baseColorLinear.b);
            var max = baseColorLinear.maxColorComponent;
            if (max - min < .1f) {
                // R/G/B components don't diverge too much
                // -> white/grey/black-ish color
                // -> Approximation via Transparent mode should be close to real transmission
                baseColorLinear.a *= 1 - transmission.transmissionFactor;
                return true;
            }
            else {
                // Color is somewhat saturated
                // -> Fallback to Blend mode
                // -> Dial down transmissionFactor by 50% to avoid material completely disappearing
                // Shows at least some color tinting
                baseColorLinear.a *= 1 - transmission.transmissionFactor * 0.5f;

                // Premultiply color? Decided not to. I prefered vivid (but too bright) colors over desaturation effect. 
                // baseColorLinear.r *= baseColorLinear.a;
                // baseColorLinear.g *= baseColorLinear.a;
                // baseColorLinear.b *= baseColorLinear.a;

                return false;
            }
        }
    }
}
