// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GLTFast.Editor
{

    [CustomEditor(typeof(ImportSettings))]
    class ImportSettingsEditor : UnityEditor.Editor
    {

        VisualElement m_Container;

        public override VisualElement CreateInspectorGUI()
        {
            m_Container = new VisualElement();
            CreateUI(serializedObject, m_Container);
            return m_Container;
        }

        internal static void CreateUI(SerializedObject serializedObject, VisualElement container, string pathPrefix = "", bool importerContext = false)
        {

            Utils.CreateProperty(container, serializedObject.FindProperty($"{pathPrefix}nodeNameMethod"));
            if (!importerContext)
            {
                Utils.CreateProperty(container, serializedObject.FindProperty($"{pathPrefix}animationMethod"));
            }

            // Group texture related properties in a foldout
            var properties = new[] {
                serializedObject.FindProperty($"{pathPrefix}generateMipMaps"),
                serializedObject.FindProperty($"{pathPrefix}defaultMinFilterMode"),
                serializedObject.FindProperty($"{pathPrefix}defaultMagFilterMode"),
                serializedObject.FindProperty($"{pathPrefix}anisotropicFilterLevel"),
            };
            var foldout = new Foldout { text = "Textures", value = true };
            Utils.CreateProperties(foldout, properties);
            container.Add(foldout);
        }
    }
}
