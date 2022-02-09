// Copyright 2020-2022 Andreas Atteneder
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

using UnityEditor;
using UnityEngine;
using static GLTFast.Materials.MaterialGenerator;
using static GLTFast.Materials.BuiltInMaterialGenerator;

namespace GLTFast.Editor
{
    using Materials;

    class BuiltInShaderGUI : ShaderGUIBase
    {

        /// <summary>
        /// Subset of <see cref="StandardShaderMode"/> as not all configurations are supported
        /// </summary>
        public enum BlendModeOption
        {
            Opaque = StandardShaderMode.Opaque,
            Cutout = StandardShaderMode.Cutout,
            Fade = StandardShaderMode.Fade,
            Transparent = StandardShaderMode.Transparent,
        }

        private UvTransform? uvTransform;
        
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor.target is Material material)
            {
                string current = material.GetTag(TAG_RENDER_TYPE, false);
                BlendModeOption currentBlendMode = BlendModeOption.Opaque;

                switch (current)
                {
                    case "":
                    case TAG_RENDER_TYPE_OPAQUE:
                        currentBlendMode = BlendModeOption.Opaque;
                        break;

                    case TAG_RENDER_TYPE_CUTOUT:
                        currentBlendMode = BlendModeOption.Cutout;
                        break;
                    case TAG_RENDER_TYPE_FADE:
                        currentBlendMode = BlendModeOption.Fade;
                        break;
                    case TAG_RENDER_TYPE_TRANSPARENT:
                        currentBlendMode = BlendModeOption.Transparent;
                        break;

                }
                GUILayout.BeginHorizontal();
                GUILayout.Label("Blend Mode");
                BlendModeOption blend = (BlendModeOption)EditorGUILayout.EnumPopup(currentBlendMode);
                GUILayout.EndHorizontal();

                if (blend != currentBlendMode)
                {
                    ConfigureBlendMode(material, blend);
                }

                uvTransform = TextureRotationSlider(material, uvTransform, mainTexScaleTransform, mainTexRotation,true);
                if (uvTransform.HasValue)
                {
                    if (uvTransform.Value.rotation != 0) {
                        material.EnableKeyword(KW_UV_ROTATION);
                    } else {
                        material.DisableKeyword(KW_UV_ROTATION);
                    }
                }
                
                if (GUI.changed) {
                    EditorUtility.SetDirty(material);
                }

                void EnsureKeywordForMap(string textureProperty, string keyword)
                {
                    if (material.HasProperty(textureProperty))
                    {
                        if(material.GetTexture(textureProperty) && !material.IsKeywordEnabled(keyword))
                            material.EnableKeyword(keyword);
                        
                        if(!material.GetTexture(textureProperty) && material.IsKeywordEnabled(keyword))
                            material.DisableKeyword(keyword);
                    }
                }

                EnsureKeywordForMap("_MetallicGlossMap", "_METALLICGLOSSMAP");
                EnsureKeywordForMap("_OcclusionMap", "_OCCLUSION");
            }

            base.OnGUI(materialEditor, properties);
        }

        public static void ConfigureBlendMode(Material material, BlendModeOption mode)
        {
            switch (mode)
            {
                case BlendModeOption.Opaque:
                    SetOpaqueMode(material);
                    break;
                case BlendModeOption.Cutout:
                    SetAlphaModeMask(material, material.GetFloat(cutoffPropId));
                    break;
                case BlendModeOption.Fade:
                    SetAlphaModeBlend(material);
                    break;
                case BlendModeOption.Transparent:
                    SetAlphaModeTransparent(material);
                    break;
            }
        }
    }
}
