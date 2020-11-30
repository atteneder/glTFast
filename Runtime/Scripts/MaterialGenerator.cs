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

#if !GLTFAST_SHADER_GRAPH
#define GLTFAST_BUILTIN_RP
#endif

using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace GLTFast {

    using Materials;
   
    using AlphaMode = Schema.Material.AlphaMode;

    public abstract class MaterialGenerator : IMaterialGenerator {

        public enum MaterialType {
            Unknown,
            MetallicRoughness,
            SpecularGlossiness,
            Unlit,
        }
        
        public static IMaterialGenerator GetDefaultMaterialGenerator() {

            // TODO: Dynamically detect if scripting defines don't match
            // actual render pipeline.
            // Show warning explaining how to setup project properly

#if UNITY_EDITOR
            string srpName = GraphicsSettings.renderPipelineAsset?.GetType().ToString();
#if GLTFAST_SHADER_GRAPH
            if (string.IsNullOrEmpty(srpName)) {
                Debug.LogError("URP/HDRP is installed but no render pipeline asset is assigned! Falling back to built-in. This will NOT work in builds");
                return new BuiltInMaterialGenerator();
            }
#endif
#endif
            
#if GLTFAST_SHADER_GRAPH
            return new ShaderGraphMaterialGenerator();
#else
            return new BuiltInMaterialGenerator();
#endif
        }

        public abstract UnityEngine.Material GetDefaultMaterial();

        public abstract UnityEngine.Material GenerateMaterial(
            Schema.Material gltfMaterial,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
        );

        protected static Shader FindShader(string shaderName) {
            var shader = Shader.Find(shaderName);
            if(shader==null) {
                Debug.LogErrorFormat(
                    "Shader \"{0}\" is missing. Make sure to include it in the build (see https://github.com/atteneder/glTFast/blob/main/Documentation%7E/glTFast.md#materials-and-shader-variants )",
                    shaderName
                    );
            }
            return shader;
        }

        protected static bool TrySetTexture(
            Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int propertyId,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
            )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int bcTextureIndex = textureInfo.index;
                if (textures != null && textures.Length > bcTextureIndex)
                {
                    var txt = textures[bcTextureIndex];
                    var imageIndex = txt.GetImageIndex();

                    Texture2D img = null;
                    if( imageVariants!=null
                        && imageIndex >= 0
                        && imageVariants.Length > imageIndex
                        && imageVariants[imageIndex]!=null
                        && imageVariants[imageIndex].TryGetValue(txt.sampler,out img)
                        )
                    {
                        material.SetTexture(propertyId,img);
                        var isKtx = txt.isKtx;
                        TrySetTextureTransform(textureInfo,material,propertyId,isKtx);
                        return true;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Image #{0} not found", imageIndex);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Texture #{0} not found", bcTextureIndex);
                }
            }
            return false;
        }
        
        protected static bool DifferentIndex(Schema.TextureInfo a, Schema.TextureInfo b) {
            return a != null && b != null && a.index>=0 && b.index>=0 && a.index != b.index;
        }

        private static void TrySetTextureTransform(
            Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int propertyId,
            bool flipY = false
            )
        {
            // Scale (x,y) and Transform (z,w)
            float4 textureST = new float4(
                1,1,// scale
                0,0 // transform
                );

            if(textureInfo.extensions != null && textureInfo.extensions.KHR_texture_transform!=null) {
                var tt = textureInfo.extensions.KHR_texture_transform;
                if(tt.texCoord!=0) {
                    Debug.LogError("Multiple UV sets are not supported!");
                }

                float cos = 1;
                float sin = 0;

                if(tt.offset!=null) {
                    textureST.z = tt.offset[0];
                    textureST.w = 1-tt.offset[1];
                }
                if(tt.scale!=null) {
                    textureST.x = tt.scale[0];
                    textureST.y = tt.scale[1];
                }
                if(tt.rotation!=0) {
                    cos = math.cos(tt.rotation);
                    sin = math.sin(tt.rotation);

                    var newRot = new Vector2(textureST.x * sin, textureST.y * -sin );
                    material.SetVector(StandardShaderHelper.mainTexRotation, newRot);
                    textureST.x *= cos;
                    textureST.y *= cos;

                    material.EnableKeyword(StandardShaderHelper.KW_UV_ROTATION);
                    textureST.z -= newRot.y; // move offset to move rotation point (horizontally) 
                }

                textureST.w -= textureST.y * cos; // move offset to move flip axis point (vertically)
            }

            if(flipY) {
                textureST.z = 1-textureST.z; // flip offset in Y
                textureST.y = -textureST.y; // flip scale in Y
            }
            
            material.SetVector(StandardShaderHelper.mainTexScaleTransform, textureST);
        }
    }
}
