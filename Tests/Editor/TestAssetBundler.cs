// SPDX-FileCopyrightText: 2023 Unity Technologies and the Draco for Unity authors
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using GLTFast.Tests.Import;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

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
            }
        }

        static void SyncAssets()
        {
            var testCaseSetGuids = AssetDatabase.FindAssets(
                "t:GltfTestCaseSet",
                new[] { $"Packages/{GltfGlobals.GltfPackageName}/Tests/Runtime/TestCaseSets" }
                );
            if (testCaseSetGuids?.Length < 1)
            {
                throw new InvalidDataException("No glTF test case set was found!");
            }
            foreach (var guid in testCaseSetGuids)
            {
                var testCaseSet = AssetDatabase.LoadAssetAtPath<GltfTestCaseSet>(AssetDatabase.GUIDToAssetPath(guid));
                testCaseSet.SerializeToStreamingAssets();
            }

            AssetDatabase.Refresh();
        }
    }
}
