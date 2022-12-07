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
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GLTFast.Editor
{

    [CustomEditor(typeof(InstantiationSettings))]
    class InstantiationSettingsEditor : UnityEditor.Editor
    {

        VisualElement m_Container;

        public override VisualElement CreateInspectorGUI()
        {
            m_Container = new VisualElement();
            CreateUI(serializedObject, m_Container);
            return m_Container;
        }

        internal static void CreateUI(SerializedObject serializedObject, VisualElement container, string pathPrefix = "")
        {
            var prop = serializedObject.FindProperty($"{pathPrefix}mask");
            Utils.CreateProperty(container, prop, "Components");

            prop = serializedObject.FindProperty($"{pathPrefix}layer");
            var layerField = new LayerField("Destination Layer");

#if UNITY_2021_2_OR_NEWER
            layerField.AddToClassList(BaseField<LayerField>.alignedFieldUssClassName);
#endif
            layerField.BindProperty(prop);
            container.Add(layerField);

            var properties = new[] {
                serializedObject.FindProperty($"{pathPrefix}skinUpdateWhenOffscreen"),
                serializedObject.FindProperty($"{pathPrefix}lightIntensityFactor"),
                serializedObject.FindProperty($"{pathPrefix}sceneObjectCreation"),
            };
            Utils.CreateProperties(container, properties);
        }
    }
}
