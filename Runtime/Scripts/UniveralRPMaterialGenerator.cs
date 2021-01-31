// Copyright 2020-2021 Andreas Atteneder
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
using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Material = UnityEngine.Material;
using Texture = GLTFast.Schema.Texture;

namespace GLTFast.Materials {

    public class UniveralRPMaterialGenerator : ShaderGraphMaterialGenerator {

        static bool supportsCameraOpaqueTexture;

        public UniveralRPMaterialGenerator(UniversalRenderPipelineAsset renderPipelineAsset) {
            supportsCameraOpaqueTexture = renderPipelineAsset.supportsCameraOpaqueTexture;
        }
        
        protected override ShaderMode? ApplyTransmissionShaderFeatures(Schema.Material gltfMaterial) {
            if (!supportsCameraOpaqueTexture) {
                // Fall back to makeshift approximation via premultiply or blend 
                return base.ApplyTransmissionShaderFeatures(gltfMaterial);
            }
            // No explicitly change in shader features
            return null;
        }
        
        protected override RenderQueue? ApplyTransmission(
            ref Color baseColorLinear,
            ref Texture[] textures,
            ref Image[] schemaImages,
            ref Dictionary<int, Texture2D>[] imageVariants,
            Transmission transmission,
            Material material,
            RenderQueue? renderQueue
        ) {
            if (supportsCameraOpaqueTexture) {
                if (transmission.transmissionFactor > 0f) {
                    material.EnableKeyword("TRANSMISSION");
                    material.SetFloat(transmissionFactorPropId, transmission.transmissionFactor);
                    renderQueue = RenderQueue.Transparent;
                    if (TrySetTexture(transmission.transmissionTexture, material, transmissionTexturePropId, ref textures, ref schemaImages, ref imageVariants)) { }
                }
                return renderQueue;
            }

            return base.ApplyTransmission(
                ref baseColorLinear,
                ref textures,
                ref schemaImages,
                ref imageVariants,
                transmission,
                material,
                renderQueue
                );
        }
    }
}
#endif // USING_URP
