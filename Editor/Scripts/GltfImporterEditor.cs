// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if !GLTFAST_EDITOR_IMPORT_OFF

using System;
using System.Collections.Generic;
using System.IO;
using GLTFast.Editor;
using UnityEditor;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GLTFast
{

    using Logging;

    [CustomEditor(typeof(GltfImporter))]
    // [CanEditMultipleObjects]
    class GltfImporterEditor : ScriptedImporterEditor
    {

        // To be assigned defaults from the inspector
        [SerializeField] VisualTreeAsset mainMarkup; // Editor/UI/GltfImporter.uxml
        [SerializeField] VisualTreeAsset reportItemMarkup; // Editor/UI/ReportItem.uxml
        [SerializeField] VisualTreeAsset dependencyMarkup; // Editor/UI/Dependency.uxml

        SerializedProperty m_AssetDependencies;
        SerializedProperty m_ReportItems;

        public override void OnEnable()
        {
            base.OnEnable();
            m_AssetDependencies = serializedObject.FindProperty("assetDependencies");
            m_ReportItems = serializedObject.FindProperty("reportItems");
        }

        public override VisualElement CreateInspectorGUI()
        {

            // Update the serializedObject in case it has been changed outside the Inspector.
            serializedObject.Update();

            var root = new VisualElement();

            mainMarkup.CloneTree(root);

            var numDeps = m_AssetDependencies.arraySize;

            var maliciousTextureImporters = new List<TextureImporter>();

            var reportItemCount = m_ReportItems.arraySize;

#if !UNITY_ANIMATION
            var animRoot = root.Query<VisualElement>(name: "Animation").First();
            animRoot.SetEnabled(false);
#endif

            var reportRoot = root.Query<VisualElement>(name: "Report").First();

            if (reportItemCount > 0)
            {
                // var reportList = new List<ReportItem>
                var reportList = reportRoot.Query<ListView>().First();
                // reportList.bindingPath = nameof(m_ReportItems);
                reportList.makeItem = () => reportItemMarkup.CloneTree();
                reportList.bindItem = (element, i) =>
                {
                    if (i >= reportItemCount)
                    {
                        element.style.display = DisplayStyle.None;
                        return;
                    }
                    var msg = element.Q<Label>("Message");

                    var item = m_ReportItems.GetArrayElementAtIndex(i);

                    var typeProp = item.FindPropertyRelative("type");
                    var codeProp = item.FindPropertyRelative("code");
                    var messagesProp = item.FindPropertyRelative("messages");

                    var type = (LogType)typeProp.intValue;
                    var code = (LogCode)codeProp.intValue;

                    var icon = element.Q<VisualElement>("Icon");
                    switch (type)
                    {
                        case LogType.Error:
                        case LogType.Assert:
                        case LogType.Exception:
                            icon.RemoveFromClassList("info");
                            icon.AddToClassList("error");
                            break;
                        case LogType.Warning:
                            icon.RemoveFromClassList("info");
                            icon.AddToClassList("warning");
                            break;
                    }

                    var messages = GetStringValues(messagesProp);
                    var reportItem = new LogItem(type, code, messages);
                    var reportItemText = reportItem.ToString();
                    msg.text = reportItemText;
                    element.tooltip = reportItemText;
                };
            }
            else
            {
                reportRoot.style.display = DisplayStyle.None;
            }

            for (int i = 0; i < numDeps; i++)
            {
                var x = m_AssetDependencies.GetArrayElementAtIndex(i);
                var assetPathProp = x.FindPropertyRelative("assetPath");

                var typeProp = x.FindPropertyRelative("type");
                var type = (GltfAssetDependency.Type)typeProp.enumValueIndex;
                if (type == GltfAssetDependency.Type.Texture)
                {
                    var importer = AssetImporter.GetAtPath(assetPathProp.stringValue) as TextureImporter;
                    if (importer != null)
                    {
                        if (importer.textureShape != TextureImporterShape.Texture2D)
                        {
                            maliciousTextureImporters.Add(importer);
                        }
                    }
                }
            }

            if (maliciousTextureImporters.Count > 0)
            {

                root.Query<Button>("fixall").First().clickable.clicked += () =>
                {
                    AssetDatabase.StartAssetEditing();
                    foreach (var maliciousTextureImporter in maliciousTextureImporters)
                    {
                        FixTextureImportSettings(maliciousTextureImporter);
                    }
                    AssetDatabase.StopAssetEditing();
                    Repaint();
                };

                var foldout = root.Query<Foldout>().First();
                // var row = root.Query<VisualElement>(className: "fix-texture-row").First();
                foreach (var maliciousTextureImporter in maliciousTextureImporters)
                {
                    var row = dependencyMarkup.CloneTree();
                    foldout.Add(row);
                    // textureRowTree.CloneTree(foldout);
                    var path = AssetDatabase.GetAssetPath(maliciousTextureImporter);
                    row.Query<Label>().First().text = Path.GetFileName(path);
                    row.Query<Button>().First().clickable.clicked += () =>
                    {
                        FixTextureImportSettings(maliciousTextureImporter);
                        row.style.display = DisplayStyle.None;
                    };
                }
            }
            else
            {
                var depRoot = root.Query<VisualElement>("Dependencies").First();
                depRoot.style.display = DisplayStyle.None;
            }

            root.Bind(serializedObject);

            var settings = root.Query<VisualElement>(name: "AdvancedSettings").First();
            ImportSettingsEditor.CreateUI(serializedObject, settings.contentContainer, "importSettings.", true);
            InstantiationSettingsEditor.CreateUI(serializedObject, settings.contentContainer, "instantiationSettings.");

            // Apply the changes so Undo/Redo is working
            serializedObject.ApplyModifiedProperties();

            root.Add(new IMGUIContainer(ApplyRevertGUI));

            return root;
        }

        static void FixTextureImportSettings(TextureImporter maliciousTextureImporter)
        {
            maliciousTextureImporter.textureShape = TextureImporterShape.Texture2D;
            maliciousTextureImporter.SaveAndReimport();
        }

        static string[] GetStringValues(SerializedProperty property)
        {
            if (!property.isArray || property.arraySize < 1) return null;
            var result = new string[property.arraySize];
            for (var i = 0; i < property.arraySize; i++)
            {
                result[i] = property.GetArrayElementAtIndex(i).stringValue;
            }
            return result;
        }
    }
}

#endif // !GLTFAST_EDITOR_IMPORT_OFF
