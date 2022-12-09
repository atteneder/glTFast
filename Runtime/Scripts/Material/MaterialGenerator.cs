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
#if USING_URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

namespace GLTFast.Materials
{

    using Logging;

    /// <summary>
    /// Common base class for implementations of IMaterialGenerator
    /// </summary>
    public abstract class MaterialGenerator : IMaterialGenerator
    {

        /// <summary>
        /// Type of material
        /// </summary>
        protected enum MaterialType
        {
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

        // ReSharper disable MemberCanBeProtected.Global
        // ReSharper disable MemberCanBePrivate.Global

        /// <summary>When a glTF mesh(primitive) has no material assigned,
        /// a default material, has to get created and assigned. This default material should get named after this
        /// field's value to ensure future round-trip workflows are functioning.</summary>
        public const string DefaultMaterialName = "glTF-Default-Material";

        /// <summary>Render type key</summary>
        public const string RenderTypeTag = "RenderType";
        /// <summary>Render type TransparentCutout value</summary>
        public const string TransparentCutoutRenderType = "TransparentCutout";
        /// <summary>Render type Opaque value</summary>
        public const string OpaqueRenderType = "Opaque";
        /// <summary>Render type Fade value</summary>
        public const string FadeRenderType = "Fade";
        /// <summary>Render type Transparent value</summary>
        public const string TransparentRenderType = "Transparent";

        /// <summary>Shader keyword _ALPHATEST_ON</summary>
        public const string AlphaTestOnKeyword = "_ALPHATEST_ON";
        /// <summary>Shader keyword _TEXTURE_TRANSFORM</summary>
        public const string TextureTransformKeyword = "_TEXTURE_TRANSFORM";
        /// <summary>Shader keyword _UV_CHANNEL_SELECT</summary>
        public const string UVChannelSelectKeyword = "_UV_CHANNEL_SELECT";

        /// <summary>Shader property ID for property alphaCutoff</summary>
        public static readonly int AlphaCutoffProperty = Shader.PropertyToID("alphaCutoff");
        /// <summary>Shader property ID for property baseColorFactor</summary>
        public static readonly int BaseColorProperty = Shader.PropertyToID("baseColorFactor");
        /// <summary>Shader property ID for property baseColorTexture</summary>
        public static readonly int BaseColorTextureProperty = Shader.PropertyToID("baseColorTexture");
        /// <summary>Shader property ID for property baseColorTexture_Rotation</summary>
        public static readonly int BaseColorTextureRotationProperty = Shader.PropertyToID("baseColorTexture_Rotation");
        /// <summary>Shader property ID for property baseColorTexture_ST</summary>
        public static readonly int BaseColorTextureScaleTransformProperty = Shader.PropertyToID("baseColorTexture_ST");
        /// <summary>Shader property ID for property baseColorTexture_texCoord</summary>
        public static readonly int BaseColorTextureTexCoordProperty = Shader.PropertyToID("baseColorTexture_texCoord");
        /// <summary>Shader property ID for property _CullMode</summary>
        public static readonly int CullModeProperty = Shader.PropertyToID("_CullMode");
        /// <summary>Shader property ID for property _Cull</summary>
        public static readonly int CullProperty = Shader.PropertyToID("_Cull");
        /// <summary>Shader property ID for property _DstBlend</summary>
        public static readonly int DstBlendProperty = Shader.PropertyToID("_DstBlend");
        /// <summary>Shader property ID for property diffuseFactor</summary>
        public static readonly int DiffuseFactorProperty = Shader.PropertyToID("diffuseFactor");
        /// <summary>Shader property ID for property diffuseTexture</summary>
        public static readonly int DiffuseTextureProperty = Shader.PropertyToID("diffuseTexture");
        /// <summary>Shader property ID for property diffuseTexture_ST</summary>
        public static readonly int DiffuseTextureScaleTransformProperty = Shader.PropertyToID("diffuseTexture_ST");
        /// <summary>Shader property ID for property diffuseTexture_Rotation</summary>
        public static readonly int DiffuseTextureRotationProperty = Shader.PropertyToID("diffuseTexture_Rotation");
        /// <summary>Shader property ID for property diffuseTexture_texCoord</summary>
        public static readonly int DiffuseTextureTexCoordProperty = Shader.PropertyToID("diffuseTexture_texCoord");
        /// <summary>Shader property ID for property emissiveFactor</summary>
        public static readonly int EmissiveFactorProperty = Shader.PropertyToID("emissiveFactor");
        /// <summary>Shader property ID for property emissiveTexture</summary>
        public static readonly int EmissiveTextureProperty = Shader.PropertyToID("emissiveTexture");
        /// <summary>Shader property ID for property emissiveTexture_Rotation</summary>
        public static readonly int EmissiveTextureRotationProperty = Shader.PropertyToID("emissiveTexture_Rotation");
        /// <summary>Shader property ID for property emissiveTexture_ST</summary>
        public static readonly int EmissiveTextureScaleTransformProperty = Shader.PropertyToID("emissiveTexture_ST");
        /// <summary>Shader property ID for property emissiveTexture_texCoord</summary>
        public static readonly int EmissiveTextureTexCoordProperty = Shader.PropertyToID("emissiveTexture_texCoord");
        /// <summary>Shader property ID for property glossinessFactor</summary>
        public static readonly int GlossinessFactorProperty = Shader.PropertyToID("glossinessFactor");
        /// <summary>Shader property ID for property normalTexture</summary>
        public static readonly int NormalTextureProperty = Shader.PropertyToID("normalTexture");
        /// <summary>Shader property ID for property normalTexture_Rotation</summary>
        public static readonly int NormalTextureRotationProperty = Shader.PropertyToID("normalTexture_Rotation");
        /// <summary>Shader property ID for property normalTexture_ST</summary>
        public static readonly int NormalTextureScaleTransformProperty = Shader.PropertyToID("normalTexture_ST");
        /// <summary>Shader property ID for property normalTexture_texCoord</summary>
        public static readonly int NormalTextureTexCoordProperty = Shader.PropertyToID("normalTexture_texCoord");
        /// <summary>Shader property ID for property normalTexture_scale</summary>
        public static readonly int NormalTextureScaleProperty = Shader.PropertyToID("normalTexture_scale");
        /// <summary>Shader property ID for property metallicFactor</summary>
        public static readonly int MetallicProperty = Shader.PropertyToID("metallicFactor");
        /// <summary>Shader property ID for property metallicRoughnessTexture</summary>
        public static readonly int MetallicRoughnessMapProperty = Shader.PropertyToID("metallicRoughnessTexture");
        /// <summary>Shader property ID for property metallicRoughnessTexture_ST</summary>
        public static readonly int MetallicRoughnessMapScaleTransformProperty = Shader.PropertyToID("metallicRoughnessTexture_ST");
        /// <summary>Shader property ID for property metallicRoughnessTexture_Rotation</summary>
        public static readonly int MetallicRoughnessMapRotationProperty = Shader.PropertyToID("metallicRoughnessTexture_Rotation");
        /// <summary>Shader property ID for property metallicRoughnessTexture_texCoord</summary>
        public static readonly int MetallicRoughnessMapUVChannelProperty = Shader.PropertyToID("metallicRoughnessTexture_texCoord");
        /// <summary>Shader property ID for property occlusionTexture</summary>
        public static readonly int OcclusionTextureProperty = Shader.PropertyToID("occlusionTexture");
        /// <summary>Shader property ID for property occlusionTexture_strength</summary>
        public static readonly int OcclusionTextureStrengthProperty = Shader.PropertyToID("occlusionTexture_strength");
        /// <summary>Shader property ID for property occlusionTexture_Rotation</summary>
        public static readonly int OcclusionTextureRotationProperty = Shader.PropertyToID("occlusionTexture_Rotation");
        /// <summary>Shader property ID for property occlusionTexture_ST</summary>
        public static readonly int OcclusionTextureScaleTransformProperty = Shader.PropertyToID("occlusionTexture_ST");
        /// <summary>Shader property ID for property occlusionTexture_texCoord</summary>
        public static readonly int OcclusionTextureTexCoordProperty = Shader.PropertyToID("occlusionTexture_texCoord");
        /// <summary>Shader property ID for property roughnessFactor</summary>
        public static readonly int RoughnessFactorProperty = Shader.PropertyToID("roughnessFactor");
        /// <summary>Shader property ID for property specularFactor</summary>
        public static readonly int SpecularFactorProperty = Shader.PropertyToID("specularFactor");
        /// <summary>Shader property ID for property specularGlossinessTexture</summary>
        public static readonly int SpecularGlossinessTextureProperty = Shader.PropertyToID("specularGlossinessTexture");
        /// <summary>Shader property ID for property specularGlossinessTexture_ST</summary>
        public static readonly int SpecularGlossinessTextureScaleTransformProperty = Shader.PropertyToID("specularGlossinessTexture_ST"); // TODO: Support in shader!
        /// <summary>Shader property ID for property specularGlossinessTexture_Rotation</summary>
        public static readonly int SpecularGlossinessTextureRotationProperty = Shader.PropertyToID("specularGlossinessTexture_Rotation"); // TODO: Support in shader!
        /// <summary>Shader property ID for property specularGlossinessTexture_texCoord</summary>
        public static readonly int SpecularGlossinessTextureTexCoordProperty = Shader.PropertyToID("specularGlossinessTexture_texCoord"); // TODO: Support in shader!
        /// <summary>Shader property ID for property _SrcBlend</summary>
        public static readonly int SrcBlendProperty = Shader.PropertyToID("_SrcBlend");
        /// <summary>Shader property ID for property _ZWrite</summary>
        public static readonly int ZWriteProperty = Shader.PropertyToID("_ZWrite");

        // ReSharper restore MemberCanBeProtected.Global
        // ReSharper restore MemberCanBePrivate.Global

        static IMaterialGenerator s_DefaultMaterialGenerator;

        static bool s_DefaultMaterialGenerated;
        static UnityEngine.Material s_DefaultMaterial;

        /// <summary>
        /// Provides the default material generator that's being used if no
        /// custom material generator was provided. The result depends on
        /// the currently used render pipeline.
        /// </summary>
        /// <returns>The default material generator</returns>
        /// <exception cref="Exception"></exception>
        public static IMaterialGenerator GetDefaultMaterialGenerator()
        {

            if (s_DefaultMaterialGenerator != null) return s_DefaultMaterialGenerator;

            var renderPipeline = RenderPipelineUtils.RenderPipeline;

            switch (renderPipeline)
            {
#if UNITY_SHADER_GRAPH_12_OR_NEWER && GLTFAST_BUILTIN_SHADER_GRAPH
                case RenderPipeline.BuiltIn:
                    s_DefaultMaterialGenerator = new BuiltInShaderGraphMaterialGenerator();
                    return s_DefaultMaterialGenerator;
#elif GLTFAST_BUILTIN_RP || UNITY_EDITOR
                case RenderPipeline.BuiltIn:
                    s_DefaultMaterialGenerator = new BuiltInMaterialGenerator();
                    return s_DefaultMaterialGenerator;
#endif
#if USING_URP
                case RenderPipeline.Universal:
                    var urpAsset = (UniversalRenderPipelineAsset) (QualitySettings.renderPipeline ? QualitySettings.renderPipeline : GraphicsSettings.defaultRenderPipeline);
                    s_DefaultMaterialGenerator = new UniversalRPMaterialGenerator(urpAsset);
                    return s_DefaultMaterialGenerator;
#endif
                case RenderPipeline.HighDefinition:
#if USING_HDRP
                    s_DefaultMaterialGenerator = new HighDefinitionRPMaterialGenerator();
                    return s_DefaultMaterialGenerator;
#endif
                default:
                    throw new Exception($"Could not determine default MaterialGenerator (render pipeline {renderPipeline})");
            }
        }

        /// <summary>
        /// Logger to be used for messaging. Can be null!
        /// </summary>
        protected ICodeLogger Logger { get; private set; }

        /// <inheritdoc />
        public UnityEngine.Material GetDefaultMaterial(bool pointsSupport = false)
        {
            if (pointsSupport)
            {
                Logger?.Warning(LogCode.TopologyPointsMaterialUnsupported);
            }
            if (!s_DefaultMaterialGenerated)
            {
                s_DefaultMaterial = GenerateDefaultMaterial(pointsSupport);
                s_DefaultMaterialGenerated = true;
            }
            return s_DefaultMaterial;
        }

        /// <summary>
        /// Creates a fallback material to be assigned to nodes without a material.
        /// </summary>
        /// <param name="pointsSupport">If true, material has to support meshes with points topology <seealso cref="MeshTopology.Points"/></param>
        /// <returns>fallback material</returns>
        protected abstract UnityEngine.Material GenerateDefaultMaterial(bool pointsSupport = false);

        /// <summary>
        /// Tries to load a shader and covers error handling.
        /// </summary>
        /// <param name="shaderName">The requested shader's name.</param>
        /// <param name="logger">Logger used for reporting errors.</param>
        /// <returns>Requested shader or null if it couldn't be loaded.</returns>
        protected static Shader FindShader(string shaderName, ICodeLogger logger)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                logger?.Error(LogCode.ShaderMissing, shaderName);
            }
            return shader;
        }

        /// <inheritdoc />
        public abstract UnityEngine.Material GenerateMaterial(
            Schema.Material gltfMaterial,
            IGltfReadable gltf,
            bool pointsSupport = false
            );

        /// <inheritdoc />
        public void SetLogger(ICodeLogger logger)
        {
            this.Logger = logger;
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
                    if (texture != null)
                    {
                        material.SetTexture(texturePropertyId, texture);
                        // TODO: Implement texture transform and UV channel selection for all texture types and remove
                        // this condition
                        if (scaleTransformPropertyId >= 0 && rotationPropertyId >= 0 && uvChannelPropertyId >= 0)
                        {
                            var isKtx = srcTexture.IsKtx;
                            TrySetTextureTransform(
                                textureInfo,
                                material,
                                texturePropertyId,
                                scaleTransformPropertyId,
                                rotationPropertyId,
                                uvChannelPropertyId,
                                isKtx
                                );
                        }
                        return true;
                    }
#if UNITY_IMAGECONVERSION
                    Logger?.Error(LogCode.TextureLoadFailed,textureIndex.ToString());
#endif
                }
                else
                {
                    Logger?.Error(LogCode.TextureNotFound, textureIndex.ToString());
                }
            }
            return false;
        }

        // protected static bool DifferentIndex(TextureInfo a, TextureInfo b) {
        //     return a != null && b != null && a.index>=0 && b.index>=0 && a.index != b.index;
        // }

        void TrySetTextureTransform(
            TextureInfo textureInfo,
            UnityEngine.Material material,
            int texturePropertyId,
            int scaleTransformPropertyId = -1,
            int rotationPropertyId = -1,
            int uvChannelPropertyId = -1,
            bool flipY = false
            )
        {
            var hasTransform = false;
            // Scale (x,y) and Transform (z,w)
            var textureScaleTranslation = new float4(
                1, 1,// scale
                0, 0 // translation
                );

            var texCoord = textureInfo.texCoord;

            if (textureInfo.extensions?.KHR_texture_transform != null)
            {
                hasTransform = true;
                var tt = textureInfo.extensions.KHR_texture_transform;
                if (tt.texCoord >= 0)
                {
                    texCoord = tt.texCoord;
                }

                if (tt.offset != null)
                {
                    textureScaleTranslation.z = tt.offset[0];
                    textureScaleTranslation.w = 1 - tt.offset[1];
                }
                if (tt.scale != null)
                {
                    textureScaleTranslation.x = tt.scale[0];
                    textureScaleTranslation.y = tt.scale[1];
                }
                if (tt.rotation != 0)
                {
                    var cos = math.cos(tt.rotation);
                    var sin = math.sin(tt.rotation);

                    var newRot = new Vector2(textureScaleTranslation.x * sin, textureScaleTranslation.y * -sin);

                    Assert.IsTrue(rotationPropertyId >= 0, "Texture rotation property invalid!");
                    material.SetVector(rotationPropertyId, newRot);

                    textureScaleTranslation.x *= cos;
                    textureScaleTranslation.y *= cos;

                    textureScaleTranslation.z -= newRot.y; // move offset to move rotation point (horizontally)
                }
                else
                {
                    // Make sure the rotation is properly zeroed
                    material.SetVector(rotationPropertyId, Vector4.zero);
                }

                textureScaleTranslation.w -= textureScaleTranslation.y; // move offset to move flip axis point (vertically)
            }

            if (texCoord != 0)
            {
                if (uvChannelPropertyId >= 0 && texCoord < 2f)
                {
                    material.EnableKeyword(UVChannelSelectKeyword);
                    material.SetFloat(uvChannelPropertyId, texCoord);
                }
                else
                {
                    Logger?.Error(LogCode.UVMulti, texCoord.ToString());
                }
            }

            if (flipY)
            {
                hasTransform = true;
                textureScaleTranslation.w = 1 - textureScaleTranslation.w; // flip offset in Y
                textureScaleTranslation.y = -textureScaleTranslation.y; // flip scale in Y
            }

            if (hasTransform)
            {
                material.EnableKeyword(TextureTransformKeyword);
            }

            material.SetTextureOffset(texturePropertyId, textureScaleTranslation.zw);
            material.SetTextureScale(texturePropertyId, textureScaleTranslation.xy);
            Assert.IsTrue(scaleTransformPropertyId >= 0, "Texture scale/transform property invalid!");
            material.SetVector(scaleTransformPropertyId, textureScaleTranslation);
        }

        /// <summary>
        /// Approximates Transmission material effect for Render Pipelines / Shaders where filtering the
        /// backbuffer is not possible.
        /// </summary>
        /// <param name="transmission">glTF transmission extension data</param>
        /// <param name="baseColorLinear">BaseColor reference. Alpha will be altered according to transmission</param>
        /// <returns>True when the transmission can be approximated with Premultiply mode. False if blending is better</returns>
        protected static bool TransmissionWorkaroundShaderMode(Transmission transmission, ref Color baseColorLinear)
        {
            var min = Mathf.Min(Mathf.Min(baseColorLinear.r, baseColorLinear.g), baseColorLinear.b);
            var max = baseColorLinear.maxColorComponent;
            if (max - min < .1f)
            {
                // R/G/B components don't diverge too much
                // -> white/grey/black-ish color
                // -> Approximation via Transparent mode should be close to real transmission
                baseColorLinear.a *= 1 - transmission.transmissionFactor;
                return true;
            }
            else
            {
                // Color is somewhat saturated
                // -> Fallback to Blend mode
                // -> Dial down transmissionFactor by 50% to avoid material completely disappearing
                // Shows at least some color tinting
                baseColorLinear.a *= 1 - transmission.transmissionFactor * 0.5f;

                // Premultiply color? Decided not to. I preferred vivid (but too bright) colors over desaturation effect.
                // baseColorLinear.r *= baseColorLinear.a;
                // baseColorLinear.g *= baseColorLinear.a;
                // baseColorLinear.b *= baseColorLinear.a;

                return false;
            }
        }
    }
}
