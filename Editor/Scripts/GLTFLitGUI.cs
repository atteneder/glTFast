// Based on Unity LitGUI from com.unity.render-pipelines.universal v12.1.7.

// com.unity.render-pipelines.universal copyright © 2020 Unity Technologies ApS
// Licensed under the Unity Companion License for Unity-dependent projects--see [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.

// Modifications Copyright 2022 Spatial

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    public static class GLTFLitGUI
    {
        private static readonly string PROP_BASEMAP_UV_CHANNEL = "_BaseMapUVChannel";
        private static readonly string PROP_BASEMAP_UV_ROTATION = "_BaseMapRotation";
        private static readonly string PROP_METALGLOSSMAP_UV_CHANNEL = "_MetallicGlossMapUVChannel";
        private static readonly string PROP_METALGLOSSMAP_UV_ROTATION = "_MetallicGlossMapRotation";
        private static readonly string PROP_OCCLUSIONMAP_UV_CHANNEL = "_OcclusionMapUVChannel";
        private static readonly string PROP_OCCLUSIONMAP_UV_ROTATION = "_OcclusionMapRotation";
        private static readonly string PROP_EMISSIONMAP_UV_CHANNEL = "_EmissionMapUVChannel";
        private static readonly string PROP_EMISSIONMAP_UV_ROTATION = "_EmissionMapRotation";
        private static readonly string PROP_TRANSMISSIONMAP_ENABLED = "_Transmission";
        private static readonly string PROP_TRANSMISSIONMAP_FACTOR = "_TransmissionFactor";
        private static readonly string PROP_TRANSMISSIONMAP = "_TransmissionMap";
        private static readonly string PROP_TRANSMISSIONMAP_UV_CHANNEL = "_TransmissionMapUVChannel";
        private static readonly string PROP_TRANSMISSIONMAP_UV_ROTATION = "_TransmissionMapRotation";

        public const string KW_UV_ROTATION = "_UV_ROTATION";

        public enum UVChannel
        {
            UV0,
            UV1,
        }

        public struct GLTFLitProperties
        {
            public MaterialProperty baseMapUVChannelProp;
            public MaterialProperty baseMapRotationProp;
            public MaterialProperty metalGlossMapUVChannelProp;
            public MaterialProperty metalGlossMapRotationProp;
            public MaterialProperty occlusionMapUVChannelProp;
            public MaterialProperty occlusionMapRotationProp;
            public MaterialProperty emissionMapUVChannelProp;
            public MaterialProperty emissionMapRotationProp;
            public MaterialProperty transmissionEnabledProp;
            public MaterialProperty transmissionFactorProp;
            public MaterialProperty transmissionMapProp;
            public MaterialProperty transmissionMapUVChannelProp;
            public MaterialProperty transmissionMapRotationProp;

            public GLTFLitProperties(MaterialProperty[] properties)
            {
                baseMapUVChannelProp = BaseShaderGUI.FindProperty(PROP_BASEMAP_UV_CHANNEL, properties, false);
                baseMapRotationProp = BaseShaderGUI.FindProperty(PROP_BASEMAP_UV_ROTATION, properties, false);
                metalGlossMapUVChannelProp = BaseShaderGUI.FindProperty(PROP_METALGLOSSMAP_UV_CHANNEL, properties, false);
                metalGlossMapRotationProp = BaseShaderGUI.FindProperty(PROP_METALGLOSSMAP_UV_ROTATION, properties, false);
                occlusionMapUVChannelProp = BaseShaderGUI.FindProperty(PROP_OCCLUSIONMAP_UV_CHANNEL, properties, false);
                occlusionMapRotationProp = BaseShaderGUI.FindProperty(PROP_OCCLUSIONMAP_UV_ROTATION, properties, false);
                emissionMapUVChannelProp = BaseShaderGUI.FindProperty(PROP_EMISSIONMAP_UV_CHANNEL, properties, false);
                emissionMapRotationProp = BaseShaderGUI.FindProperty(PROP_EMISSIONMAP_UV_ROTATION, properties, false);
                transmissionEnabledProp = BaseShaderGUI.FindProperty(PROP_TRANSMISSIONMAP_ENABLED, properties, false);
                transmissionFactorProp = BaseShaderGUI.FindProperty(PROP_TRANSMISSIONMAP_FACTOR, properties, false);
                transmissionMapProp = BaseShaderGUI.FindProperty(PROP_TRANSMISSIONMAP, properties, false);
                transmissionMapUVChannelProp = BaseShaderGUI.FindProperty(PROP_TRANSMISSIONMAP_UV_CHANNEL, properties, false);
                transmissionMapRotationProp = BaseShaderGUI.FindProperty(PROP_TRANSMISSIONMAP_UV_ROTATION, properties, false);
            }
        }

        public static void Inputs(GLTFLitProperties properties, MaterialEditor materialEditor, Material material)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("glTF shader properties", EditorStyles.boldLabel);

            // Base Map UV
            EditorGUILayout.Space(5);
            HandleDropdown<UVChannel>(materialEditor, properties.baseMapUVChannelProp, "Base Map UV Channel");
            materialEditor.VectorProperty(properties.baseMapRotationProp, "Base Map UV Rotation");

            // Metallic Glossiness Map UV
            EditorGUILayout.Space(5);
            HandleDropdown<UVChannel>(materialEditor, properties.metalGlossMapUVChannelProp, "Metallic Glossiness Map UV Channel");
            materialEditor.VectorProperty(properties.metalGlossMapRotationProp, "Metallic Glossiness Map UV Rotation");

            // Occlusion Map UV
            EditorGUILayout.Space(5);
            HandleDropdown<UVChannel>(materialEditor, properties.occlusionMapUVChannelProp, "Occlusion Map UV Channel");
            materialEditor.VectorProperty(properties.occlusionMapRotationProp, "Occlusion Map UV Rotation");

            // Emission Map UV
            EditorGUILayout.Space(5);
            HandleDropdown<UVChannel>(materialEditor, properties.emissionMapUVChannelProp, "Emission Map UV Channel");
            materialEditor.VectorProperty(properties.emissionMapRotationProp, "Emission Map UV Rotation");

            // Transmission
            EditorGUILayout.Space(5);
            HandleToggle(properties.transmissionEnabledProp, "Transmission");
            if(properties.transmissionEnabledProp.floatValue > 0)
            {
                materialEditor.RangeProperty(properties.transmissionFactorProp, "Transmission Factor");
                materialEditor.TextureProperty(properties.transmissionMapProp, "Transmission Map");
                HandleDropdown<UVChannel>(materialEditor, properties.transmissionMapUVChannelProp, "Transmission Map UV Channel");
                materialEditor.VectorProperty(properties.transmissionMapRotationProp, "Transmission Map UV Rotation");
            }

            EditorGUILayout.Space(10);
        }

        public static void SetMaterialKeywords(Material material)
        {
            Vector4 baseMapRot = material.GetVector(PROP_BASEMAP_UV_ROTATION);
            Vector4 metalMapRot = material.GetVector(PROP_METALGLOSSMAP_UV_ROTATION);
            Vector4 emissionMapRot = material.GetVector(PROP_EMISSIONMAP_UV_ROTATION);
            Vector4 transmissionMapRot = material.GetVector(PROP_TRANSMISSIONMAP_UV_ROTATION);
            bool isRotation = baseMapRot.x != 0f || baseMapRot.y != 0f || 
                                metalMapRot.x != 0f || metalMapRot.y != 0f || 
                                emissionMapRot.x != 0f || emissionMapRot.y != 0f ||
                                transmissionMapRot.x != 0f || transmissionMapRot.y != 0f;
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

        private static void HandleToggle(MaterialProperty property, string name)
        {
            EditorGUI.BeginChangeCheck();

            bool enabled = property.floatValue > 0f;
            enabled = EditorGUILayout.Toggle(name, enabled);

            if (EditorGUI.EndChangeCheck())
                property.floatValue = enabled ? 1f : 0f;
        }
    }
}
