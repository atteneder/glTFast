// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GLTFast.Editor
{
    static class Utils
    {
        internal static void CreateProperties(VisualElement container, IEnumerable<SerializedProperty> properties)
        {
            foreach (var property in properties)
            {
                CreateProperty(container, property);
            }
        }

        internal static PropertyField CreateProperty(VisualElement container, SerializedProperty property, string label = null)
        {
            var propertyField = new PropertyField(property.Copy(), label) { name = "PropertyField:" + property.propertyPath };
            propertyField.BindProperty(property.Copy());
            container.Add(propertyField);
            return propertyField;
        }
    }
}
