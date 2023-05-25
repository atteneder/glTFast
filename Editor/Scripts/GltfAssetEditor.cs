// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GLTFast.Editor
{

    [CustomEditor(typeof(GltfAsset)), CanEditMultipleObjects]
    class GltfAssetInspector : UnityEditor.Editor
    {
        VisualElement m_Container;

        public override VisualElement CreateInspectorGUI()
        {
            m_Container = new VisualElement();
            // Create properties in this particular order
            var properties = new[] {
                serializedObject.FindProperty("url"),
                serializedObject.FindProperty("streamingAsset"),
                serializedObject.FindProperty("loadOnStartup"),
                serializedObject.FindProperty("sceneId"),
                serializedObject.FindProperty("playAutomatically"),
            };

            Utils.CreateProperties(m_Container, properties);

            // Import Settings Foldout
            var importSettingsFoldout = new Foldout { text = "Import Settings", value = true };
            ImportSettingsEditor.CreateUI(serializedObject, importSettingsFoldout, "importSettings.");
            m_Container.Add(importSettingsFoldout);

            // Instantiation Settings Foldout
            var instantiationSettingsFoldout = new Foldout { text = "Instantiation Settings", value = true };
            InstantiationSettingsEditor.CreateUI(serializedObject, instantiationSettingsFoldout, "instantiationSettings.");
            m_Container.Add(instantiationSettingsFoldout);

            return m_Container;
        }
    }
}
