// SPDX-FileCopyrightText: 2023 Unity Technologies and the Draco for Unity authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;
using GLTFast.Tests.Export;
using GLTFast.Tests.Import;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace GLTFast.Editor.Tests
{
    public class PreprocessBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        static string pkgPath => $"Packages/{GltfGlobals.GltfPackageName}";

        public void OnPreprocessBuild(BuildReport target)
        {
            if ((target.summary.options & BuildOptions.IncludeTestAssemblies) != 0)
            {
                SyncTestAssets();
                AddShaderVariantCollections();
                ExportTests.CertifyStreamingAssetsFolder();
                ExportTests.SetupTests();
                CopyExportTargetsToStreamingAssets();
                AssetDatabase.Refresh();
            }
        }

        static void SyncTestAssets()
        {
            var streamingAssets = Application.streamingAssetsPath;
            if (!Directory.Exists(streamingAssets))
            {
                Directory.CreateDirectory(streamingAssets);
            }
            foreach (var testCaseSet in IterateAssets<GltfTestCaseSet>("Tests/Runtime/TestCaseSets"))
            {
                testCaseSet.SerializeToStreamingAssets();
                if (GltfTestCaseSet.IsStreamingAssetsPlatform)
                {
                    testCaseSet.CopyToStreamingAssets();
                }
            }

            AssetDatabase.Refresh();
        }

        static IEnumerable<T> IterateAssets<T>(string inPackageLocation) where T : Object
        {
            var guids = FindAssets($"t:{typeof(T).Name}", inPackageLocation);
            if (guids == null || guids.Length < 1)
            {
                throw new InvalidDataException($"No {typeof(T).Name} asset set was found in {inPackageLocation}!");
            }
            foreach (var guid in guids)
            {
                yield return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        static IEnumerable<ShaderVariantCollection> IterateAllShaderVariantCollections()
        {
            foreach (var collection in IterateAssets<ShaderVariantCollection>("Tests/Runtime/TestCaseSets"))
            {
                yield return collection;
            }

            // Shaders required for export tests.
            var export = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>($"{pkgPath}/Runtime/Shader/Export/glTFExport.shadervariants");
            Assert.IsNotNull(export);
            yield return export;
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

        static void CopyExportTargetsToStreamingAssets()
        {
            const string inPackageLocation = "Tests/Resources/ExportTargets";
            var sourceRoot = $"{pkgPath}/{inPackageLocation}";
            ExportTests.TryFixPackageAssetPath(ref sourceRoot);
            var exportTargetFolder = $"Assets/StreamingAssets/{ExportTests.exportTargetFolder}";
            if (!AssetDatabase.IsValidFolder(exportTargetFolder))
            {
                AssetDatabase.CreateFolder("Assets/StreamingAssets", ExportTests.exportTargetFolder);
            }

            foreach (var target in IterateAssets<TextAsset>(inPackageLocation))
            {
                var sourcePath = AssetDatabase.GetAssetPath(target);
                Assert.IsTrue(sourcePath.StartsWith(sourceRoot), $"{sourcePath} is not relative to {sourceRoot}");
                var relativePath = sourcePath.Substring(sourceRoot.Length + 1);
                Debug.Log($"Relative export target path {relativePath}\n{sourcePath}\n{sourceRoot}");
                var destinationPath = Path.Combine(exportTargetFolder, relativePath);
                var destinationDir = Path.GetDirectoryName(destinationPath);
                Assert.IsFalse(string.IsNullOrEmpty(destinationDir));
                Directory.CreateDirectory(destinationDir);
                File.Copy(sourcePath, destinationPath, true);
            }
        }

        static string[] FindAssets(string filter, string inPackageLocation)
        {
            var guids = AssetDatabase.FindAssets(
                filter,
                new[] { $"{pkgPath}/{inPackageLocation}" }
            );
            if (guids?.Length < 1)
            {
                // Try again with separate tests package
                guids = AssetDatabase.FindAssets(
                    filter,
                    new[] { $"{pkgPath}.tests/{inPackageLocation}" }
                );
            }

            return guids;
        }
    }
}
