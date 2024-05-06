// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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

        /// <inheritdoc cref="MaterialProperty.AlphaCutoff" />
        [Obsolete("Use MaterialProperty.AlphaCutoff instead.")]
        public static readonly int AlphaCutoffProperty = MaterialProperty.AlphaCutoff;
        /// <inheritdoc cref="MaterialProperty.BaseColor"/>
        [Obsolete("Use MaterialProperty.BaseColor instead.")]
        public static readonly int BaseColorProperty = MaterialProperty.BaseColor;
        /// <inheritdoc cref="MaterialProperty.BaseColorTexture"/>
        [Obsolete("Use MaterialProperty.BaseColorTexture instead.")]
        public static readonly int BaseColorTextureProperty = MaterialProperty.BaseColorTexture;
        /// <inheritdoc cref="MaterialProperty.BaseColorTextureRotation"/>
        [Obsolete("Use MaterialProperty.BaseColorTextureRotation instead.")]
        public static readonly int BaseColorTextureRotationProperty = MaterialProperty.BaseColorTextureRotation;
        /// <inheritdoc cref="MaterialProperty.BaseColorTextureScaleTransform"/>
        [Obsolete("Use MaterialProperty.BaseColorTextureScaleTransform instead.")]
        public static readonly int BaseColorTextureScaleTransformProperty = MaterialProperty.BaseColorTextureScaleTransform;
        /// <inheritdoc cref="MaterialProperty.BaseColorTextureTexCoord"/>
        [Obsolete("Use MaterialProperty.BaseColorTextureTexCoord instead.")]
        public static readonly int BaseColorTextureTexCoordProperty = MaterialProperty.BaseColorTextureTexCoord;
        /// <inheritdoc cref="MaterialProperty.CullMode" />
        [Obsolete("Use MaterialProperty.CullMode instead.")]
        public static readonly int CullModeProperty = MaterialProperty.CullMode;
        /// <inheritdoc cref="MaterialProperty.Cull"/>
        [Obsolete("Use MaterialProperty.Cull instead.")]
        public static readonly int CullProperty = MaterialProperty.Cull;
        /// <inheritdoc cref="MaterialProperty.DstBlend" />
        [Obsolete("Use MaterialProperty.DstBlend instead.")]
        public static readonly int DstBlendProperty = MaterialProperty.DstBlend;
        /// <inheritdoc cref="MaterialProperty.DiffuseFactor" />
        [Obsolete("Use MaterialProperty.DiffuseFactor instead.")]
        public static readonly int DiffuseFactorProperty = MaterialProperty.DiffuseFactor;
        /// <inheritdoc cref="MaterialProperty.DiffuseTexture" />
        [Obsolete("Use MaterialProperty.DiffuseTexture instead.")]
        public static readonly int DiffuseTextureProperty = MaterialProperty.DiffuseTexture;
        /// <inheritdoc cref="MaterialProperty.DiffuseTextureScaleTransform" />
        [Obsolete("Use MaterialProperty.DiffuseTextureScaleTransform instead.")]
        public static readonly int DiffuseTextureScaleTransformProperty = MaterialProperty.DiffuseTextureScaleTransform;
        /// <inheritdoc cref="MaterialProperty.DiffuseTextureRotation" />
        [Obsolete("Use MaterialProperty.DiffuseTextureRotation instead.")]
        public static readonly int DiffuseTextureRotationProperty = MaterialProperty.DiffuseTextureRotation;
        /// <inheritdoc cref="MaterialProperty.DiffuseTextureTexCoord" />
        [Obsolete("Use MaterialProperty.DiffuseTextureTexCoord instead.")]
        public static readonly int DiffuseTextureTexCoordProperty = MaterialProperty.DiffuseTextureTexCoord;
        /// <inheritdoc cref="MaterialProperty.EmissiveFactor" />
        [Obsolete("Use MaterialProperty.EmissiveFactor instead.")]
        public static readonly int EmissiveFactorProperty = MaterialProperty.EmissiveFactor;
        /// <inheritdoc cref="MaterialProperty.EmissiveTexture" />
        [Obsolete("Use MaterialProperty.EmissiveTexture instead.")]
        public static readonly int EmissiveTextureProperty = MaterialProperty.EmissiveTexture;
        /// <inheritdoc cref="MaterialProperty.EmissiveTextureRotation" />
        [Obsolete("Use MaterialProperty.EmissiveTextureRotation instead.")]
        public static readonly int EmissiveTextureRotationProperty = MaterialProperty.EmissiveTextureRotation;
        /// <inheritdoc cref="MaterialProperty.EmissiveTextureScaleTransform" />
        [Obsolete("Use MaterialProperty.EmissiveTextureScaleTransform instead.")]
        public static readonly int EmissiveTextureScaleTransformProperty = MaterialProperty.EmissiveTextureScaleTransform;
        /// <inheritdoc cref="MaterialProperty.EmissiveTextureTexCoord" />
        [Obsolete("Use MaterialProperty.EmissiveTextureTexCoord instead.")]
        public static readonly int EmissiveTextureTexCoordProperty = MaterialProperty.EmissiveTextureTexCoord;
        /// <inheritdoc cref="MaterialProperty.GlossinessFactor" />
        [Obsolete("Use MaterialProperty.GlossinessFactor instead.")]
        public static readonly int GlossinessFactorProperty = MaterialProperty.GlossinessFactor;
        /// <inheritdoc cref="MaterialProperty.NormalTexture" />
        [Obsolete("Use MaterialProperty.NormalTexture instead.")]
        public static readonly int NormalTextureProperty = MaterialProperty.NormalTexture;
        /// <inheritdoc cref="MaterialProperty.NormalTextureRotation" />
        [Obsolete("Use MaterialProperty.NormalTextureRotation instead.")]
        public static readonly int NormalTextureRotationProperty = MaterialProperty.NormalTextureRotation;
        /// <inheritdoc cref="MaterialProperty.NormalTextureScaleTransform" />
        [Obsolete("Use MaterialProperty.NormalTextureScaleTransform instead.")]
        public static readonly int NormalTextureScaleTransformProperty = MaterialProperty.NormalTextureScaleTransform;
        /// <inheritdoc cref="MaterialProperty.NormalTextureTexCoord" />
        [Obsolete("Use MaterialProperty.NormalTextureTexCoord instead.")]
        public static readonly int NormalTextureTexCoordProperty = MaterialProperty.NormalTextureTexCoord;
        /// <inheritdoc cref="MaterialProperty.NormalTextureScale" />
        [Obsolete("Use MaterialProperty.NormalTextureScale instead.")]
        public static readonly int NormalTextureScaleProperty = MaterialProperty.NormalTextureScale;
        /// <inheritdoc cref="MaterialProperty.Metallic" />
        [Obsolete("Use MaterialProperty.Metallic instead.")]
        public static readonly int MetallicProperty = MaterialProperty.Metallic;
        /// <inheritdoc cref="MaterialProperty.MetallicRoughnessMap" />
        [Obsolete("Use MaterialProperty.MetallicRoughnessMap instead.")]
        public static readonly int MetallicRoughnessMapProperty = MaterialProperty.MetallicRoughnessMap;
        /// <inheritdoc cref="MaterialProperty.MetallicRoughnessMapScaleTransform" />
        [Obsolete("Use MaterialProperty.MetallicRoughnessMapScaleTransform instead.")]
        public static readonly int MetallicRoughnessMapScaleTransformProperty = MaterialProperty.MetallicRoughnessMapScaleTransform;
        /// <inheritdoc cref="MaterialProperty.MetallicRoughnessMapRotation" />
        [Obsolete("Use MaterialProperty.MetallicRoughnessMapRotation instead.")]
        public static readonly int MetallicRoughnessMapRotationProperty = MaterialProperty.MetallicRoughnessMapRotation;
        /// <inheritdoc cref="MaterialProperty.MetallicRoughnessMapTexCoord" />
        [Obsolete("Use MaterialProperty.MetallicRoughnessMapTexCoord instead.")]
        public static readonly int MetallicRoughnessMapUVChannelProperty = MaterialProperty.MetallicRoughnessMapTexCoord;
        /// <inheritdoc cref="MaterialProperty.OcclusionTexture" />
        [Obsolete("Use MaterialProperty.OcclusionTexture instead.")]
        public static readonly int OcclusionTextureProperty = MaterialProperty.OcclusionTexture;
        /// <inheritdoc cref="MaterialProperty.OcclusionTextureStrength" />
        [Obsolete("Use MaterialProperty.OcclusionTextureStrength instead.")]
        public static readonly int OcclusionTextureStrengthProperty = MaterialProperty.OcclusionTextureStrength;
        /// <inheritdoc cref="MaterialProperty.OcclusionTextureRotation" />
        [Obsolete("Use MaterialProperty.OcclusionTextureRotation instead.")]
        public static readonly int OcclusionTextureRotationProperty = MaterialProperty.OcclusionTextureRotation;
        /// <inheritdoc cref="MaterialProperty.OcclusionTextureScaleTransform" />
        [Obsolete("Use MaterialProperty.OcclusionTextureScaleTransform instead.")]
        public static readonly int OcclusionTextureScaleTransformProperty = MaterialProperty.OcclusionTextureScaleTransform;
        /// <inheritdoc cref="MaterialProperty.OcclusionTextureTexCoord" />
        [Obsolete("Use MaterialProperty.OcclusionTextureTexCoord instead.")]
        public static readonly int OcclusionTextureTexCoordProperty = MaterialProperty.OcclusionTextureTexCoord;
        /// <inheritdoc cref="MaterialProperty.RoughnessFactor" />
        [Obsolete("Use MaterialProperty.RoughnessFactor instead.")]
        public static readonly int RoughnessFactorProperty = MaterialProperty.RoughnessFactor;
        /// <inheritdoc cref="MaterialProperty.SpecularFactor" />
        [Obsolete("Use MaterialProperty.SpecularFactor instead.")]
        public static readonly int SpecularFactorProperty = MaterialProperty.SpecularFactor;
        /// <inheritdoc cref="MaterialProperty.SpecularGlossinessTexture" />
        [Obsolete("Use MaterialProperty.SpecularGlossinessTexture instead.")]
        public static readonly int SpecularGlossinessTextureProperty = MaterialProperty.SpecularGlossinessTexture;
        /// <inheritdoc cref="MaterialProperty.SpecularGlossinessTextureScaleTransform" />
        [Obsolete("Use MaterialProperty.SpecularGlossinessTextureScaleTransform instead.")]
        public static readonly int SpecularGlossinessTextureScaleTransformProperty = MaterialProperty.SpecularGlossinessTextureScaleTransform;
        /// <inheritdoc cref="MaterialProperty.SpecularGlossinessTextureRotation" />
        [Obsolete("Use MaterialProperty.SpecularGlossinessTextureRotation instead.")]
        public static readonly int SpecularGlossinessTextureRotationProperty = MaterialProperty.SpecularGlossinessTextureRotation;
        /// <inheritdoc cref="MaterialProperty.SpecularGlossinessTextureTexCoord" />
        [Obsolete("Use MaterialProperty.SpecularGlossinessTextureTexCoord instead.")]
        public static readonly int SpecularGlossinessTextureTexCoordProperty = MaterialProperty.SpecularGlossinessTextureTexCoord;
        /// <inheritdoc cref="MaterialProperty.SrcBlend" />
        [Obsolete("Use MaterialProperty.SrcBlend instead.")]
        public static readonly int SrcBlendProperty = MaterialProperty.SrcBlend;
        /// <inheritdoc cref="MaterialProperty.ZWrite" />
        [Obsolete("Use MaterialProperty.ZWrite instead.")]
        public static readonly int ZWriteProperty = MaterialProperty.ZWrite;

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
        /// <exception cref="InvalidOperationException">Is thrown when the default material generator couldn't be determined based on the current render pipeline.</exception>
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
                    throw new InvalidOperationException($"Could not determine default MaterialGenerator (render pipeline {renderPipeline})");
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
        /// <param name="pointsSupport">If true, material has to support meshes with <see cref="MeshTopology.Points">points</see> topology</param>
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
            MaterialBase gltfMaterial,
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
            TextureInfoBase textureInfo,
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
                            var flipY = gltf.IsTextureYFlipped(textureIndex);
                            TrySetTextureTransform(
                                textureInfo,
                                material,
                                texturePropertyId,
                                scaleTransformPropertyId,
                                rotationPropertyId,
                                uvChannelPropertyId,
                                flipY
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
            TextureInfoBase textureInfo,
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

            if (textureInfo.Extensions?.KHR_texture_transform != null)
            {
                hasTransform = true;
                var tt = textureInfo.Extensions.KHR_texture_transform;
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
