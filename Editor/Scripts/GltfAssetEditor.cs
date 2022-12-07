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
