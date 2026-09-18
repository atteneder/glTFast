// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_URP

using System;

using Unity.Cloud.Gltfast.Objects;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Scripting.APIUpdating;
using Color = UnityEngine.Color;
using Material = UnityEngine.Material;
using GltfMaterial = Unity.Cloud.Gltfast.Objects.Material;

namespace Unity.Cloud.Gltfast.Materials
{

    [MovedFrom(true, sourceNamespace: "GLTFast.Materials", sourceAssembly: "glTFast")]
    public class UniversalRPMaterialGenerator : ShaderGraphMaterialGenerator
    {

        // Keywords
        const string k_TransmissionKeyword = "_TRANSMISSION";

#if UNITY_EDITOR
        /// <summary>Guid of the shader graph with clearcoat support</summary>
        const string k_MetallicClearcoatShaderGuid = "c18c97ae1ce021b4980c5d19a54f0d3c";
#endif
        /// <summary>Name of the shader graph with clearcoat support</summary>
        public const string MetallicClearcoatShader = "glTF-pbrMetallicRoughness-Clearcoat";

        static bool s_MetallicClearcoatShaderQueried;
        static Shader s_MetallicClearcoatShader;

        bool m_SupportsCameraOpaqueTexture;

        public UniversalRPMaterialGenerator(UniversalRenderPipelineAsset renderPipelineAsset)
        {
            m_SupportsCameraOpaqueTexture = renderPipelineAsset.supportsCameraOpaqueTexture;
        }

        protected override void SetDoubleSided(GltfMaterial gltfMaterial, Material material)
        {
            base.SetDoubleSided(gltfMaterial, material);
            material.SetFloat(MaterialProperty.Cull, (int)CullMode.Off);
        }

        protected override void SetAlphaModeMask(GltfMaterial gltfMaterial, Material material)
        {
            base.SetAlphaModeMask(gltfMaterial, material);
            material.SetFloat(MaterialProperty.AlphaClip, 1);
        }

        protected override void SetShaderModeBlend(GltfMaterial gltfMaterial, Material material)
        {
            material.SetOverrideTag(RenderTypeTag, TransparentRenderType);
            material.EnableKeyword(SurfaceTypeTransparentKeyword);
            material.EnableKeyword(DisableSsrTransparentKeyword);
            material.EnableKeyword(EnableFogOnTransparentKeyword);
            material.SetShaderPassEnabled(ShaderPassTransparentDepthPrepass, false);
            material.SetShaderPassEnabled(ShaderPassTransparentDepthPostpass, false);
            material.SetShaderPassEnabled(ShaderPassTransparentBackface, false);
            material.SetShaderPassEnabled(ShaderPassRayTracingPrepass, false);
            material.SetShaderPassEnabled(ShaderPassDepthOnlyPass, false);
            material.SetFloat(MaterialProperty.SrcBlend, (int)BlendMode.SrcAlpha);//5
            material.SetFloat(MaterialProperty.DstBlend, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(ZTestGBufferProperty, (int)CompareFunction.Equal); //3
            material.SetFloat(AlphaDstBlendProperty, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(MaterialProperty.Surface, 1);
            material.SetFloat(MaterialProperty.ZWrite, 0);
        }

        /// <summary>
        /// Picks the shader graph with clearcoat support, if any material feature requires it.
        /// </summary>
        /// <param name="features">Material features</param>
        /// <returns>Shader capable of rendering the features</returns>
        protected override Shader GetMetallicShader(MetallicShaderFeatures features)
        {
            if ((features & MetallicShaderFeatures.ClearCoat) != 0)
            {
                if (!s_MetallicClearcoatShaderQueried)
                {
#if UNITY_EDITOR
                    s_MetallicClearcoatShader = LoadShaderByGuid(new GUID(k_MetallicClearcoatShaderGuid));
#else
                    s_MetallicClearcoatShader = LoadShaderByName(MetallicClearcoatShader);
#endif
                    if (s_MetallicClearcoatShader == null)
                    {
                        // Fallback to regular shader graph
                        s_MetallicClearcoatShader = base.GetMetallicShader(features);
                    }
                    s_MetallicClearcoatShaderQueried = true;
                }
                return s_MetallicClearcoatShader;
            }

            return base.GetMetallicShader(features);
        }

        protected override ShaderMode? ApplyTransmissionShaderFeatures(GltfMaterial gltfMaterial)
        {
            if (!m_SupportsCameraOpaqueTexture)
            {
                // Fall back to makeshift approximation via premultiply or blend
                return base.ApplyTransmissionShaderFeatures(gltfMaterial);
            }

            if (gltfMaterial?.Extensions?.Transmission != null
                && gltfMaterial.Extensions.Transmission.TransmissionFactor > 0f)
            {
                return ShaderMode.Blend;
            }

            // No explicitly change in shader features
            return null;
        }

        protected override RenderQueue? ApplyTransmission(
            ref Color baseColorLinear,
            IGltfReadable gltf,
            Transmission transmission,
            Material material,
            RenderQueue? renderQueue
        )
        {
            if (m_SupportsCameraOpaqueTexture)
            {
                if (transmission.TransmissionFactor > 0f)
                {
                    material.EnableKeyword(k_TransmissionKeyword);
                    material.SetFloat(TransmissionFactorProperty, transmission.TransmissionFactor);
                    renderQueue = RenderQueue.Transparent;
                    if (TrySetTexture(
                        transmission.TransmissionTexture,
                        material,
                        gltf,
                        TransmissionTextureProperty
                    // TransmissionTextureScaleTransformProperty, // TODO: add support in shader
                    // TransmissionTextureRotationProperty, // TODO: add support in shader
                    // TransmissionTextureUVChannelProperty // TODO: add support in shader
                    )) { }
                }
                return renderQueue;
            }

            return base.ApplyTransmission(
                ref baseColorLinear,
                gltf,
                transmission,
                material,
                renderQueue
                );
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsOnLoad()
        {
            // Reset static state
            s_MetallicClearcoatShader = null;
            s_MetallicClearcoatShaderQueried = false;
        }
#endif
    }
}
#endif // USING_URP
