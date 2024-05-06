// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using static GLTFast.Materials.MaterialGenerator;

namespace GLTFast.Editor
{
    class ShaderGUIBase : ShaderGUI
    {
        const float k_Tolerance = 0.001f;

        protected UvTransform? TextureRotationSlider(
            Material material,
            UvTransform? uvTransform,
            int scaleTransformPropertyId,
            int rotationPropertyId,
            bool freezeScale = false,
            string label = "Texture Rotation"
            )
        {
            UvTransform oldUvTransform;
            UvTransform newUvTransform;

            if (uvTransform.HasValue)
            {
                oldUvTransform = uvTransform.Value;
                newUvTransform = uvTransform.Value;
            }
            else
            {
                oldUvTransform = UvTransform.FromMaterial(material, scaleTransformPropertyId, rotationPropertyId);
                newUvTransform = new UvTransform();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            var newUvRotation = EditorGUILayout.Slider(oldUvTransform.rotation, 0, 360);
            GUILayout.EndHorizontal();

            float2 newUvScale = new float2(1, 1);
            if (!freezeScale)
            {
                GUILayout.BeginHorizontal();
                newUvScale = EditorGUILayout.Vector2Field("Scale", oldUvTransform.scale);
                GUILayout.EndHorizontal();
            }

            if (!uvTransform.HasValue)
            {
                newUvTransform.rotation = newUvRotation;
                newUvTransform.scale = newUvScale;
            }

            bool update = false;

            if (Math.Abs(newUvRotation - oldUvTransform.rotation) > k_Tolerance)
            {
                newUvTransform.rotation = newUvRotation;
                update = true;
            }

            if (!freezeScale && !newUvScale.Equals(oldUvTransform.scale))
            {
                newUvTransform.scale = newUvScale;
                update = true;
            }

            if (update)
            {
                var cos = math.cos(newUvTransform.rotation * Mathf.Deg2Rad);
                var sin = math.sin(newUvTransform.rotation * Mathf.Deg2Rad);
                var currentScaleTransform = material.GetVector(scaleTransformPropertyId);
                float2x2 rotScale = math.mul(new float2x2(cos, sin, -sin, cos), new float2x2(newUvTransform.scale.x, 0, 0, newUvTransform.scale.y));
                material.SetVector(scaleTransformPropertyId, new Vector4(rotScale.c0.x, rotScale.c1.y, currentScaleTransform.z, currentScaleTransform.w));
                material.SetVector(rotationPropertyId, new Vector4(rotScale.c1.x, rotScale.c0.y, 0, 0));
                if (newUvTransform.rotation == 0)
                {
                    material.DisableKeyword(TextureTransformKeyword);
                }
                else
                {
                    material.EnableKeyword(TextureTransformKeyword);
                }
                return newUvTransform;
            }

            return uvTransform;
        }
    }
}
