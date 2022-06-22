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

#if USING_URP || USING_HDRP || (UNITY_SHADER_GRAPH_12_OR_NEWER && GLTFAST_BUILTIN_SHADER_GRAPH)
#define GLTFAST_SHADER_GRAPH
#else
#define GLTFAST_BUILTIN_RP
#endif

using System;
using GLTFast.Schema;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
#if USING_URP
using UnityEngine.Rendering.Universal;
#endif

namespace GLTFast.Materials {
    
    using Logging;

    /// <summary>
    /// Common base class for implementations of IMaterialGenerator
    /// </summary>
    public abstract class MaterialGenerator : IMaterialGenerator {
        
        /// <summary>
        /// Type of material
        /// </summary>
        protected enum MaterialType {
            // Unknown
            /// <summary>
            /// Metallic-Roughness material
            /// </summary>
            MetallicRoughness,
            /// <summary>
            /// Specular-Glossiness material
            /// </summary>
            SpecularGlossiness,
            /// <summary>
            /// Unlit material
            /// </summary>
            Unlit,
        }
        
        /// <summary>Render type key</summary>
        public const string TAG_RENDER_TYPE = "RenderType";
        /// <summary>Render type TransparentCutout value</summary>
        public const string TAG_RENDER_TYPE_CUTOUT = "TransparentCutout";
        /// <summary>Render type Opaque value</summary>
        public const string TAG_RENDER_TYPE_OPAQUE = "Opaque";
        /// <summary>Render type Fade value</summary>
        public const string TAG_RENDER_TYPE_FADE = "Fade";
        /// <summary>Render type Transparent value</summary>
        public const string TAG_RENDER_TYPE_TRANSPARENT = "Transparent";
        
        /// <summary>Shader keyword _ALPHATEST_ON</summary>
        public const string KW_ALPHATEST_ON = "_ALPHATEST_ON";
        /// <summary>Shader keyword _UV_ROTATION</summary>
        public const string KW_UV_ROTATION = "_UV_ROTATION";
        /// <summary>Shader keyword _UV_CHANNEL_SELECT</summary>
        public const string KW_UV_CHANNEL_SELECT = "_UV_CHANNEL_SELECT";
        
        /// <summary>Shader property ID for property _BumpMap</summary>
        public static readonly int bumpMapPropId = Shader.PropertyToID("_BumpMap");
        /// <summary>Shader property ID for property _BumpMapRotation</summary>
        public static readonly int bumpMapRotationPropId = Shader.PropertyToID("_BumpMapRotation");
        /// <summary>Shader property ID for property _BumpMap_ST</summary>
        public static readonly int bumpMapScaleTransformPropId = Shader.PropertyToID("_BumpMap_ST");
        /// <summary>Shader property ID for property _BumpMapUVChannel</summary>
        public static readonly int bumpMapUVChannelPropId = Shader.PropertyToID("_BumpMapUVChannel");
        /// <summary>Shader property ID for property _BumpScale</summary>
        public static readonly int bumpScalePropId = Shader.PropertyToID("_BumpScale");
        /// <summary>Shader property ID for property _Cull</summary>
        public static readonly int cullPropId = Shader.PropertyToID("_Cull");
        /// <summary>Shader property ID for property _CullMode</summary>
        public static readonly int cullModePropId = Shader.PropertyToID("_CullMode");
        /// <summary>Shader property ID for property _Cutoff</summary>
        public static readonly int cutoffPropId = Shader.PropertyToID("_Cutoff");
        /// <summary>Shader property ID for property _DstBlend</summary>
        public static readonly int dstBlendPropId = Shader.PropertyToID("_DstBlend");
        /// <summary>Shader property ID for property _EmissionColor</summary>
        public static readonly int emissionColorPropId = Shader.PropertyToID("_EmissionColor");
        /// <summary>Shader property ID for property _EmissionMap</summary>
        public static readonly int emissionMapPropId = Shader.PropertyToID("_EmissionMap");
        /// <summary>Shader property ID for property _EmissionMapRotation</summary>
        public static readonly int emissionMapRotationPropId = Shader.PropertyToID("_EmissionMapRotation");
        /// <summary>Shader property ID for property _EmissionMap_ST</summary>
        public static readonly int emissionMapScaleTransformPropId = Shader.PropertyToID("_EmissionMap_ST");
        /// <summary>Shader property ID for property _EmissionMapUVChannel</summary>
        public static readonly int emissionMapUVChannelPropId = Shader.PropertyToID("_EmissionMapUVChannel");
        /// <summary>Shader property ID for property _MainTex</summary>
        public static readonly int mainTexPropId = Shader.PropertyToID("_MainTex");
        /// <summary>Shader property ID for property _MainTexRotation</summary>
        public static readonly int mainTexRotation = Shader.PropertyToID("_MainTexRotation");
        /// <summary>Shader property ID for property _MainTex_ST</summary>
        public static readonly int mainTexScaleTransform = Shader.PropertyToID("_MainTex_ST");
        /// <summary>Shader property ID for property _MainTexUVChannel</summary>
        public static readonly int mainTexUVChannelPropId = Shader.PropertyToID("_MainTexUVChannel");
        /// <summary>Shader property ID for property _Metallic</summary>
        public static readonly int metallicPropId = Shader.PropertyToID("_Metallic");
        /// <summary>Shader property ID for property _OcclusionMap</summary>
        public static readonly int occlusionMapPropId = Shader.PropertyToID("_OcclusionMap");
        /// <summary>Shader property ID for property _OcclusionStrength</summary>
        public static readonly int occlusionStrengthPropId = Shader.PropertyToID("_OcclusionStrength");
        /// <summary>Shader property ID for property _OcclusionMapRotation</summary>
        public static readonly int occlusionMapRotationPropId = Shader.PropertyToID("_OcclusionMapRotation");
        /// <summary>Shader property ID for property _OcclusionMap_ST</summary>
        public static readonly int occlusionMapScaleTransformPropId = Shader.PropertyToID("_OcclusionMap_ST");
        /// <summary>Shader property ID for property _OcclusionMapUVChannel</summary>
        public static readonly int occlusionMapUVChannelPropId = Shader.PropertyToID("_OcclusionMapUVChannel");
        /// <summary>Shader property ID for property _SpecColor</summary>
        public static readonly int specColorPropId = Shader.PropertyToID("_SpecColor");
        /// <summary>Shader property ID for property _SpecGlossMap</summary>
        public static readonly int specGlossMapPropId = Shader.PropertyToID("_SpecGlossMap");
        /// <summary>Shader property ID for property _SpecGlossMap_ST</summary>
        public static readonly int specGlossScaleTransformMapPropId = Shader.PropertyToID("_SpecGlossMap_ST"); // TODO: Support in shader!
        /// <summary>Shader property ID for property _SpecGlossMapRotation</summary>
        public static readonly int specGlossMapRotationPropId = Shader.PropertyToID("_SpecGlossMapRotation"); // TODO: Support in shader!
        /// <summary>Shader property ID for property _SpecGlossMapUVChannel</summary>
        public static readonly int specGlossMapUVChannelPropId = Shader.PropertyToID("_SpecGlossMapUVChannel"); // TODO: Support in shader!
        /// <summary>Shader property ID for property _SrcBlend</summary>
        public static readonly int srcBlendPropId = Shader.PropertyToID("_SrcBlend");
        /// <summary>Shader property ID for property _ZWrite</summary>
        public static readonly int zWritePropId = Shader.PropertyToID("_ZWrite");

        static IMaterialGenerator defaultMaterialGenerator;
        
        /// <summary>
        /// Provides the default material generator that's being used if no
        /// custom material generator was provided. The result depends on
        /// the currently used render pipeline.
        /// </summary>
        /// <returns>The default material generator</returns>
        /// <exception cref="Exception"></exception>
        public static IMaterialGenerator GetDefaultMaterialGenerator() {

            if (defaultMaterialGenerator != null) return defaultMaterialGenerator;

            var renderPipeline = RenderPipelineUtils.renderPipeline;

            switch (renderPipeline) {
#if UNITY_SHADER_GRAPH_12_OR_NEWER && GLTFAST_BUILTIN_SHADER_GRAPH
                case RenderPipeline.BuiltIn:
                    defaultMaterialGenerator = new BuiltInShaderGraphMaterialGenerator();
                    return defaultMaterialGenerator;
#elif GLTFAST_BUILTIN_RP || UNITY_EDITOR
                case RenderPipeline.BuiltIn:
                    defaultMaterialGenerator = new BuiltInMaterialGenerator();
                    return defaultMaterialGenerator;
#endif
#if USING_URP
                case RenderPipeline.Universal:
                    var urpAsset = (UniversalRenderPipelineAsset) (QualitySettings.renderPipeline ? QualitySettings.renderPipeline : GraphicsSettings.defaultRenderPipeline);
                    defaultMaterialGenerator = new UniversalRPMaterialGenerator(urpAsset);
                    return defaultMaterialGenerator;
#endif
                case RenderPipeline.HighDefinition:
#if USING_HDRP
                    defaultMaterialGenerator = new HighDefinitionRPMaterialGenerator();
                    return defaultMaterialGenerator;
#endif
                default:
                    throw new System.Exception($"Could not determine default MaterialGenerator (render pipeline {renderPipeline})");
            }
        }

        /// <summary>
        /// Logger to be used for messaging. Can be null!
        /// </summary>
        protected ICodeLogger logger;

        /// <inheritdoc />
        public abstract UnityEngine.Material GetDefaultMaterial();

        /// <summary>
        /// Tries to load a shader and covers error handling.
        /// </summary>
        /// <param name="shaderName">The requested shader's name.</param>
        /// <returns>Requested shader or null if it couldn't be loaded.</returns>
        protected Shader FindShader(string shaderName) {
            var shader = Shader.Find(shaderName);
            if(shader==null) {
                logger?.Error(LogCode.ShaderMissing, shaderName);
            }
            return shader;
        }
        
        /// <inheritdoc />
        public abstract UnityEngine.Material GenerateMaterial(Schema.Material gltfMaterial, IGltfReadable gltf);

        /// <inheritdoc />
        public void SetLogger(ICodeLogger logger) {
            this.logger = logger;
        }

        /// <summary>
        /// Attempts assigning a glTF texture to a Unity material.
        /// </summary>
        /// <param name="textureInfo">glTF source texture</param>
        /// <param name="material">target material</param>
        /// <param name="gltf">Context glTF</param>
        /// <param name="texturePropertyId">Target texture property</param>
        /// <param name="scaleTransformPropertyId">Scale/transform (_ST) property</param>
        /// <param name="rotationPropertyId">Rotation property</param>
        /// <param name="uvChannelPropertyId">UV channel selection property</param>
        /// <returns>True if texture assignment was successful, false otherwise.</returns>
        protected bool TrySetTexture(
            TextureInfo textureInfo,
            UnityEngine.Material material,
            IGltfReadable gltf,
            int texturePropertyId,
            int scaleTransformPropertyId = -1,
            int rotationPropertyId = -1,
            int uvChannelPropertyId = -1
            )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int textureIndex = textureInfo.index;
                var srcTexture = gltf.GetSourceTexture(textureIndex);
                if (srcTexture != null)
                {
                    var texture = gltf.GetTexture(textureIndex);
                    if(texture != null) {
                        material.SetTexture(texturePropertyId,texture);
                        var isKtx = srcTexture.isKtx;
                        TrySetTextureTransform(
                            textureInfo,
                            material,
                            texturePropertyId,
                            scaleTransformPropertyId,
                            rotationPropertyId,
                            uvChannelPropertyId,
                            isKtx
                            );
                        return true;
                    }
#if UNITY_IMAGECONVERSION
                    logger?.Error(LogCode.TextureLoadFailed,textureIndex.ToString());
#endif
                } else {
                    logger?.Error(LogCode.TextureNotFound,textureIndex.ToString());
                }
            }
            return false;
        }
        
        // protected static bool DifferentIndex(TextureInfo a, TextureInfo b) {
        //     return a != null && b != null && a.index>=0 && b.index>=0 && a.index != b.index;
        // }

        void TrySetTextureTransform(
            Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int texturePropertyId,
            int scaleTransformPropertyId = -1,
            int rotationPropertyId = -1,
            int uvChannelPropertyId = -1,
            bool flipY = false
            )
        {
            // Scale (x,y) and Transform (z,w)
            float4 textureST = new float4(
                1,1,// scale
                0,0 // transform
                );

            var texCoord = textureInfo.texCoord;

            if(textureInfo.extensions != null && textureInfo.extensions.KHR_texture_transform!=null) {
                var tt = textureInfo.extensions.KHR_texture_transform;
                if (tt.texCoord >= 0) {
                    texCoord = tt.texCoord;
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
                    
                    Assert.IsTrue(rotationPropertyId >= 0,"Texture rotation property invalid!");
                    material.SetVector(rotationPropertyId, newRot);
                    
                    textureST.x *= cos;
                    textureST.y *= cos;

                    material.EnableKeyword(KW_UV_ROTATION);
                    textureST.z -= newRot.y; // move offset to move rotation point (horizontally) 
                } else {
                    // In case _UV_ROTATION keyword is set (because another texture is rotated),
                    // make sure the rotation is properly nulled
                    material.SetVector(rotationPropertyId, Vector4.zero);
                }

                textureST.w -= textureST.y * cos; // move offset to move flip axis point (vertically)
            }

            if(texCoord!=0) {
                if (uvChannelPropertyId >= 0 && texCoord < 2f) {
                    material.EnableKeyword(KW_UV_CHANNEL_SELECT);
                    material.SetFloat(uvChannelPropertyId,texCoord);
                } else {
                    logger?.Error(LogCode.UVMulti,texCoord.ToString());
                }
            }
            
            if(flipY) {
                textureST.w = 1-textureST.w; // flip offset in Y
                textureST.y = -textureST.y; // flip scale in Y
            }

            material.SetTextureOffset(texturePropertyId, textureST.zw);
            material.SetTextureScale(texturePropertyId, textureST.xy);
            Assert.IsTrue(scaleTransformPropertyId >= 0,"Texture scale/transform property invalid!");
            material.SetVector(scaleTransformPropertyId,textureST);
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
