// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_HDRP

using System;
using Unity.Cloud.Gltfast.Objects;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Scripting.APIUpdating;

using Material = UnityEngine.Material;
using GltfMaterial = Unity.Cloud.Gltfast.Objects.Material;

namespace Unity.Cloud.Gltfast.Materials
{

    [MovedFrom(true, sourceNamespace: "GLTFast.Materials", sourceAssembly: "glTFast")]
    public class HighDefinitionRPMaterialGenerator : ShaderGraphMaterialGenerator
    {

        // ReSharper disable MemberCanBePrivate.Global

        /// <summary>DistortionVectors shader pass name</summary>
        public const string DistortionVectorsPass = "DistortionVectors";

        /// <summary>_CullModeForward shader property identifier</summary>
        public static readonly int CullModeForwardProperty = Shader.PropertyToID("_CullModeForward");

        // ReSharper restore MemberCanBePrivate.Global

        static readonly int k_ZTestDepthEqualForOpaque = Shader.PropertyToID("_ZTestDepthEqualForOpaque");

        static readonly int k_RenderQueueType = Shader.PropertyToID("_RenderQueueType");
        const string k_DoubleSidedOnKeyword = "_DOUBLESIDED_ON";

        static readonly int k_DoubleSidedNormalModePropId = Shader.PropertyToID("_DoubleSidedNormalMode");
        static readonly int k_DoubleSidedConstantsPropId = Shader.PropertyToID("_DoubleSidedConstants");

#if UNITY_EDITOR
        /// <summary>GUID of the stack lit shader graph used for advanced PBR materials</summary>
        public const string MetallicStackLitShaderGuid = "429ab83ee9ef05b4f8a76e58ea5d5ad4";
#endif
        /// <summary>Name of the stack lit shader graph used for advanced PBR materials</summary>
        public const string MetallicStackLitShader = "glTF-pbrMetallicRoughnessStackLit";

        static bool s_MetallicStackLitShaderQueried;
        static Shader s_MetallicStackLitShader;

        public override Material GenerateMaterial(GltfMaterial gltfMaterial, IGltfReadable gltf, bool pointsSupport = false)
        {
            var material = base.GenerateMaterial(gltfMaterial, gltf, pointsSupport);
            HDMaterial.ValidateMaterial(material);
            return material;
        }

        protected override void SetDoubleSided(Objects.Material gltfMaterial, Material material)
        {
            base.SetDoubleSided(gltfMaterial, material);

            material.EnableKeyword(k_DoubleSidedOnKeyword);
            material.SetFloat(MaterialProperty.DoubleSidedEnable, 1);

            // UnityEditor.Rendering.HighDefinition.DoubleSidedNormalMode.Flip
            material.SetFloat(k_DoubleSidedNormalModePropId, 0);
            material.SetVector(k_DoubleSidedConstantsPropId, new Vector4(-1, -1, -1, 0));

            material.SetFloat(MaterialProperty.CullMode, (int)CullMode.Off);
            material.SetFloat(CullModeForwardProperty, (int)CullMode.Off);
        }

        protected override void SetAlphaModeMask(Objects.Material gltfMaterial, Material material)
        {
            base.SetAlphaModeMask(gltfMaterial, material);

            material.SetFloat(MaterialProperty.AlphaCutoffEnable, 1);
            material.SetOverrideTag(MotionVectorTag, MotionVectorUser);
            material.SetShaderPassEnabled(MotionVectorsPass, false);


            if (gltfMaterial.Extensions?.Unlit != null)
            {
                material.EnableKeyword(SurfaceTypeTransparentKeyword);
                material.EnableKeyword(DisableSsrTransparentKeyword);
                material.EnableKeyword(EnableFogOnTransparentKeyword);

                material.SetShaderPassEnabled(ShaderPassTransparentDepthPrepass, false);
                material.SetShaderPassEnabled(ShaderPassTransparentDepthPostpass, false);
                material.SetShaderPassEnabled(ShaderPassTransparentBackface, false);
                material.SetShaderPassEnabled(ShaderPassRayTracingPrepass, false);
                material.SetShaderPassEnabled(ShaderPassDepthOnlyPass, false);

                material.SetFloat(AlphaDstBlendProperty, (int)BlendMode.OneMinusSrcAlpha);//10
                material.SetOverrideTag(RenderTypeTag, TransparentRenderType);
                material.SetShaderPassEnabled(DistortionVectorsPass, false);
                material.SetFloat(MaterialProperty.DstBlend, (int)BlendMode.OneMinusSrcAlpha);//10
                material.SetFloat(MaterialProperty.SrcBlend, (int)BlendMode.One);
                // material.SetFloat(k_RenderQueueType, 4);
                // material.SetFloat(k_SurfaceType, 1);
                material.SetFloat(k_ZTestDepthEqualForOpaque, (int)CompareFunction.LessEqual);
                material.SetFloat(MaterialProperty.ZWrite, 0);
            }
        }

        /// <summary>
        /// Picks more advanced StackLit based shader graph, if any material feature requires it.
        /// </summary>
        /// <param name="features">Material features</param>
        /// <returns>Shader capable of rendering the features</returns>
        protected override Shader GetMetallicShader(MetallicShaderFeatures features)
        {
            if ((features & MetallicShaderFeatures.ClearCoat) != 0)
            {
                if (!s_MetallicStackLitShaderQueried)
                {
#if UNITY_EDITOR
                    s_MetallicStackLitShader = LoadShaderByGuid(new GUID(MetallicStackLitShaderGuid));
#else
                    s_MetallicStackLitShader = LoadShaderByName(MetallicStackLitShader);
#endif
                    if (s_MetallicStackLitShader == null)
                    {
                        // Fallback to regular shader graph
                        s_MetallicStackLitShader = base.GetMetallicShader(features);
                    }
                    s_MetallicStackLitShaderQueried = true;
                }
                return s_MetallicStackLitShader;
            }

            return base.GetMetallicShader(features);
        }

        protected override void SetShaderModeBlend(Objects.Material gltfMaterial, Material material)
        {

            material.DisableKeyword(AlphaTestOnKeyword);
            material.EnableKeyword(SurfaceTypeTransparentKeyword);
            // material.EnableKeyword(KW_DISABLE_DECALS);
            material.EnableKeyword(DisableSsrTransparentKeyword);
            material.EnableKeyword(EnableFogOnTransparentKeyword);

            material.SetOverrideTag(RenderTypeTag, TransparentRenderType);

            material.SetShaderPassEnabled(ShaderPassTransparentDepthPrepass, false);
            material.SetShaderPassEnabled(ShaderPassTransparentDepthPostpass, false);
            material.SetShaderPassEnabled(ShaderPassTransparentBackface, false);
            material.SetShaderPassEnabled(ShaderPassRayTracingPrepass, false);
            material.SetShaderPassEnabled(ShaderPassDepthOnlyPass, false);

            material.SetFloat(MaterialProperty.AlphaCutoffEnable, 0);
            material.SetFloat(k_RenderQueueType, (int)CustomPass.RenderQueueType.PreRefraction);// 4
            material.SetFloat(MaterialProperty.SurfaceType, 1);
            material.SetFloat(MaterialProperty.ZWrite, 0);
            material.SetFloat(ZTestGBufferProperty, (int)CompareFunction.LessEqual); //4
            material.SetFloat(k_ZTestDepthEqualForOpaque, (int)CompareFunction.LessEqual); //4
            material.SetFloat(AlphaDstBlendProperty, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(MaterialProperty.DstBlend, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(MaterialProperty.SrcBlend, (int)BlendMode.SrcAlpha);//5
            material.SetFloat(MaterialProperty.EnableBlendModePreserveSpecularLighting, 0);
        }
    }
}
#endif // USING_URP
