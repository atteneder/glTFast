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

#if ! (GLTFAST_URP || GLTFAST_HDRP)
#define GLTFAST_BUILTIN_RP
#endif

using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {

    using Materials;
   
    using AlphaMode = Schema.Material.AlphaMode;

    public abstract class MaterialGenerator : IMaterialGenerator {

        public static IMaterialGenerator GetDefaultMaterialGenerator() {

            /// TODO: Dynamically detect if scripting defines don't match
            /// actual render pipeline.
            /// Show warning explaining how to setup project properly

#if GLTFAST_URP
            return new URPMaterialGenerator();
#endif

#if ! (GLTFAST_URP || GLTFAST_HDRP)
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

        protected static void TrySetTextureTransform(
            Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int propertyId,
            bool flipY = false
            )
        {
            Vector2 offset = Vector2.zero;
            Vector2 scale = Vector2.one;

            if(textureInfo.extensions != null && textureInfo.extensions.KHR_texture_transform!=null) {
                var tt = textureInfo.extensions.KHR_texture_transform;
                if(tt.texCoord!=0) {
                    Debug.LogError("Multiple UV sets are not supported!");
                }

                float cos = 1;
                float sin = 0;

                if(tt.offset!=null) {
                    offset.x = tt.offset[0];
                    offset.y = 1-tt.offset[1];
                }
                if(tt.scale!=null) {
                    scale.x = tt.scale[0];
                    scale.y = tt.scale[1];
                    material.SetTextureScale(propertyId,scale);
                }
                if(tt.rotation!=0) {
                    cos = Mathf.Cos(tt.rotation);
                    sin = Mathf.Sin(tt.rotation);
                    material.SetVector(StandardShaderHelper.mainTexRotatePropId,new Vector4(cos,sin,-sin,cos));
                    material.EnableKeyword(StandardShaderHelper.KW_UV_ROTATION);
                    offset.x += scale.y * sin;
                }
                offset.y -= scale.y * cos;
                material.SetTextureOffset(propertyId,offset);
            }

            if(flipY) {
                offset.y = 1-offset.y;
                scale.y = -scale.y;
            }

            material.SetTextureOffset(propertyId,offset);
            material.SetTextureScale(propertyId,scale);
        }
    }
}
