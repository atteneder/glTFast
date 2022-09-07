// Based on Unity UnlitGUI from com.unity.render-pipelines.universal v12.1.7.

// com.unity.render-pipelines.universal copyright © 2020 Unity Technologies ApS
// Licensed under the Unity Companion License for Unity-dependent projects--see [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.

// Modifications Copyright 2022 Spatial

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public static class GLTFUnlitGUI
    {
        private static readonly string PROP_BASEMAP_UV_CHANNEL = "_BaseMapUVChannel";
        private static readonly string PROP_BASEMAP_UV_ROTATION = "_BaseMapRotation";

        public const string KW_UV_ROTATION = "_UV_ROTATION";

        public enum UVChannel
        {
            UV0,
            UV1,
        }

        public struct GLTFUnlitProperties
        {
            public MaterialProperty baseMapUVChannelProp;
            public MaterialProperty baseMapRotationProp;

            public GLTFUnlitProperties(MaterialProperty[] properties)
            {
                baseMapUVChannelProp = BaseShaderGUI.FindProperty(PROP_BASEMAP_UV_CHANNEL, properties, false);
                baseMapRotationProp = BaseShaderGUI.FindProperty(PROP_BASEMAP_UV_ROTATION, properties, false);
            }
        }

        public static void Inputs(GLTFUnlitProperties properties, MaterialEditor materialEditor, Material material)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("glTF shader properties", EditorStyles.boldLabel);

            // Base Map UV
            EditorGUILayout.Space(5);
            HandleDropdown<UVChannel>(materialEditor, properties.baseMapUVChannelProp, "Base Map UV Channel");
            materialEditor.VectorProperty(properties.baseMapRotationProp, "Base Map UV Rotation");
        }

        public static void SetMaterialKeywords(Material material)
        {
            Vector4 baseMapRot = material.GetVector(PROP_BASEMAP_UV_ROTATION);
            bool isRotation = baseMapRot.x != 0f || baseMapRot.y != 0f;
            CoreUtils.SetKeyword(material, KW_UV_ROTATION, isRotation);
        }

        public static void HandleDropdown<Type>(MaterialEditor materialEditor, MaterialProperty property, string name) where Type : Enum
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = property.hasMixedValue;

            var value = property.floatValue;
            value = EditorGUILayout.Popup(name, (int)value, Enum.GetNames(typeof(UVChannel)));
            if (EditorGUI.EndChangeCheck())
                property.floatValue = value;

            EditorGUI.showMixedValue = false;
        }
    }
}
