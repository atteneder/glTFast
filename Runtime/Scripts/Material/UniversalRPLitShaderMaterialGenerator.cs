// Based on glTFast MaterialGenerator source. Copyright (c) 2020-2022 Andreas Atteneder. Apache license

// Modifications Copyright 2022 Spatial
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

#if USING_URP

using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

using GLTFast.Schema;

using Material = UnityEngine.Material;
using AlphaMode = GLTFast.Schema.Material.AlphaMode;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GLTFast.Materials
{
    public class UniversalRPLitShaderMaterialGenerator : MaterialGenerator
    {
        // Lit shader metallic or specular mode. Copied from GLTFLitGUI(LitGUI).
        public enum WorkflowMode
        {
            Specular = 0,
            Metallic
        }

        // Lit shader surface type. Copied from BaseShaderGUI.
        public enum SurfaceType
        {
            Opaque = 0,
            Transparent = 1
        }

        // Lit shader blend modes. Copied from BaseShaderGUI.
        public enum BlendMode
        {
            Alpha = 0,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Premultiply = 1, // Physically plausible transparency mode, implemented as alpha pre-multiply
            Additive = 2,
            Multiply = 3
        }

        // glTFast defined enums
        [Flags]
        public enum ShaderMode
        {
            Opaque = 0,
            Blend = 1,
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

        static bool supportsCameraOpaqueTexture;

        public UniversalRPLitShaderMaterialGenerator(UniversalRenderPipelineAsset renderPipelineAsset)
        {
            supportsCameraOpaqueTexture = renderPipelineAsset.supportsCameraOpaqueTexture;
        }

        #region Keywords
        private const string KW_ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";
        private const string KW_EMISSION = "_EMISSION";
        private const string KW_METALLIC_ROUGHNESS_MAP = "_METALLICSPECGLOSSMAP";
        private const string KW_OCCLUSION_MAP = "_OCCLUSIONMAP";
        private const string KW_SPECULAR_SETUP = "_SPECULAR_SETUP";
        // private const string KW_TRANSMISSION_BLUR = "_TRANSMISSION_BLUR";
        #endregion


        #region Properties
        private const string TAG_MOTION_VECTOR = "MotionVector";
        private const string TAG_MOTION_VECTOR_USER = "User";

        private const string k_MotionVectorsPass = "MOTIONVECTORS";

        private static readonly int workflowModePropId = Shader.PropertyToID("_WorkflowMode");
        private static readonly int surfaceTypePropId = Shader.PropertyToID("_Surface");
        private static readonly int blendModePropId = Shader.PropertyToID("_Blend");
        private static readonly int alphaClipPropId = Shader.PropertyToID("_AlphaClip");

        private static readonly int baseColorPropId = Shader.PropertyToID("_BaseColor");
        private static readonly int baseMapPropId = Shader.PropertyToID("_BaseMap");
        private static readonly int baseMapScaleTransformPropId = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int baseMapRotationPropId = Shader.PropertyToID("_BaseMapRotation");
        private static readonly int baseMapUVChannelPropId = Shader.PropertyToID("_BaseMapUVChannel");
        private static readonly int metallicRoughnessMapPropId = Shader.PropertyToID("_MetallicGlossMap");
        private static readonly int metallicRoughnessMapScaleTransformPropId = Shader.PropertyToID("_MetallicGlossMap_ST");
        private static readonly int metallicRoughnessMapRotationPropId = Shader.PropertyToID("_MetallicGlossMapRotation");
        private static readonly int metallicRoughnessMapUVChannelPropId = Shader.PropertyToID("_MetallicGlossMapUVChannel");
        private static readonly int specularFactorPropId = Shader.PropertyToID("_SpecularFactor");
        private static readonly int smoothnessPropId = Shader.PropertyToID("_Smoothness");
        private static readonly int transmissionPropId = Shader.PropertyToID("_Transmission");
        private static readonly int transmissionFactorPropId = Shader.PropertyToID("_TransmissionFactor");
        private static readonly int transmissionTexturePropId = Shader.PropertyToID("_TransmissionMap");
        private static readonly int transmissionTextureScaleTransformPropId = Shader.PropertyToID("_TransmissionMap_ST");
        private static readonly int transmissionTextureRotationPropId = Shader.PropertyToID("_TransmissionMapRotation");
        private static readonly int transmissionTextureUVChannelPropId = Shader.PropertyToID("_TransmissionMapUVChannel");
        #endregion


#if UNITY_EDITOR
        const string SHADER_PATH_PREFIX = "Packages/com.atteneder.gltfast/Runtime/Shader/URP/";
        const string SHADER_PATH_URP_LIT = "glTF-urp-lit.shader";
        const string SHADER_PATH_URP_UNLIT = "glTF-urp-unlit.shader";
#else
        const string SHADER_LIT = "glTF/Universal/Lit";
        const string SHADER_UNLIT = "glTF/Universal/Unlit";
#endif

        private static Shader litShader;
        private static Shader unlitShader;

        public override Material GetDefaultMaterial()
        {
            return GetLitMaterial();
        }

        protected virtual Shader FinderShaderLit()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{SHADER_PATH_PREFIX}{SHADER_PATH_URP_LIT}");
#else
            return FindShader(SHADER_LIT);
#endif
        }

        protected virtual Shader FinderShaderUnlit()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{SHADER_PATH_PREFIX}{SHADER_PATH_URP_UNLIT}");
#else
            return FindShader(SHADER_UNLIT);
#endif
        }

        Material GetLitMaterial()
        {
            if (litShader == null)
            {
                litShader = FinderShaderLit();
            }
            if (litShader == null)
            {
                return null;
            }
            return new Material(litShader);
        }

        Material GetUnlitMaterial()
        {
            if (unlitShader == null)
            {
                unlitShader = FinderShaderUnlit();
            }
            if (unlitShader == null)
            {
                return null;
            }
            return new Material(unlitShader);
        }

        public override Material GenerateMaterial(Schema.Material gltfMaterial, IGltfReadable gltf)
        {
            Material material;

            ShaderMode shaderMode = ShaderMode.Opaque;

            bool isUnlit = gltfMaterial.extensions?.KHR_materials_unlit != null;
            bool isMetallicRoughness = gltfMaterial.extensions?.KHR_materials_pbrSpecularGlossiness == null;

            // Get Material
            if (isUnlit)
            {
                material = GetUnlitMaterial();
                shaderMode = gltfMaterial.alphaModeEnum == AlphaMode.BLEND ? ShaderMode.Blend : ShaderMode.Opaque;
            }
            else
            {
                material = GetLitMaterial();
                if (isMetallicRoughness)
                {
                    var metallicShaderFeatures = GetMetallicShaderFeatures(gltfMaterial);
                    shaderMode = (ShaderMode)(metallicShaderFeatures & MetallicShaderFeatures.ModeMask);
                }
                else
                {
                    var specularShaderFeatures = GetSpecularShaderFeatures(gltfMaterial);
                    if ((specularShaderFeatures & SpecularShaderFeatures.AlphaBlend) != 0)
                    {
                        shaderMode = ShaderMode.Blend;
                    }
                }
            }

            if (material == null) return null;

            material.name = gltfMaterial.name;

            // MetallicRoughness or SpecularGlossiness
            WorkflowMode workflowMode = isMetallicRoughness ? WorkflowMode.Metallic : WorkflowMode.Specular;
            material.SetFloat(workflowModePropId, (int)workflowMode);

            // Set Modes
            // Note: SurfaceType will be Transparent if it's AlphaTest.
            SurfaceType surfaceType = (gltfMaterial.alphaModeEnum == AlphaMode.OPAQUE) ? SurfaceType.Opaque : SurfaceType.Transparent;
            // Note: BlendMode will be Alpha(default) if it's AlphaTest and surfaceType is Opaque.
            BlendMode blendMode = (shaderMode == ShaderMode.Premultiply) ? BlendMode.Premultiply : BlendMode.Alpha;

            // Transmission
            Transmission transmission = null;
            if (gltfMaterial.extensions != null)
            {
                transmission = gltfMaterial.extensions.KHR_materials_transmission;
                if (transmission != null)
                {
                    surfaceType = SurfaceType.Transparent;
                    blendMode = BlendMode.Premultiply; // Use alpha Premultiply to visualize approximate transmission instead of using CameraOpaqueTexture.
                }
            }

            material.SetFloat(surfaceTypePropId, (int)surfaceType);
            material.SetFloat(blendModePropId, (int)blendMode);

            // Alpha Clip
            bool alphaClip = gltfMaterial.alphaModeEnum == AlphaMode.MASK;
            material.SetFloat(alphaClipPropId, alphaClip ? 1f : 0f);

            if (alphaClip)
            {
                SetAlphaModeMask(material, gltfMaterial.alphaCutoff);
            }
            else if (surfaceType == SurfaceType.Opaque)
            {
                SetOpaqueMode(material);
            }
            else if (blendMode == BlendMode.Alpha)
            {
                SetAlphaModeBlend(material);
            }
            else if (blendMode == BlendMode.Premultiply)
            {
                SetAlphaModePremultiply(material);
            }

            // Double Side
            if (gltfMaterial.doubleSided)
            {
                // Turn off back-face culling
                material.SetFloat(cullPropId, 0);
#if UNITY_EDITOR
                material.doubleSidedGI = true;
#endif
            }

            // [Base Color, texture and PBR textures]
            Color baseColorLinear = Color.white;
            //added support for KHR_materials_pbrSpecularGlossiness
            if (gltfMaterial.extensions != null)
            {
                // Specular glossiness
                Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null)
                {
                    material.EnableKeyword(KW_SPECULAR_SETUP);

                    baseColorLinear = specGloss.diffuseColor;

                    material.SetVector(specColorPropId, specGloss.specularColor);
                    material.SetFloat(smoothnessPropId, specGloss.glossinessFactor);

                    TrySetTexture(
                        specGloss.diffuseTexture,
                        material,
                        gltf,
                        baseMapPropId,
                        baseMapScaleTransformPropId,
                        baseMapRotationPropId,
                        baseMapUVChannelPropId
                        );

                    if (TrySetTexture(
                        specGloss.specularGlossinessTexture,
                        material,
                        gltf,
                        specGlossMapPropId,
                        specGlossScaleTransformMapPropId,
                        specGlossMapRotationPropId,
                        specGlossMapUVChannelPropId
                        ))
                    {
                        material.EnableKeyword(KW_METALLIC_ROUGHNESS_MAP);
                    }
                }
            }

            // not KHR_materials_pbrSpecularGlossiness
            if (gltfMaterial.pbrMetallicRoughness != null
                // If there's a specular-glossiness extension, ignore metallic-roughness
                // (according to extension specification)
                && gltfMaterial.extensions?.KHR_materials_pbrSpecularGlossiness == null)
            {
                baseColorLinear = gltfMaterial.pbrMetallicRoughness.baseColor;

                // baseColorTexture can be used by both MetallicRoughness AND Unlit materials
                TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                    material,
                    gltf,
                    baseMapPropId,
                    baseMapScaleTransformPropId,
                    baseMapRotationPropId,
                    baseMapUVChannelPropId
                    );

                if (workflowMode == WorkflowMode.Metallic)
                {
                    material.SetFloat(metallicPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor);
                    material.SetFloat(smoothnessPropId, 1 - gltfMaterial.pbrMetallicRoughness.roughnessFactor);

                    if (TrySetTexture(
                        gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,
                        material,
                        gltf,
                        metallicRoughnessMapPropId,
                        metallicRoughnessMapScaleTransformPropId,
                        metallicRoughnessMapRotationPropId,
                        metallicRoughnessMapUVChannelPropId
                        ))
                    {
                        material.EnableKeyword(KW_METALLIC_ROUGHNESS_MAP);
                        material.SetFloat(smoothnessPropId, 1f); // We don't need this property if it uses a texture.
                    }

                    // TODO: When the occlusionTexture equals the metallicRoughnessTexture, we could sample just once instead of twice.
                    // if (!DifferentIndex(gltfMaterial.occlusionTexture,gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture)) {
                    //    ...
                    // }
                }
            }

            // [Normal map]
            if (TrySetTexture(
                gltfMaterial.normalTexture,
                material,
                gltf,
                bumpMapPropId,
                bumpMapScaleTransformPropId,
                bumpMapRotationPropId,
                bumpMapUVChannelPropId
            ))
            {
                material.EnableKeyword(Constants.kwNormalMap);
                material.SetFloat(bumpScalePropId, gltfMaterial.normalTexture.scale);
            }

            // [Ambient Occlusion]
            if (TrySetTexture(
                gltfMaterial.occlusionTexture,
                material,
                gltf,
                occlusionMapPropId,
                occlusionMapScaleTransformPropId,
                occlusionMapRotationPropId,
                occlusionMapUVChannelPropId
                ))
            {
                material.EnableKeyword(KW_OCCLUSION_MAP);
                material.SetFloat(occlusionStrengthPropId, gltfMaterial.occlusionTexture.strength);
            }

            // [Emissive]
            if (TrySetTexture(
                gltfMaterial.emissiveTexture,
                material,
                gltf,
                emissionMapPropId,
                emissionMapScaleTransformPropId,
                emissionMapRotationPropId,
                emissionMapUVChannelPropId
                ))
            {
                material.EnableKeyword(KW_EMISSION);
            }

            // [Transmission]
            if (transmission != null)
            {
#if UNITY_EDITOR
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                logger?.Warning(LogCode.MaterialTransmissionApproxURP);
#endif
                // TODO: Blur cameraOpaqueTexture along with surface roughness(smoothness)
                // if(supportsCameraOpaqueTexture)
                // {
                //     material.EnableKeyword(KW_TRANSMISSION_BLUR);
                // }
                material.SetFloat(transmissionPropId, 1.0f); // Use float instead of using a keyword to reduce variants.
                material.SetFloat(transmissionFactorPropId, transmission.transmissionFactor);
                if (TrySetTexture(
                    transmission.transmissionTexture,
                    material,
                    gltf,
                    transmissionTexturePropId,
                    transmissionTextureScaleTransformPropId,
                    transmissionTextureRotationPropId,
                    transmissionTextureUVChannelPropId
                    ))
                { }
            }

            // [MOTION VECTOR]
            if (gltfMaterial.alphaModeEnum != AlphaMode.MASK)
            {
                // double sided opaque would make errors in HDRP 7.3 otherwise
                material.SetOverrideTag(TAG_MOTION_VECTOR, TAG_MOTION_VECTOR_USER);
                material.SetShaderPassEnabled(k_MotionVectorsPass, false);
            }

            // [Base Color]
            material.SetColor(baseColorPropId, baseColorLinear.gamma);

            // [Emission]
            if (gltfMaterial.emissive != Color.black)
            {
                material.SetColor(emissionColorPropId, gltfMaterial.emissive.gamma);
                material.EnableKeyword(KW_EMISSION);
            }

            return material;
        }

        protected MetallicShaderFeatures GetMetallicShaderFeatures(Schema.Material gltfMaterial)
        {
            var feature = MetallicShaderFeatures.Default;
            ShaderMode? sm = null;

            if (gltfMaterial.extensions != null)
            {

                if (gltfMaterial.extensions.KHR_materials_clearcoat != null &&
                    gltfMaterial.extensions.KHR_materials_clearcoat.clearcoatFactor > 0)
                {
                    feature |= MetallicShaderFeatures.ClearCoat;
                }
                if (gltfMaterial.extensions.KHR_materials_sheen != null &&
                    gltfMaterial.extensions.KHR_materials_sheen.sheenColor.maxColorComponent > 0)
                {
                    feature |= MetallicShaderFeatures.Sheen;
                }
                if (gltfMaterial.extensions.KHR_materials_transmission != null
                    && gltfMaterial.extensions.KHR_materials_transmission.transmissionFactor > 0)
                {
                    sm = ApplyTransmissionShaderFeatures(gltfMaterial);
                }
            }

            if (gltfMaterial.doubleSided)
            {
                feature |= MetallicShaderFeatures.DoubleSided;
            }

            if (!sm.HasValue)
            {
                sm = gltfMaterial.alphaModeEnum == AlphaMode.BLEND ? ShaderMode.Blend : ShaderMode.Opaque;
            }

            feature |= (MetallicShaderFeatures)sm;

            return feature;
        }

        protected virtual ShaderMode? ApplyTransmissionShaderFeatures(Schema.Material gltfMaterial)
        {
            // Makeshift approximation
            Color baseColorLinear = Color.white;
            bool premul = TransmissionWorkaroundShaderMode(gltfMaterial.extensions.KHR_materials_transmission, ref baseColorLinear);
            ShaderMode? sm = premul ? ShaderMode.Premultiply : ShaderMode.Blend;
            return sm;
        }

        static SpecularShaderFeatures GetSpecularShaderFeatures(Schema.Material gltfMaterial)
        {
            var feature = SpecularShaderFeatures.Default;
            if (gltfMaterial.doubleSided)
            {
                feature |= SpecularShaderFeatures.DoubleSided;
            }
            if (gltfMaterial.alphaModeEnum != AlphaMode.OPAQUE)
            {
                feature |= SpecularShaderFeatures.AlphaBlend;
            }
            return feature;
        }

        private static void SetAlphaModeMask(UnityEngine.Material material, float alphaCutoff)
        {
            material.SetFloat(cutoffPropId, alphaCutoff);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_CUTOUT);
            material.EnableKeyword(KW_ALPHATEST_ON);
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(zWritePropId, 1);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;  //2450
        }

        private static void SetAlphaModeBlend(UnityEngine.Material material)
        {
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_TRANSPARENT);
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);//5
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        private static void SetAlphaModePremultiply(UnityEngine.Material material)
        {
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_TRANSPARENT);
            material.EnableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);//1
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        private static void SetOpaqueMode(UnityEngine.Material material)
        {
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_OPAQUE);
            material.renderQueue = -1;
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(zWritePropId, 1);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
        }
    }
}
#endif // USING_URP
