// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
        enum BlendModeOption
        {
            Opaque = StandardShaderMode.Opaque,
            Cutout = StandardShaderMode.Cutout,
            Fade = StandardShaderMode.Fade,
            Transparent = StandardShaderMode.Transparent,
        }

        UvTransform? m_UVTransform;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor.target is Material material)
            {
                string current = material.GetTag(RenderTypeTag, false);
                BlendModeOption currentBlendMode = BlendModeOption.Opaque;

                switch (current)
                {
                    case "":
                    case OpaqueRenderType:
                        currentBlendMode = BlendModeOption.Opaque;
                        break;

                    case TransparentCutoutRenderType:
                        currentBlendMode = BlendModeOption.Cutout;
                        break;
                    case FadeRenderType:
                        currentBlendMode = BlendModeOption.Fade;
                        break;
                    case TransparentRenderType:
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

                m_UVTransform = TextureRotationSlider(
                    material,
                    m_UVTransform,
                    BaseColorTextureScaleTransformProperty,
                    BaseColorTextureRotationProperty,
                    true,
                    "Base Color Tex Rotation");
                if (m_UVTransform.HasValue)
                {
                    if (m_UVTransform.Value.rotation != 0)
                    {
                        material.EnableKeyword(TextureTransformKeyword);
                    }
                    else
                    {
                        material.DisableKeyword(TextureTransformKeyword);
                    }
                }

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(material);
                }

                void EnsureKeywordForMap(string textureProperty, string keyword)
                {
                    if (material.HasProperty(textureProperty))
                    {
                        if (material.GetTexture(textureProperty) && !material.IsKeywordEnabled(keyword))
                            material.EnableKeyword(keyword);

                        if (!material.GetTexture(textureProperty) && material.IsKeywordEnabled(keyword))
                            material.DisableKeyword(keyword);
                    }
                }

                EnsureKeywordForMap("_MetallicGlossMap", "_METALLICGLOSSMAP");
                EnsureKeywordForMap("occlusionTexture", "_OCCLUSION");
            }

            base.OnGUI(materialEditor, properties);
        }

        static void ConfigureBlendMode(Material material, BlendModeOption mode)
        {
            switch (mode)
            {
                case BlendModeOption.Opaque:
                    SetOpaqueMode(material);
                    break;
                case BlendModeOption.Cutout:
                    SetAlphaModeMask(material, material.GetFloat(AlphaCutoffProperty));
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
