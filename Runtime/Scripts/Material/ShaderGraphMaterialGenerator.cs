// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_URP || USING_HDRP || GLTFAST_BUILTIN_SHADER_GRAPH
#define GLTFAST_SHADER_GRAPH
#endif

#if GLTFAST_SHADER_GRAPH

using System;

using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Objects;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using Color = Unity.Cloud.Gltfast.Objects.Color;
using Material = UnityEngine.Material;
using GltfMaterial = Unity.Cloud.Gltfast.Objects.Material;

namespace Unity.Cloud.Gltfast.Materials
{

    using AlphaMode = AlphaMode;

    [MovedFrom(true, sourceNamespace: "GLTFast.Materials", sourceAssembly: "glTFast")]
    public class ShaderGraphMaterialGenerator : MaterialGenerator
    {

        [Flags]
        protected enum ShaderMode
        {
            Opaque = 0,
            Blend = 1 << 0,
            Premultiply = 1 << 1,
        }

        [Flags]
        protected enum MetallicShaderFeatures
        {
            Default = 0,
            // Bits 0-1 are the shader modes
            ModeMask = 0x3,
            ModeOpaque = 0,
            ModeFade = 1,
            ModeTransparent = 1 << 1,
            // Other flags
            DoubleSided = 1 << 2,
            ClearCoat = 1 << 3,
            Sheen = 1 << 4,
        }


        [Flags]
        protected enum SpecularShaderFeatures
        {
            Default = 0,
            AlphaBlend = 1 << 1,
            DoubleSided = 1 << 2
        }

        // These are used in HighDefinitionRPMaterialGenerator.cs for older versions of HDRP
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable MemberCanBeProtected.Global

#if UNITY_EDITOR
        /// <summary>GUID of the shader graph used for PBR metallic/roughness materials</summary>
        const string k_MetallicShaderGuid = "b9d29dfa1474148e792ac720cbd45122";
        /// <summary>GUID of the shader graph used for unlit materials</summary>
        const string k_UnlitShaderGuid = "c87047c884d9843f5b0f4cce282aa760";
        /// <summary>GUID of the shader graph used for PBR specular/glossiness materials</summary>
        const string k_SpecularShaderGuid = "9a07dad0f3c4e43ff8312e3b5fa42300";
#endif

        /// <summary>Name of the shader graph used for PBR metallic/roughness materials</summary>
        public const string MetallicShader = "glTF-pbrMetallicRoughness";
        /// <summary>Name of the shader graph used for unlit materials</summary>
        public const string UnlitShader = "glTF-unlit";
        /// <summary>Name of the shader graph used for PBR specular/glossiness materials</summary>
        public const string SpecularShader = "glTF-pbrSpecularGlossiness";

        /// <summary>MotionVector shader tag name</summary>
        public const string MotionVectorTag = "MotionVector";
        /// <summary>MotionVector shader tag User value</summary>
        public const string MotionVectorUser = "User";

        /// <summary>MotionVector shader pass name</summary>
        public const string MotionVectorsPass = "MOTIONVECTORS";

        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore MemberCanBeProtected.Global

#if UNITY_EDITOR
        const string k_ShaderPathPrefix = "Packages/" + GltfGlobals.GltfPackageName + "/Runtime/Shader/";
#endif

        const string k_ShaderGraphsPrefix = "Shader Graphs/";

        const string k_OcclusionKeyword = "_OCCLUSION";
        const string k_EmissiveKeyword = "_EMISSIVE";
        const string k_SpecularSetupKeyword = "_SPECULAR_SETUP";

        static readonly int k_BaseMapPropId = Shader.PropertyToID("baseColorTexture");
        static readonly int k_BaseMapScaleTransformPropId = Shader.PropertyToID("baseColorTexture_ST"); //TODO: support in shader!
        static readonly int k_BaseMapRotationPropId = Shader.PropertyToID("baseColorTexture_Rotation"); //TODO; support in shader!
        static readonly int k_BaseMapUVChannelPropId = Shader.PropertyToID("baseColorTexture_texCoord"); //TODO; support in shader!

        // ReSharper disable MemberCanBeProtected.Global
        /// <summary>Shader property ID for property transmissionFactor</summary>
        public static readonly int TransmissionFactorProperty = Shader.PropertyToID("transmissionFactor");
        /// <summary>Shader property ID for property transmissionTexture</summary>
        public static readonly int TransmissionTextureProperty = Shader.PropertyToID("transmissionTexture");
        // ReSharper restore MemberCanBeProtected.Global

        // /// <summary>Shader property ID for property transmissionTexture_texCoord</summary>
        // public static readonly int TransmissionTextureScaleTransformProperty = Shader.PropertyToID("transmissionTexture_texCoord");
        // /// <summary>Shader property ID for property transmissionTexture_Rotation</summary>
        // public static readonly int TransmissionTextureRotationProperty = Shader.PropertyToID("transmissionTexture_Rotation");
        // /// <summary>Shader property ID for property transmissionTexture_texCoord</summary>
        // public static readonly int TransmissionTextureUVChannelProperty = Shader.PropertyToID("transmissionTexture_texCoord");

        /// <summary>Shader property ID for property clearcoatFactor</summary>
        public static readonly int ClearcoatProperty = Shader.PropertyToID("clearcoatFactor");
        /// <summary>Shader property ID for property clearcoatTexture</summary>
        public static readonly int ClearcoatTextureProperty = Shader.PropertyToID("clearcoatTexture");
        /// <summary>Shader property ID for property clearcoatTexture_ST</summary>
        public static readonly int ClearcoatTextureScaleTransformProperty = Shader.PropertyToID("clearcoatTexture_ST");
        /// <summary>Shader property ID for property clearcoatTexture_Rotation</summary>
        public static readonly int ClearcoatTextureRotationProperty = Shader.PropertyToID("clearcoatTexture_Rotation");
        /// <summary>Shader property ID for property clearcoatTexture_texCoord</summary>
        public static readonly int ClearcoatTextureTexCoordProperty = Shader.PropertyToID("clearcoatTexture_texCoord");
        /// <summary>Shader property ID for property clearcoatRoughnessFactor</summary>
        public static readonly int ClearcoatRoughnessProperty = Shader.PropertyToID("clearcoatRoughnessFactor");
        /// <summary>Shader property ID for property clearcoatRoughnessTexture</summary>
        public static readonly int ClearcoatRoughnessTextureProperty = Shader.PropertyToID("clearcoatRoughnessTexture");
        /// <summary>Shader property ID for property clearcoatRoughnessTexture_ST</summary>
        public static readonly int ClearcoatRoughnessTextureScaleTransformProperty = Shader.PropertyToID("clearcoatRoughnessTexture_ST");
        /// <summary>Shader property ID for property clearcoatRoughnessTexture_Rotation</summary>
        public static readonly int ClearcoatRoughnessTextureRotationProperty = Shader.PropertyToID("clearcoatRoughnessTexture_Rotation");
        /// <summary>Shader property ID for property clearcoatRoughnessTexture_texCoord</summary>
        public static readonly int ClearcoatRoughnessTextureTexCoordProperty = Shader.PropertyToID("clearcoatRoughnessTexture_texCoord");
        /// <summary>Shader property ID for property clearcoatNormalTexture</summary>
        public static readonly int ClearcoatNormalTextureProperty = Shader.PropertyToID("clearcoatNormalTexture");
        /// <summary>Shader property ID for property clearcoatNormalTexture_Scale</summary>
        public static readonly int ClearcoatNormalTextureScaleProperty = Shader.PropertyToID("clearcoatNormalTexture_Scale");
        /// <summary>Shader property ID for property clearcoatNormalTexture_ST</summary>
        public static readonly int ClearcoatNormalTextureScaleTransformProperty = Shader.PropertyToID("clearcoatNormalTexture_ST");
        /// <summary>Shader property ID for property clearcoatNormalTextureRotation</summary>
        public static readonly int ClearcoatNormalTextureRotationProperty = Shader.PropertyToID("clearcoatNormalTextureRotation");
        /// <summary>Shader property ID for property clearcoatNormalTexture_texCoord</summary>
        public static readonly int ClearcoatNormalTextureTexCoordProperty = Shader.PropertyToID("clearcoatNormalTexture_texCoord");
        /// <summary>Shader keyword _CLEARCOAT</summary>
        const string k_ClearcoatKeyword = "_CLEARCOAT";

        // ReSharper disable MemberCanBeProtected.Global
        // const string KW_DISABLE_DECALS = "_DISABLE_DECALS";
        /// <summary>Shader keyword _DISABLE_SSR_TRANSPARENT</summary>
        public const string DisableSsrTransparentKeyword = "_DISABLE_SSR_TRANSPARENT";
        /// <summary>Shader keyword _ENABLE_FOG_ON_TRANSPARENT</summary>
        public const string EnableFogOnTransparentKeyword = "_ENABLE_FOG_ON_TRANSPARENT";
        /// <summary>Shader keyword _SURFACE_TYPE_TRANSPARENT</summary>
        public const string SurfaceTypeTransparentKeyword = "_SURFACE_TYPE_TRANSPARENT";

        /// <summary>Shader pass TransparentDepthPrepass</summary>
        public const string ShaderPassTransparentDepthPrepass = "TransparentDepthPrepass";
        /// <summary>Shader pass TransparentDepthPostpass</summary>
        public const string ShaderPassTransparentDepthPostpass = "TransparentDepthPostpass";
        /// <summary>Shader pass TransparentBackface</summary>
        public const string ShaderPassTransparentBackface = "TransparentBackface";
        /// <summary>Shader pass RayTracingPrepass</summary>
        public const string ShaderPassRayTracingPrepass = "RayTracingPrepass";
        /// <summary>Shader pass DepthOnly</summary>
        public const string ShaderPassDepthOnlyPass = "DepthOnly";

        /// <summary>Shader property ID for property _AlphaDstBlend</summary>
        public static readonly int AlphaDstBlendProperty = Shader.PropertyToID("_AlphaDstBlend");
        /// <summary>Shader property ID for property _ZTestGBuffer</summary>
        public static readonly int ZTestGBufferProperty = Shader.PropertyToID("_ZTestGBuffer");
        // ReSharper restore MemberCanBeProtected.Global

        static Shader s_MetallicShader;
        static Shader s_SpecularShader;
        static Shader s_UnlitShader;

        static bool s_MetallicShaderQueried;
        static bool s_SpecularShaderQueried;
        static bool s_UnlitShaderQueried;

        /// <inheritdoc />
        protected override Material GenerateDefaultMaterial(bool pointsSupport = false)
        {
            if (pointsSupport)
            {
                Logger?.Warning(LogCode.TopologyPointsMaterialUnsupported);
            }
            var defaultMaterial = GetMetallicMaterial(MetallicShaderFeatures.Default);
            if (defaultMaterial != null)
            {
                defaultMaterial.name = DefaultMaterialName;
            }
            return defaultMaterial;
        }

        /// <inheritdoc />
        public override Material GenerateMaterial(
            GltfMaterial gltfMaterial,
            IGltfReadable gltf,
            bool pointsSupport = false
            )
        {
            if (pointsSupport)
            {
                Logger?.Warning(LogCode.TopologyPointsMaterialUnsupported);
            }

            Material material;

            MaterialType? materialType;
            ShaderMode shaderMode = ShaderMode.Opaque;

            bool isUnlit = gltfMaterial.Extensions?.Unlit != null;
            bool isSpecularGlossiness = gltfMaterial.Extensions?.PbrSpecularGlossiness != null;

            if (isUnlit)
            {
                material = GetUnlitMaterial(gltfMaterial);
                materialType = MaterialType.Unlit;
                shaderMode = gltfMaterial.AlphaMode == AlphaMode.Blend ? ShaderMode.Blend : ShaderMode.Opaque;
            }
            else if (isSpecularGlossiness)
            {
                materialType = MaterialType.SpecularGlossiness;
                var specularShaderFeatures = GetSpecularShaderFeatures(gltfMaterial);
                material = GetSpecularMaterial(specularShaderFeatures);
                material?.EnableKeyword(k_SpecularSetupKeyword);
                if ((specularShaderFeatures & SpecularShaderFeatures.AlphaBlend) != 0)
                {
                    shaderMode = ShaderMode.Blend;
                }
            }
            else
            {
                materialType = MaterialType.MetallicRoughness;
                var metallicShaderFeatures = GetMetallicShaderFeatures(gltfMaterial);
                material = GetMetallicMaterial(metallicShaderFeatures);
                shaderMode = (ShaderMode)(metallicShaderFeatures & MetallicShaderFeatures.ModeMask);
            }

            if (material == null) return null;

            material.name = gltfMaterial.Name;

            UnityEngine.Color baseColorLinear = UnityEngine.Color.white;
            RenderQueue? renderQueue = null;

            //added support for KHR_materials_pbrSpecularGlossiness
            if (gltfMaterial.Extensions != null)
            {
                PbrSpecularGlossiness specGloss = gltfMaterial.Extensions.PbrSpecularGlossiness;
                if (specGloss != null)
                {
                    baseColorLinear = specGloss.DiffuseFactor;
                    material.SetVector(MaterialProperty.DiffuseFactor, ((UnityEngine.Color)specGloss.DiffuseFactor).gamma);
                    material.SetVector(MaterialProperty.SpecularFactor, (UnityEngine.Color)specGloss.SpecularFactor);
                    material.SetFloat(MaterialProperty.GlossinessFactor, specGloss.GlossinessFactor);

                    TrySetTexture(
                        specGloss.DiffuseTexture,
                        material,
                        gltf,
                        MaterialProperty.DiffuseTexture,
                        MaterialProperty.DiffuseTextureScaleTransform,
                        MaterialProperty.DiffuseTextureRotation,
                        MaterialProperty.DiffuseTextureTexCoord
                        );

                    if (TrySetTexture(
                        specGloss.SpecularGlossinessTexture,
                        material,
                        gltf,
                        MaterialProperty.SpecularGlossinessTexture,
                        MaterialProperty.SpecularGlossinessTextureScaleTransform,
                        MaterialProperty.SpecularGlossinessTextureRotation,
                        MaterialProperty.SpecularGlossinessTextureTexCoord
                        ))
                    {
                        // material.EnableKeyword();
                    }
                }
            }

            if (gltfMaterial.PbrMetallicRoughness != null
                // If there's a specular-glossiness extension, ignore metallic-roughness
                // (according to extension specification)
                && gltfMaterial.Extensions?.PbrSpecularGlossiness == null)
            {
                baseColorLinear = gltfMaterial.PbrMetallicRoughness.BaseColorFactor;

                if (materialType != MaterialType.SpecularGlossiness)
                {
                    // baseColorTexture can be used by both MetallicRoughness AND Unlit materials
                    TrySetTexture(
                        gltfMaterial.PbrMetallicRoughness.BaseColorTexture,
                        material,
                        gltf,
                        k_BaseMapPropId,
                        k_BaseMapScaleTransformPropId,
                        k_BaseMapRotationPropId,
                        k_BaseMapUVChannelPropId
                        );
                }

                if (materialType == MaterialType.MetallicRoughness)
                {
                    material.SetFloat(MaterialProperty.Metallic, gltfMaterial.PbrMetallicRoughness.MetallicFactor);
                    material.SetFloat(MaterialProperty.RoughnessFactor, gltfMaterial.PbrMetallicRoughness.RoughnessFactor);

                    if (TrySetTexture(
                        gltfMaterial.PbrMetallicRoughness.MetallicRoughnessTexture,
                        material,
                        gltf,
                        MaterialProperty.MetallicRoughnessMap,
                        MaterialProperty.MetallicRoughnessMapScaleTransform,
                        MaterialProperty.MetallicRoughnessMapRotation,
                        MaterialProperty.MetallicRoughnessMapTexCoord
                        ))
                    {
                        // material.EnableKeyword(KW_METALLIC_ROUGHNESS_MAP);
                    }

                    // TODO: When the occlusionTexture equals the metallicRoughnessTexture, we could sample just once instead of twice.
                    // if (!DifferentIndex(gltfMaterial.occlusionTexture,gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture)) {
                    //    ...
                    // }
                }
            }

            if (TrySetTexture(
                gltfMaterial.NormalTexture,
                material,
                gltf,
                MaterialProperty.NormalTexture,
                MaterialProperty.NormalTextureScaleTransform,
                MaterialProperty.NormalTextureRotation,
                MaterialProperty.NormalTextureTexCoord
                ))
            {
                // material.EnableKeyword(ShaderKeyword.normalMap);
                material.SetFloat(MaterialProperty.NormalTextureScale, gltfMaterial.NormalTexture.Scale);
            }

            if (TrySetTexture(
                gltfMaterial.OcclusionTexture,
                material,
                gltf,
                MaterialProperty.OcclusionTexture,
                MaterialProperty.OcclusionTextureScaleTransform,
                MaterialProperty.OcclusionTextureRotation,
                MaterialProperty.OcclusionTextureTexCoord
                ))
            {
                material.EnableKeyword(k_OcclusionKeyword);
                material.SetFloat(MaterialProperty.OcclusionTextureStrength, gltfMaterial.OcclusionTexture.Strength);
            }

            if (TrySetTexture(
                gltfMaterial.EmissiveTexture,
                material,
                gltf,
                MaterialProperty.EmissiveTexture,
                MaterialProperty.EmissiveTextureScaleTransform,
                MaterialProperty.EmissiveTextureRotation,
                MaterialProperty.EmissiveTextureTexCoord
                ))
            {
                material.EnableKeyword(k_EmissiveKeyword);
            }

            if (gltfMaterial.Extensions != null)
            {

                // Transmission - Approximation
                var transmission = gltfMaterial.Extensions.Transmission;
                if (transmission != null)
                {
                    renderQueue = ApplyTransmission(ref baseColorLinear, gltf, transmission, material, null);
                }
            }

            if (gltfMaterial.AlphaMode == AlphaMode.Mask)
            {
                SetAlphaModeMask(gltfMaterial, material);
#if USING_HDRP
                renderQueue = gltfMaterial.Extensions?.Unlit == null
                    ? RenderQueue.AlphaTest
                    : RenderQueue.Transparent;
#else
                renderQueue = RenderQueue.AlphaTest;
#endif
            }
            else
            {
                material.SetFloat(MaterialProperty.AlphaCutoff, 0);
                // double sided opaque would make errors in HDRP 7.3 otherwise
                material.SetOverrideTag(MotionVectorTag, MotionVectorUser);
                material.SetShaderPassEnabled(MotionVectorsPass, false);
            }
            if (!renderQueue.HasValue)
            {
                if (shaderMode == ShaderMode.Opaque)
                {
                    renderQueue = gltfMaterial.AlphaMode == AlphaMode.Mask
                        ? RenderQueue.AlphaTest
                        : RenderQueue.Geometry;
                }
                else
                {
                    renderQueue = RenderQueue.Transparent;
                }
            }

            material.renderQueue = (int)renderQueue.Value;

            if (gltfMaterial.DoubleSided)
            {
                SetDoubleSided(gltfMaterial, material);
            }

            switch (shaderMode)
            {
                case ShaderMode.Opaque:
                    SetShaderModeOpaque(gltfMaterial, material);
                    break;
                case ShaderMode.Blend:
                    SetShaderModeBlend(gltfMaterial, material);
                    break;
                case ShaderMode.Premultiply:
                    SetShaderModePremultiply(gltfMaterial, material);
                    break;
            }

            material.SetVector(MaterialProperty.BaseColor, baseColorLinear.gamma);

            if (gltfMaterial.EmissiveFactor != Color.Black)
            {
                material.SetColor(MaterialProperty.EmissiveFactor, gltfMaterial.EmissiveFactor);
                material.EnableKeyword(k_EmissiveKeyword);
            }

            if (gltfMaterial.Extensions?.Clearcoat?.ClearcoatFactor > 0)
            {
                var clearcoat = gltfMaterial.Extensions.Clearcoat;
                material.SetFloat(ClearcoatProperty, clearcoat.ClearcoatFactor);
                TrySetTexture(clearcoat.ClearcoatTexture,
                    material,
                    gltf,
                    ClearcoatTextureProperty,
                    ClearcoatTextureScaleTransformProperty,
                    ClearcoatTextureRotationProperty,
                    ClearcoatTextureTexCoordProperty);
                material.SetFloat(ClearcoatRoughnessProperty, clearcoat.ClearcoatRoughnessFactor);
                material.EnableKeyword(k_ClearcoatKeyword);
                TrySetTexture(clearcoat.ClearcoatRoughnessTexture,
                    material,
                    gltf,
                    ClearcoatRoughnessTextureProperty,
                    ClearcoatRoughnessTextureScaleTransformProperty,
                    ClearcoatRoughnessTextureRotationProperty,
                    ClearcoatRoughnessTextureTexCoordProperty);
                if (TrySetTexture(clearcoat.ClearcoatNormalTexture,
                        material,
                        gltf,
                        ClearcoatNormalTextureProperty,
                        ClearcoatNormalTextureScaleTransformProperty,
                        ClearcoatNormalTextureRotationProperty,
                        ClearcoatNormalTextureTexCoordProperty))
                {
                    material.SetFloat(ClearcoatNormalTextureScaleProperty, clearcoat.ClearcoatNormalTexture.Scale);
                }
            }

            return material;
        }

        Material GetMetallicMaterial(MetallicShaderFeatures metallicShaderFeatures)
        {
            Shader shader = GetMetallicShader(metallicShaderFeatures);
            if (shader == null)
            {
                return null;
            }
            var mat = new Material(shader);
#if UNITY_EDITOR
            mat.doubleSidedGI = (metallicShaderFeatures & MetallicShaderFeatures.DoubleSided) != 0;
#endif
            return mat;
        }

        Material GetUnlitMaterial(GltfMaterial gltfMaterial)
        {
            Shader shader = GetUnlitShader(gltfMaterial);
            if (shader == null)
            {
                return null;
            }
            var mat = new Material(shader);
#if UNITY_EDITOR
            mat.doubleSidedGI = gltfMaterial.DoubleSided;
#endif
            return mat;
        }

        Material GetSpecularMaterial(SpecularShaderFeatures features)
        {
            var shader = GetSpecularShader(features);
            if (shader == null)
            {
                return null;
            }
            var mat = new Material(shader);
#if UNITY_EDITOR
            mat.doubleSidedGI = (features & SpecularShaderFeatures.DoubleSided) != 0;
#endif
            return mat;
        }

        // ReSharper disable once UnusedParameter.Local
        protected virtual Shader GetMetallicShader(MetallicShaderFeatures features)
        {
            if (!s_MetallicShaderQueried)
            {
#if UNITY_EDITOR
                s_MetallicShader = LoadShaderByGuid(new GUID(k_MetallicShaderGuid));
#else
                s_MetallicShader = LoadShaderByName(MetallicShader);
#endif
                s_MetallicShaderQueried = true;
            }
            return s_MetallicShader;
        }

        // ReSharper disable once UnusedParameter.Local
        Shader GetUnlitShader(GltfMaterial gltfMaterial)
        {
            if (!s_UnlitShaderQueried)
            {
#if UNITY_EDITOR
                s_UnlitShader = LoadShaderByGuid(new GUID(k_UnlitShaderGuid));
#else
                s_UnlitShader = LoadShaderByName(UnlitShader);
#endif
                s_UnlitShaderQueried = true;
            }
            return s_UnlitShader;
        }


        // ReSharper disable once UnusedParameter.Local
        Shader GetSpecularShader(SpecularShaderFeatures features)
        {
            if (!s_SpecularShaderQueried)
            {
#if UNITY_EDITOR
                s_SpecularShader = LoadShaderByGuid(new GUID(k_SpecularShaderGuid));
#else
                s_SpecularShader = LoadShaderByName(SpecularShader);
#endif
                s_SpecularShaderQueried = true;
            }
            return s_SpecularShader;
        }

#if UNITY_EDITOR
        protected static Shader LoadShaderByGuid(GUID guid)
        {
            return AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guid));
        }
#endif

        /// <summary>
        /// Loads a shader by name and does eventual error reporting.
        /// </summary>
        /// <param name="shaderName">The requested shader's name.</param>
        /// <returns>Requested shader or null if it couldn't be loaded.</returns>
        protected virtual Shader LoadShaderByName(string shaderName)
        {
#if UNITY_EDITOR
            var shaderPath = $"{k_ShaderPathPrefix}{shaderName}.shadergraph";
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            if (shader == null)
            {
                Logger?.Error(LogCode.ShaderMissing, shaderPath);
            }
            return shader;
#else
            return FindShader($"{k_ShaderGraphsPrefix}{shaderName}");
#endif
        }

        protected virtual void SetDoubleSided(GltfMaterial gltfMaterial, Material material)
        {
            material.doubleSidedGI = true;
        }

        protected virtual void SetAlphaModeMask(GltfMaterial gltfMaterial, Material material)
        {
            material.SetFloat(MaterialProperty.AlphaCutoff, gltfMaterial.AlphaCutoff);
            material.EnableKeyword(AlphaTestOnKeyword);
            material.SetOverrideTag(RenderTypeTag, TransparentCutoutRenderType);
            material.SetFloat(ZTestGBufferProperty, (int)CompareFunction.Equal); //3
        }

        protected virtual void SetShaderModeOpaque(GltfMaterial gltfMaterial, Material material) { }
        protected virtual void SetShaderModeBlend(GltfMaterial gltfMaterial, Material material) { }
        protected virtual void SetShaderModePremultiply(GltfMaterial gltfMaterial, Material material) { }

        protected virtual RenderQueue? ApplyTransmission(
            ref UnityEngine.Color baseColorLinear,
            IGltfReadable gltf,
            Transmission transmission,
            Material material,
            RenderQueue? renderQueue
            )
        {
#if UNITY_EDITOR
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Logger?.Warning(LogCode.MaterialTransmissionApproxUrp);
#endif
            // Correct transmission is not supported in Built-In renderer
            // This is an approximation for some corner cases
            if (transmission.TransmissionFactor > 0f
                && (transmission.TransmissionTexture == null
                    || transmission.TransmissionTexture.Index < 0)
                )
            {
                TransmissionWorkaroundShaderMode(transmission, ref baseColorLinear);
            }
            return renderQueue;
        }

        protected MetallicShaderFeatures GetMetallicShaderFeatures(GltfMaterial gltfMaterial)
        {

            var feature = MetallicShaderFeatures.Default;
            ShaderMode? sm = null;

            if (gltfMaterial.Extensions != null)
            {

                if (gltfMaterial.Extensions.Clearcoat != null &&
                    gltfMaterial.Extensions.Clearcoat.ClearcoatFactor > 0) feature |= MetallicShaderFeatures.ClearCoat;
                if (gltfMaterial.Extensions.Sheen != null &&
                    gltfMaterial.Extensions.Sheen.SheenColorFactor.MaxColorComponent > 0) feature |= MetallicShaderFeatures.Sheen;

                if (
                    gltfMaterial.Extensions.Transmission != null
                    && gltfMaterial.Extensions.Transmission.TransmissionFactor > 0
                )
                {
                    sm = ApplyTransmissionShaderFeatures(gltfMaterial);
                }
            }

            if (gltfMaterial.DoubleSided) feature |= MetallicShaderFeatures.DoubleSided;

            if (!sm.HasValue)
            {
                sm = gltfMaterial.AlphaMode == AlphaMode.Blend ? ShaderMode.Blend : ShaderMode.Opaque;
            }

            feature |= (MetallicShaderFeatures)sm;

            return feature;
        }

        protected virtual ShaderMode? ApplyTransmissionShaderFeatures(GltfMaterial gltfMaterial)
        {
            // Makeshift approximation
            UnityEngine.Color baseColorLinear = UnityEngine.Color.white;
            var premultiply = TransmissionWorkaroundShaderMode(
                gltfMaterial.Extensions.Transmission,
                ref baseColorLinear
                );
            ShaderMode? sm = premultiply ? ShaderMode.Premultiply : ShaderMode.Blend;
            return sm;
        }

        static SpecularShaderFeatures GetSpecularShaderFeatures(GltfMaterial gltfMaterial)
        {

            var feature = SpecularShaderFeatures.Default;
            if (gltfMaterial.DoubleSided) feature |= SpecularShaderFeatures.DoubleSided;

            if (gltfMaterial.AlphaMode == AlphaMode.Blend)
            {
                feature |= SpecularShaderFeatures.AlphaBlend;
            }
            return feature;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsOnLoad()
        {
            // Reset static state
            s_MetallicShader = null;
            s_SpecularShader = null;
            s_UnlitShader = null;
            s_MetallicShaderQueried = false;
            s_SpecularShaderQueried = false;
            s_UnlitShaderQueried = false;
        }
#endif
    }
}
#endif // GLTFAST_SHADER_GRAPH
