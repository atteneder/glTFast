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
