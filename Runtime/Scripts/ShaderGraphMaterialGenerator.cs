// Copyright 2020 Andreas Atteneder
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
#endif

#if GLTFAST_SHADER_GRAPH

using System;
using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using Material = UnityEngine.Material;

namespace GLTFast.Materials {

    using AlphaMode = Schema.Material.AlphaMode;
    using Texture = Schema.Texture;

    public class ShaderGraphMaterialGenerator : MaterialGenerator {
        
        [Flags]
        public enum ShaderMode {
            Opaque = 0,
            Blend = 1,
            Premultiply = 1<<1,
        }

        [Flags]
        protected enum MetallicShaderFeatures {
            Default = 0,
            // Bits 0-1 are the shader modes
            ModeMask = 0x3,
            ModeOpaque = 0,
            ModeFade = 1,
            ModeTransparent = 1<<1,
            // Other flags
            DoubleSided = 1<<2,
            ClearCoat = 1<<3,
            Sheen = 1<<4,
        }

        
        [Flags]
        protected enum SpecularShaderFeatures {
            Default = 0,
            AlphaBlend = 1<<1,
            DoubleSided = 1<<2
        }

        const string SHADER_UNLIT = "Shader Graphs/glTF-unlit-Opaque";
        const string SHADER_SPECULAR = "Shader Graphs/glTF-specular";

        // Keywords
        const string KW_OCCLUSION = "OCCLUSION";
        const string KW_EMISSION = "EMISSION";
        const string KW_CLEARCOAT_MAP = "CLEARCOAT_MAP";

        protected static readonly int alphaCutoffPropId = Shader.PropertyToID("alphaCutoff");
        static readonly int baseColorFactorPropId = Shader.PropertyToID("baseColorFactor");
        static readonly int baseColorTexturePropId = Shader.PropertyToID("baseColorTexture");
        static readonly int clearcoatFactorPropId = Shader.PropertyToID("clearcoatFactor");
        static readonly int clearcoatRoughnessFactorPropId = Shader.PropertyToID("clearcoatRoughnessFactor");
        static readonly int clearcoatTexturePropId = Shader.PropertyToID("clearcoatTexture");
        protected static readonly int clearcoatNormalTexturePropId = Shader.PropertyToID("clearcoatNormalTexture");
        static readonly int emissiveFactorPropId = Shader.PropertyToID("emissiveFactor");
        static readonly int emissiveTexturePropId = Shader.PropertyToID("emissiveTexture");
        static readonly int glossinessFactorPropId = Shader.PropertyToID("glossinessFactor");
        static readonly int metallicFactorPropId = Shader.PropertyToID("metallicFactor");
        static readonly int metallicRoughnessTexturePropId = Shader.PropertyToID("metallicRoughnessTexture");
        static readonly int normalScalePropId = Shader.PropertyToID("normalScale");
        static readonly int normalTexturePropId = Shader.PropertyToID("normalTexture");
        static readonly int occlusionTexturePropId = Shader.PropertyToID("occlusionTexture");
        static readonly int roughnessFactorPropId = Shader.PropertyToID("roughnessFactor");
        static readonly int specularFactorPropId = Shader.PropertyToID("specularFactor");
        static readonly int specularGlossinessTexturePropId = Shader.PropertyToID("specularGlossinessTexture");
        protected static readonly int transmissionFactorPropId = Shader.PropertyToID("transmissionFactor");
        protected static readonly int transmissionTexturePropId = Shader.PropertyToID("transmissionTexture");

        static Dictionary<MetallicShaderFeatures,Shader> metallicShaders = new Dictionary<MetallicShaderFeatures,Shader>();
        static Dictionary<SpecularShaderFeatures,Shader> specularShaders = new Dictionary<SpecularShaderFeatures,Shader>();
        static Shader[] unlitShaders = new Shader[2]; // single- and double-sided

        public override Material GetDefaultMaterial() {
            return GetMetallicMaterial(MetallicShaderFeatures.Default);
        }

        Material GetMetallicMaterial( MetallicShaderFeatures metallicShaderFeatures ) {
            
            bool doubleSided = (metallicShaderFeatures & MetallicShaderFeatures.DoubleSided) != 0;
            
            if(!metallicShaders.TryGetValue(metallicShaderFeatures,value: out var shader)) {
                shader = FindShader(GetShaderName(metallicShaderFeatures));
                metallicShaders[metallicShaderFeatures] = shader;
            }
            if(shader==null) {
                return null;
            }
            var mat = new Material(shader);
#if UNITY_EDITOR
            mat.doubleSidedGI = doubleSided; 
#endif
            return mat;
        }

        /// <summary>
        /// Get the shader's name that supports the required features.
        /// </summary>
        /// <param name="metallicShaderFeatures">Required features</param>
        protected virtual string GetShaderName(MetallicShaderFeatures metallicShaderFeatures) {
            ShaderMode mode = (ShaderMode) (metallicShaderFeatures & MetallicShaderFeatures.ModeMask);
            bool coat = (metallicShaderFeatures & MetallicShaderFeatures.ClearCoat) != 0;
            // TODO: add sheen support
            bool sheen = false; // (metallicShaderFeatures & MetallicShaderFeatures.Sheen) != 0;
            bool doubleSided = (metallicShaderFeatures & MetallicShaderFeatures.DoubleSided) != 0;
            return $"Shader Graphs/glTF-metallic-{mode}{(coat ? "-coat" : "")}{(sheen ? "-sheen" : "")}{(doubleSided ? "-double" : "")}";
        }

        static Material GetUnlitMaterial(bool doubleSided=false)
        {
            int index = doubleSided ? 0 : 1;
            if(unlitShaders[index]==null) {
                var shaderName = doubleSided ? string.Format("{0}{1}",SHADER_UNLIT,"-double") : SHADER_UNLIT;
                unlitShaders[index] = FindShader(shaderName);
            }
            if(unlitShaders[index]==null) {
                return null;
            }
            var mat = new Material(unlitShaders[index]);
#if UNITY_EDITOR
            mat.doubleSidedGI = doubleSided;
#endif
            return mat;
        }
        
        static Material GetSpecularMaterial(SpecularShaderFeatures features) {
            bool doubleSided = (features & SpecularShaderFeatures.DoubleSided) != 0;
            Shader shader = null;
            if(!specularShaders.TryGetValue(features,out shader)) {
                bool alphaBlend = (features & SpecularShaderFeatures.AlphaBlend) != 0;
                var shaderName = string.Format(
                    "{0}{1}{2}",
                    SHADER_SPECULAR,
                    alphaBlend ? "-Blend" : "-Opaque",
                    doubleSided ? "-double" : ""
                    );
                shader = FindShader(shaderName);
                specularShaders[features] = shader;
            }
            if(shader==null) {
                return null;
            }
            var mat = new Material(shader);
#if UNITY_EDITOR
            mat.doubleSidedGI = doubleSided;
#endif
            return mat;
        }

        public override Material GenerateMaterial(
            Schema.Material gltfMaterial,
            ref Texture[] textures,
            ref Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
        ) {
            Material material;

            MaterialType? materialType = null;
            ShaderMode shaderMode = ShaderMode.Opaque;

            if (gltfMaterial.extensions.KHR_materials_unlit!=null) {
                material = GetUnlitMaterial(gltfMaterial.doubleSided);
                materialType = MaterialType.Unlit;
            } else {
                bool isMetallicRoughness = gltfMaterial.extensions == null ||
                           gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness == null;
                if (isMetallicRoughness) {
                    materialType = MaterialType.MetallicRoughness;
                    var metallicShaderFeatures = GetMetallicShaderFeatures(gltfMaterial);
                    material = GetMetallicMaterial(metallicShaderFeatures);
                    shaderMode = (ShaderMode)(metallicShaderFeatures & MetallicShaderFeatures.ModeMask);
                }
                else {
                    materialType = MaterialType.SpecularGlossiness;
                    var specularShaderFeatures = GetSpecularShaderFeatures(gltfMaterial);
                    material = GetSpecularMaterial(specularShaderFeatures);
                    if ((specularShaderFeatures & SpecularShaderFeatures.AlphaBlend) != 0) {
                        shaderMode = ShaderMode.Blend;
                    }
                }
            }

            if(material==null) return null;

            material.name = gltfMaterial.name;

            Color baseColorLinear = Color.white;
            RenderQueue? renderQueue = null;
            
            //added support for KHR_materials_pbrSpecularGlossiness
            if (gltfMaterial.extensions != null) {
                PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null) {
                    baseColorLinear = specGloss.diffuseColor;
                    material.SetVector(specularFactorPropId, specGloss.specularColor);
                    material.SetFloat(glossinessFactorPropId, specGloss.glossinessFactor);

                    TrySetTexture(specGloss.diffuseTexture,material,baseColorTexturePropId,ref textures,ref schemaImages, ref imageVariants);

                    if (TrySetTexture(specGloss.specularGlossinessTexture,material,specularGlossinessTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                        // material.EnableKeyword();
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness!=null) {
                baseColorLinear = gltfMaterial.pbrMetallicRoughness.baseColor;

                if (materialType != MaterialType.SpecularGlossiness) {
                    // baseColorTexture can be used by both MetallicRoughness AND Unlit materials
                    TrySetTexture(
                        gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                        material,
                        baseColorTexturePropId,
                        ref textures,
                        ref schemaImages,
                        ref imageVariants
                        );
                }

                if (materialType==MaterialType.MetallicRoughness)
                {
                    material.SetFloat(metallicFactorPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor );
                    material.SetFloat(roughnessFactorPropId, gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                    if(TrySetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,material,metallicRoughnessTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                        // material.EnableKeyword(KW_METALLIC_ROUGHNESS_MAP);
                    }

                    // TODO: When the occlusionTexture equals the metallicRoughnessTexture, we could sample just once instead of twice.
                    // if (!DifferentIndex(gltfMaterial.occlusionTexture,gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture)) {
                    //    ...
                    // }
                }
            }

            if(TrySetTexture(gltfMaterial.normalTexture,material,normalTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                // material.EnableKeyword(KW_NORMALMAP);
                material.SetFloat(normalScalePropId,gltfMaterial.normalTexture.scale);
            }
            
            if(TrySetTexture(gltfMaterial.occlusionTexture,material,occlusionTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(KW_OCCLUSION);
            }

            if(TrySetTexture(gltfMaterial.emissiveTexture,material,emissiveTexturePropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(KW_EMISSION);
            }
            
            if (gltfMaterial.extensions != null) {

                // Transmission - Approximation
                var transmission = gltfMaterial.extensions.KHR_materials_transmission;
                if (transmission != null) {
                    var premul = ApplyTransmission(ref baseColorLinear, ref textures, ref schemaImages, ref imageVariants, transmission, material, ref renderQueue);
                    if (premul.HasValue) {
                        if (premul.Value) {
                            renderQueue = RenderQueue.Transparent;
                            shaderMode = ShaderMode.Premultiply;
                        }
                        else {
                            renderQueue = RenderQueue.Transparent;
                            shaderMode = ShaderMode.Blend;
                        }
                    }
                }

                var clearcoat = gltfMaterial.extensions.KHR_materials_clearcoat;
                if (clearcoat != null) {
                    ApplyClearcoat(ref textures, ref schemaImages, ref imageVariants, material, clearcoat);
                }
            }

            ApplyAlphaCutoff(material,gltfMaterial.alphaModeEnum==AlphaMode.MASK, gltfMaterial.alphaCutoff);
            
            if (!renderQueue.HasValue) {
                if(shaderMode == ShaderMode.Opaque) {
                    renderQueue = gltfMaterial.alphaModeEnum == AlphaMode.MASK
                        ? RenderQueue.AlphaTest
                        : RenderQueue.Geometry;
                } else {
                    renderQueue = RenderQueue.Transparent;
                }
            }

            material.renderQueue = (int) renderQueue.Value;
            switch (shaderMode) {
                case ShaderMode.Opaque:
                    break;
                case ShaderMode.Blend:
                    SetAlphaModeBlend(material);
                    break;
                case ShaderMode.Premultiply:
                    SetAlphaModeTransparent(material);
                    break;
            }
            material.SetVector(baseColorFactorPropId, baseColorLinear);
            
            if (gltfMaterial.emissive != Color.black) {
                ApplyEmission(material,gltfMaterial.emissive);
            }

            return material;
        }

        protected virtual void ApplyEmission(Material material,Color emissive) {
            material.SetColor(emissiveFactorPropId, emissive);
            material.EnableKeyword(KW_EMISSION);
        }

        protected virtual void ApplyAlphaCutoff(Material material, bool enable, float alphaCutoff) {
            if (enable) {
                material.EnableKeyword(KW_ALPHATEST_ON);
                material.SetFloat(alphaCutoffPropId, alphaCutoff);
            }
            else {
                material.DisableKeyword(KW_ALPHATEST_ON);
                material.SetFloat(alphaCutoffPropId, 0);
            }
        }

        protected virtual void ApplyClearcoat(ref Texture[] textures, ref Image[] schemaImages, ref Dictionary<int, Texture2D>[] imageVariants, Material material, ClearCoat clearcoat) {
            material.SetFloat(clearcoatFactorPropId, clearcoat.clearcoatFactor);
            material.SetFloat(clearcoatRoughnessFactorPropId, clearcoat.clearcoatRoughnessFactor);
            if (TrySetTexture(clearcoat.clearcoatTexture, material, clearcoatTexturePropId, ref textures, ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(KW_CLEARCOAT_MAP);
            }

            if (TrySetTexture(clearcoat.clearcoatRoughnessTexture, material, clearcoatTexturePropId, ref textures, ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(KW_CLEARCOAT_MAP);
            }
        }

        /// <summary>
        /// Applies Material Transmission
        /// </summary>
        /// <param name="baseColorLinear"></param>
        /// <param name="textures"></param>
        /// <param name="schemaImages"></param>
        /// <param name="imageVariants"></param>
        /// <param name="transmission"></param>
        /// <param name="material"></param>
        /// <param name="renderQueue"></param>
        /// <returns>True, when premultiplied makeshift solution was applied</returns>
        protected virtual bool? ApplyTransmission(
            ref Color baseColorLinear,
            ref Texture[] textures,
            ref Image[] schemaImages,
            ref Dictionary<int, Texture2D>[] imageVariants,
            Transmission transmission,
            Material material,
            ref RenderQueue? renderQueue
            )
        {
#if UNITY_EDITOR
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.LogWarning(
                "Chance of incorrect materials! glTF transmission"
                + " is approximated. Enable Opaque Texture access in Universal Render Pipeline!"
                );
#endif
            // Correct transmission is not supported in Built-In renderer
            // This is an approximation for some corner cases
            if (transmission.transmissionFactor > 0f && transmission.transmissionTexture.index < 0) {
                return TransmissionWorkaroundShaderMode(transmission, ref baseColorLinear);
            }
            return null;
        }

        protected MetallicShaderFeatures GetMetallicShaderFeatures(Schema.Material gltfMaterial) {

            var feature = MetallicShaderFeatures.Default;
            ShaderMode? sm = null;

            if (gltfMaterial.extensions != null) {

                if (gltfMaterial.extensions.KHR_materials_clearcoat != null &&
                    gltfMaterial.extensions.KHR_materials_clearcoat.clearcoatFactor > 0) feature |= MetallicShaderFeatures.ClearCoat;
                if (gltfMaterial.extensions.KHR_materials_sheen != null &&
                    gltfMaterial.extensions.KHR_materials_sheen.sheenColor.maxColorComponent > 0) feature |= MetallicShaderFeatures.Sheen;

                if (
                    gltfMaterial.extensions.KHR_materials_transmission != null
                    && gltfMaterial.extensions.KHR_materials_transmission.transmissionFactor > 0
                ) {
                    sm = ApplyTransmissionShaderFeatures(gltfMaterial);
                }
            }

            if (gltfMaterial.doubleSided) feature |= MetallicShaderFeatures.DoubleSided;

            if (!sm.HasValue) {
                sm = gltfMaterial.alphaModeEnum != AlphaMode.OPAQUE ? ShaderMode.Blend : ShaderMode.Opaque;
            } 
            
            feature |= (MetallicShaderFeatures)sm;

            return feature;
        }

        protected virtual ShaderMode? ApplyTransmissionShaderFeatures(Schema.Material gltfMaterial) {
            // Makeshift approximation
            Color baseColorLinear = Color.white;
            var premul = TransmissionWorkaroundShaderMode(gltfMaterial.extensions.KHR_materials_transmission, ref baseColorLinear);
            ShaderMode? sm = premul ? ShaderMode.Premultiply : ShaderMode.Blend;
            return sm;
        }

        static SpecularShaderFeatures GetSpecularShaderFeatures(Schema.Material gltfMaterial) {

            var feature = SpecularShaderFeatures.Default;
            if (gltfMaterial.doubleSided) feature |= SpecularShaderFeatures.DoubleSided;

            if (gltfMaterial.alphaModeEnum != AlphaMode.OPAQUE) {
                feature |= SpecularShaderFeatures.AlphaBlend;
            }
            return feature;
        }
        
        public virtual void SetAlphaModeBlend( Material material ) {}

        public virtual void SetAlphaModeTransparent( Material material ) {}
    }
}
#endif // GLTFAST_SHADER_GRAPH
