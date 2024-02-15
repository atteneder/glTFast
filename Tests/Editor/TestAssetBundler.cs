// SPDX-FileCopyrightText: 2023 Unity Technologies and the Draco for Unity authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;
using GLTFast.Tests.Import;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace GLTFast.Editor.Tests
{
    public class TestAssetBundler : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport target)
        {
            if ((target.summary.options & BuildOptions.IncludeTestAssemblies) != 0)
            {
                SyncAssets();
                AddShaderVariantCollections();
            }
        }

        static void SyncAssets()
        {
            var streamingAssets = Application.streamingAssetsPath;
            if (!Directory.Exists(streamingAssets))
            {
                Directory.CreateDirectory(streamingAssets);
            }
            foreach (var testCaseSet in IterateAllGltfTestCaseSets())
            {
                testCaseSet.SerializeToStreamingAssets();
                if (GltfTestCaseSet.IsStreamingAssetsPlatform)
                {
                    testCaseSet.CopyToStreamingAssets();
                }
            }

            AssetDatabase.Refresh();
        }

        static IEnumerable<GltfTestCaseSet> IterateAllGltfTestCaseSets()
        {
            var testCaseSetGuids = FindAssets("t:GltfTestCaseSet", "/Tests/Runtime/TestCaseSets");
            if (testCaseSetGuids == null || testCaseSetGuids.Length < 1)
            {
                throw new InvalidDataException("No glTF test case set was found!");
            }
            foreach (var guid in testCaseSetGuids)
            {
                yield return AssetDatabase.LoadAssetAtPath<GltfTestCaseSet>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        static IEnumerable<ShaderVariantCollection> IterateAllShaderVariantCollections()
        {
            var guids = FindAssets("t:ShaderVariantCollection", "/Tests/Runtime/TestCaseSets");
            if (guids == null || guids.Length < 1)
            {
                throw new InvalidDataException("No shader variant collection was found!");
            }
            foreach (var guid in guids)
            {
                yield return AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        static void AddShaderVariantCollections()
        {
            var settings = GraphicsSettings.GetGraphicsSettings();
            var obj = new SerializedObject(settings);
            var preloadedShaders = obj.FindProperty("m_PreloadedShaders");

            foreach (var svc in IterateAllShaderVariantCollections())
            {
                var found = false;
                var arraySize = preloadedShaders.arraySize;
                for (var i = 0; i < arraySize; i++)
                {
                    var e = preloadedShaders.GetArrayElementAtIndex(i);
                    if (e.objectReferenceValue == svc)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    preloadedShaders.InsertArrayElementAtIndex(arraySize);
                    var entry = preloadedShaders.GetArrayElementAtIndex(arraySize);
                    entry.objectReferenceValue = svc;
                }
            }

            obj.ApplyModifiedProperties();
        }

        static string[] FindAssets(string filter, string inPackageLocation)
        {
            var guids = AssetDatabase.FindAssets(
                filter,
                new[] { $"Packages/{GltfGlobals.GltfPackageName}{inPackageLocation}" }
            );
            if (guids?.Length < 1)
            {
                // Try again with separate tests package
                guids = AssetDatabase.FindAssets(
                    filter,
                    new[] { $"Packages/{GltfGlobals.GltfPackageName}.tests{inPackageLocation}" }
                );
            }

            return guids;
        }
    }
}
