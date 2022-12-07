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
