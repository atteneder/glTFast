// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_URP

using System;

using GLTFast.Schema;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using Material = UnityEngine.Material;

namespace GLTFast.Materials {

    public class UniversalRPMaterialGenerator : ShaderGraphMaterialGenerator {

        // Keywords
        const string k_TransmissionKeyword = "_TRANSMISSION";

        static bool s_SupportsCameraOpaqueTexture;

#if USING_URP_12_OR_NEWER
        static readonly int k_AlphaClipPropId = Shader.PropertyToID("_AlphaClip");
        static readonly int k_SurfacePropId = Shader.PropertyToID("_Surface");

#if UNITY_EDITOR
        /// <summary>Guid of the shader graph with clearcoat support</summary>
        const string k_MetallicClearcoatShaderGuid = "c18c97ae1ce021b4980c5d19a54f0d3c";
#endif
        /// <summary>Name of the shader graph with clearcoat support</summary>
        public const string MetallicClearcoatShader = "glTF-pbrMetallicRoughness-Clearcoat";

        static bool s_MetallicClearcoatShaderQueried;
        static Shader s_MetallicClearcoatShader;
#endif

        public UniversalRPMaterialGenerator(UniversalRenderPipelineAsset renderPipelineAsset) {
            s_SupportsCameraOpaqueTexture = renderPipelineAsset.supportsCameraOpaqueTexture;
        }

#if USING_URP_12_OR_NEWER
        protected override void SetDoubleSided(MaterialBase gltfMaterial, Material material) {
            base.SetDoubleSided(gltfMaterial,material);
            material.SetFloat(CullProperty, (int)CullMode.Off);
        }

        protected override void SetAlphaModeMask(MaterialBase gltfMaterial, Material material) {
            base.SetAlphaModeMask(gltfMaterial, material);
            material.SetFloat(k_AlphaClipPropId, 1);
        }

        protected override void SetShaderModeBlend(MaterialBase gltfMaterial, Material material) {
            material.SetOverrideTag(RenderTypeTag, TransparentRenderType);
            material.EnableKeyword(SurfaceTypeTransparentKeyword);
            material.EnableKeyword(DisableSsrTransparentKeyword);
            material.EnableKeyword(EnableFogOnTransparentKeyword);
            material.SetShaderPassEnabled(ShaderPassTransparentDepthPrepass, false);
            material.SetShaderPassEnabled(ShaderPassTransparentDepthPostpass, false);
            material.SetShaderPassEnabled(ShaderPassTransparentBackface, false);
            material.SetShaderPassEnabled(ShaderPassRayTracingPrepass, false);
            material.SetShaderPassEnabled(ShaderPassDepthOnlyPass, false);
            material.SetFloat(SrcBlendProperty, (int) BlendMode.SrcAlpha);//5
            material.SetFloat(DstBlendProperty, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(ZTestGBufferProperty, (int)CompareFunction.Equal); //3
            material.SetFloat(AlphaDstBlendProperty, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(k_SurfacePropId, 1);
            material.SetFloat(ZWriteProperty, 0);
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
#endif

        protected override ShaderMode? ApplyTransmissionShaderFeatures(MaterialBase gltfMaterial) {
            if (!s_SupportsCameraOpaqueTexture) {
                // Fall back to makeshift approximation via premultiply or blend
                return base.ApplyTransmissionShaderFeatures(gltfMaterial);
            }

            if (gltfMaterial?.Extensions?.KHR_materials_transmission != null
                && gltfMaterial.Extensions.KHR_materials_transmission.transmissionFactor > 0f)
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
        ) {
            if (s_SupportsCameraOpaqueTexture) {
                if (transmission.transmissionFactor > 0f) {
                    material.EnableKeyword(k_TransmissionKeyword);
                    material.SetFloat(TransmissionFactorProperty, transmission.transmissionFactor);
                    renderQueue = RenderQueue.Transparent;
                    if (TrySetTexture(
                        transmission.transmissionTexture,
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
    }
}
#endif // USING_URP
