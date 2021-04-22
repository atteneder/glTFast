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

using System.Collections.Generic;
using System.IO;
using GLTFast.Editor;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace GLTFast {

    [CustomEditor(typeof(GltfImporter))]
    // [CanEditMultipleObjects]
    public class GltfImporterEditor : ScriptedImporterEditor
    {
        // Stored SerializedProperty to draw in OnInspectorGUI.
        SerializedProperty m_AssetDependencies;

        // protected override bool needsApplyRevert => false;
        
        public override void OnEnable()
        {
            base.OnEnable();
            // Once in OnEnable, retrieve the serializedObject property and store it.
            m_AssetDependencies = serializedObject.FindProperty("assetDependencies");
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedObject in case it has been changed outside the Inspector.
            serializedObject.Update();

            DrawDefaultInspector();

            var numDeps = m_AssetDependencies.arraySize;
            
            var malicousTextureImporters = new List<TextureImporter>();
            
            for (int i = 0; i < numDeps; i++) {
                var x = m_AssetDependencies.GetArrayElementAtIndex(i);
                var assetPathProp = x.FindPropertyRelative("assetPath");
                
                var typeProp = x.FindPropertyRelative("type");
                var type = (GltfAssetDependency.Type)typeProp.enumValueIndex;
                if (type == GltfAssetDependency.Type.Texture) {
                    var importer = AssetImporter.GetAtPath(assetPathProp.stringValue) as TextureImporter;
                    if (importer!=null) {
                        if (importer.textureShape != TextureImporterShape.Texture2D) {
                            malicousTextureImporters.Add(importer);
                            var nameWithoutExtension = Path.GetFileNameWithoutExtension(assetPathProp.stringValue);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"Texture {nameWithoutExtension}");
                            if (GUILayout.Button("Fix Texture Type")) {
                                importer.textureShape = TextureImporterShape.Texture2D;
                                importer.SaveAndReimport();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }

            if (malicousTextureImporters.Count>0) {
                EditorGUILayout.HelpBox("Some textures couldn't be assigned since they were not 2D textures! To resolve this press the fix button",MessageType.Error);
                if (GUILayout.Button("Fix All")) {
                    foreach (var malicousTextureImporter in malicousTextureImporters) {
                        malicousTextureImporter.textureShape = TextureImporterShape.Texture2D;
                    }
                    foreach (var malicousTextureImporter in malicousTextureImporters) {
                        malicousTextureImporter.SaveAndReimport();
                    }
                    AssetDatabase.Refresh();
                }
            }

            // Apply the changes so Undo/Redo is working
            serializedObject.ApplyModifiedProperties();

            // Call ApplyRevertGUI to show Apply and Revert buttons.
            ApplyRevertGUI();
        }
    }
}
