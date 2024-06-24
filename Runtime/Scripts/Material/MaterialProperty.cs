// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace GLTFast.Materials
{
    /// <summary>
    /// Holds static material property identifiers.
    /// </summary>
    public static class MaterialProperty
    {
        /// <summary>Shader property ID for property alphaCutoff</summary>
        public static readonly int AlphaCutoff = Shader.PropertyToID("alphaCutoff");
        /// <summary>Shader property ID for property baseColorTexture</summary>
        public static readonly int BaseColor = Shader.PropertyToID("baseColorFactor");
        /// <summary>Shader property ID for property baseColorTexture</summary>
        public static readonly int BaseColorTexture = Shader.PropertyToID("baseColorTexture");
        /// <summary>Shader property ID for property baseColorTexture_Rotation</summary>
        public static readonly int BaseColorTextureRotation = Shader.PropertyToID("baseColorTexture_Rotation");
        /// <summary>Shader property ID for property baseColorTexture_ST</summary>
        public static readonly int BaseColorTextureScaleTransform = Shader.PropertyToID("baseColorTexture_ST");
        /// <summary>Shader property ID for property baseColorTexture_texCoord</summary>
        public static readonly int BaseColorTextureTexCoord = Shader.PropertyToID("baseColorTexture_texCoord");
        /// <summary>Shader property ID for property _Cull</summary>
        public static readonly int Cull = Shader.PropertyToID("_Cull");
        /// <summary>Shader property ID for property _CullMode</summary>
        public static readonly int CullMode = Shader.PropertyToID("_CullMode");
        /// <summary>Shader property ID for property _DstBlend</summary>
        public static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        /// <summary>Shader property ID for property diffuseFactor</summary>
        public static readonly int DiffuseFactor = Shader.PropertyToID("diffuseFactor");
        /// <summary>Shader property ID for property diffuseTexture</summary>
        public static readonly int DiffuseTexture = Shader.PropertyToID("diffuseTexture");
        /// <summary>Shader property ID for property diffuseTexture_ST</summary>
        public static readonly int DiffuseTextureScaleTransform = Shader.PropertyToID("diffuseTexture_ST");
        /// <summary>Shader property ID for property diffuseTexture_Rotation</summary>
        public static readonly int DiffuseTextureRotation = Shader.PropertyToID("diffuseTexture_Rotation");
        /// <summary>Shader property ID for property diffuseTexture_texCoord</summary>
        public static readonly int DiffuseTextureTexCoord = Shader.PropertyToID("diffuseTexture_texCoord");
        /// <summary>Shader property ID for property emissiveFactor</summary>
        public static readonly int EmissiveFactor = Shader.PropertyToID("emissiveFactor");
        /// <summary>Shader property ID for property emissiveTexture</summary>
        public static readonly int EmissiveTexture = Shader.PropertyToID("emissiveTexture");
        /// <summary>Shader property ID for property emissiveTexture_Rotation</summary>
        public static readonly int EmissiveTextureRotation = Shader.PropertyToID("emissiveTexture_Rotation");
        /// <summary>Shader property ID for property emissiveTexture_ST</summary>
        public static readonly int EmissiveTextureScaleTransform = Shader.PropertyToID("emissiveTexture_ST");
        /// <summary>Shader property ID for property emissiveTexture_texCoord</summary>
        public static readonly int EmissiveTextureTexCoord = Shader.PropertyToID("emissiveTexture_texCoord");
        /// <summary>Shader property ID for property glossinessFactor</summary>
        public static readonly int GlossinessFactor = Shader.PropertyToID("glossinessFactor");
        /// <summary>Shader property ID for property normalTexture</summary>
        public static readonly int NormalTexture = Shader.PropertyToID("normalTexture");
        /// <summary>Shader property ID for property normalTexture_Rotation</summary>
        public static readonly int NormalTextureRotation = Shader.PropertyToID("normalTexture_Rotation");
        /// <summary>Shader property ID for property normalTexture_ST</summary>
        public static readonly int NormalTextureScaleTransform = Shader.PropertyToID("normalTexture_ST");
        /// <summary>Shader property ID for property normalTexture_texCoord</summary>
        public static readonly int NormalTextureTexCoord = Shader.PropertyToID("normalTexture_texCoord");
        /// <summary>Shader property ID for property normalTexture_scale</summary>
        public static readonly int NormalTextureScale = Shader.PropertyToID("normalTexture_scale");
        /// <summary>Shader property ID for property metallicFactor</summary>
        public static readonly int Metallic = Shader.PropertyToID("metallicFactor");
        /// <summary>Shader property ID for property metallicRoughnessTexture</summary>
        public static readonly int MetallicRoughnessMap = Shader.PropertyToID("metallicRoughnessTexture");
        /// <summary>Shader property ID for property metallicRoughnessTexture_ST</summary>
        public static readonly int MetallicRoughnessMapScaleTransform = Shader.PropertyToID("metallicRoughnessTexture_ST");
        /// <summary>Shader property ID for property metallicRoughnessTexture_Rotation</summary>
        public static readonly int MetallicRoughnessMapRotation = Shader.PropertyToID("metallicRoughnessTexture_Rotation");
        /// <summary>Shader property ID for property metallicRoughnessTexture_texCoord</summary>
        public static readonly int MetallicRoughnessMapTexCoord = Shader.PropertyToID("metallicRoughnessTexture_texCoord");
        /// <summary>Shader property ID for property _Mode</summary>
        public static readonly int Mode = Shader.PropertyToID("_Mode");
        /// <summary>Shader property ID for property occlusionTexture</summary>
        public static readonly int OcclusionTexture = Shader.PropertyToID("occlusionTexture");
        /// <summary>Shader property ID for property occlusionTexture_strength</summary>
        public static readonly int OcclusionTextureStrength = Shader.PropertyToID("occlusionTexture_strength");
        /// <summary>Shader property ID for property occlusionTexture_Rotation</summary>
        public static readonly int OcclusionTextureRotation = Shader.PropertyToID("occlusionTexture_Rotation");
        /// <summary>Shader property ID for property occlusionTexture_ST</summary>
        public static readonly int OcclusionTextureScaleTransform = Shader.PropertyToID("occlusionTexture_ST");
        /// <summary>Shader property ID for property occlusionTexture_texCoord</summary>
        public static readonly int OcclusionTextureTexCoord = Shader.PropertyToID("occlusionTexture_texCoord");
        /// <summary>Shader property ID for property roughnessFactor</summary>
        public static readonly int RoughnessFactor = Shader.PropertyToID("roughnessFactor");
        /// <summary>Shader property ID for property specularFactor</summary>
        public static readonly int SpecularFactor = Shader.PropertyToID("specularFactor");
        /// <summary>Shader property ID for property specularGlossinessTexture</summary>
        public static readonly int SpecularGlossinessTexture = Shader.PropertyToID("specularGlossinessTexture");
        /// <summary>Shader property ID for property specularGlossinessTexture_ST</summary>
        public static readonly int SpecularGlossinessTextureScaleTransform = Shader.PropertyToID("specularGlossinessTexture_ST"); // TODO: Support in shader!
        /// <summary>Shader property ID for property specularGlossinessTexture_Rotation</summary>
        public static readonly int SpecularGlossinessTextureRotation = Shader.PropertyToID("specularGlossinessTexture_Rotation"); // TODO: Support in shader!
        /// <summary>Shader property ID for property specularGlossinessTexture_texCoord</summary>
        public static readonly int SpecularGlossinessTextureTexCoord = Shader.PropertyToID("specularGlossinessTexture_texCoord"); // TODO: Support in shader!
        /// <summary>Shader property ID for property _SrcBlend</summary>
        public static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        /// <summary>Shader property ID for property _ZWrite</summary>
        public static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

#if UNITY_SHADER_GRAPH
        /// <summary>Shader property ID for property _AlphaClip</summary>
        public static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        /// <summary>Shader property ID for property _Surface</summary>
        public static readonly int Surface = Shader.PropertyToID("_Surface");
#endif

#if USING_HDRP
        /// <summary>Shader property ID for property _AlphaCutoffEnable</summary>
        public static readonly int AlphaCutoffEnable = Shader.PropertyToID("_AlphaCutoffEnable");
        /// <summary>Shader property ID for property _DoubleSidedEnable</summary>
        public static readonly int DoubleSidedEnable = Shader.PropertyToID("_DoubleSidedEnable");
#if UNITY_2021_1_OR_NEWER
        /// <summary>Shader property ID for property _EnableBlendModePreserveSpecularLighting</summary>
        public static readonly int EnableBlendModePreserveSpecularLighting = Shader.PropertyToID(HDMaterialProperties.kEnableBlendModePreserveSpecularLighting);
#endif
        /// <summary>Shader property ID for property _SurfaceType</summary>
        public static readonly int SurfaceType = Shader.PropertyToID("_SurfaceType");
#endif
    }
}
