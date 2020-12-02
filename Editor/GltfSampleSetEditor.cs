// Copyright 2020 Andreas Atteneder
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

[CustomEditor(typeof(GltfSampleSet))]
public class GltfSampleSetEditor : Editor
{
    private GltfSampleSet _sampleSet;
    private string searchPattern = "*.gl*";

    public void OnEnable() {
        _sampleSet = (GltfSampleSet)target;
    }

    public override void OnInspectorGUI() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Search Pattern");
        searchPattern = GUILayout.TextField(searchPattern);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Find in path")) {
            _sampleSet.LoadItemsFromPath(searchPattern);
        }
        base.OnInspectorGUI();
        
        if (GUI.changed) {
            EditorUtility.SetDirty(_sampleSet);
        }
    }
}