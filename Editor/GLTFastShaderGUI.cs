using System;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using static GLTFast.Materials.StandardShaderHelper;

namespace GLTFast.Editor
{
    public class GLTFastShaderGUI : GLTFastShaderGUIBase
    {

        /// <summary>
        /// Subset of <see cref="StandardShaderMode"/> as not all configurations are supported
        /// </summary>
        public enum BlendModeOption
        {
            Opaque = StandardShaderMode.Opaque,
            Cutout = StandardShaderMode.Cutout,
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

                uvTransform = TextureRotationSlider(material, uvTransform, mainTexRotatePropId, true);
                if (uvTransform.HasValue)
                {
                    if (uvTransform.Value.rotation != 0) {
                        material.EnableKeyword(KW_UV_ROTATION);
                    } else {
                        material.DisableKeyword(KW_UV_ROTATION);
                    }
                }
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
                case BlendModeOption.Transparent:
                    SetAlphaModeBlend(material);
                    break;
            }
        }
    }
}
