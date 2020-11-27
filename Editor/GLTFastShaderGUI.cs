using System;
using UnityEditor;
using UnityEngine;
using static GLTFast.Materials.StandardShaderHelper;

namespace GLTFast.Editor
{
    public class GLTFastShaderGUI : ShaderGUI
    {
        private const float TOLERANCE = 0.001f;

        /// <summary>
        /// Subset of <see cref="StandardShaderMode"/> as not all configurations are supported
        /// </summary>
        public enum BlendModeOption
        {
            Opaque = StandardShaderMode.Opaque,
            Cutout = StandardShaderMode.Cutout,
            Transparent = StandardShaderMode.Transparent,
        }

        private float? uvRotation;

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

                GUILayout.BeginHorizontal();
                GUILayout.Label("Texture Rotation");
                float oldUvRotation = uvRotation.HasValue ? uvRotation.Value : GetUvRotation(material);
                float newUvRotation = EditorGUILayout.Slider(oldUvRotation,0,360);
                GUILayout.EndHorizontal();
                
                if (Math.Abs(newUvRotation - oldUvRotation) > TOLERANCE) {
                    uvRotation = newUvRotation;
                    float cos = Mathf.Cos(uvRotation.Value*Mathf.Deg2Rad);
                    float sin = Mathf.Sin(uvRotation.Value*Mathf.Deg2Rad);
                    material.SetVector(mainTexRotatePropId,new Vector4(cos,sin,-sin,cos));
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

        /// <summary>
        /// Extracts a material's texture rotation (degrees) from the 2 by 2 matrix
        /// </summary>
        /// <param name="material"></param>
        /// <returns>texture rotation in degrees</returns>
        float GetUvRotation(Material material) {
            var r = material.GetVector(mainTexRotatePropId);
            var acos = Mathf.Acos(r.x);
            if (r.y < 0) acos = (Mathf.PI*2)-acos;
            return acos * Mathf.Rad2Deg;
        }
    }
}
