// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
