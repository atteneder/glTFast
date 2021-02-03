// Copyright 2020-2021 Andreas Atteneder
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

namespace GLTFast.Editor {

    using Samples;

    [CustomPropertyDrawer(typeof(SampleSetItemEntry))]
    public class SampleSetItemEntryDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var active = property.FindPropertyRelative(nameof(SampleSetItemEntry.active));
            var item = property.FindPropertyRelative(nameof(SampleSetItemEntry.item));

            EditorGUI.BeginProperty(position, label, property);
            {
                float x = 30;
                float width = position.width - x;
                EditorGUI.BeginChangeCheck();
                var newActive = EditorGUI.Toggle(
                    new Rect(position.x, position.y, x, position.height),
                    active.boolValue
                );
                if (EditorGUI.EndChangeCheck())
                    active.boolValue = newActive;

                EditorGUI.BeginDisabledGroup(!active.boolValue);
                {
                    float nameWidth = width * 0.4f;
                    var nameProp = item.FindPropertyRelative(nameof(SampleSetItem.name));
                    nameProp.stringValue = EditorGUI.TextField(
                        new Rect(position.x + x, position.y, nameWidth, position.height),
                        nameProp.stringValue
                    );
                    width -= nameWidth;
                    x += nameWidth;

                    var pathProp = item.FindPropertyRelative(nameof(SampleSetItem.path));
                    pathProp.stringValue = EditorGUI.TextField(
                        new Rect(position.x + x, position.y, width, position.height),
                        pathProp.stringValue
                    );
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndProperty();
        }
    }
}
