// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;

[assembly: InternalsVisibleTo("glTFast.Editor.Tests")]

namespace GLTFast.Tests.Import
{
    [CreateAssetMenu(fileName = "glTF-TestCaseCollection", menuName = "ScriptableObjects/glTFast Test Case Collection")]
    class GltfTestCaseSet : ScriptableObject
    {
        /// <summary>
        /// Path relative to "Assets", a folder at root level of the repository.
        /// </summary>
        public string assetsRelativePath;

        public string assetsAbsolutePath;

        [SerializeField]
        GltfTestCase[] m_TestCases;

        public int TestCaseCount => m_TestCases?.Length ?? 0;

        public IEnumerable<GltfTestCase> IterateTestCases()
        {
            foreach (var testCase in m_TestCases)
            {
                yield return testCase;
            }
        }

        public string GetSourcePath()
        {
#if UNITY_EDITOR
            return Path.Combine(GetAssetsPath(), assetsRelativePath);
#else
            return assetsAbsolutePath;
#endif
        }

        public void SerializeToStreamingAssets()
        {
            assetsAbsolutePath = GetSourcePath();
            var jsonPathAbsolute = Path.Combine(Application.streamingAssetsPath, $"{name}.json");
            File.WriteAllText(jsonPathAbsolute, ToJson());
        }

        public static GltfTestCaseSet DeserializeFromStreamingAssets(string path)
        {
            var json = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, path));
            var sampleSet = CreateInstance<GltfTestCaseSet>();
            JsonUtility.FromJsonOverwrite(json, sampleSet);
            return sampleSet;
        }

        string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        [ContextMenu("Scan for glTF test files")]
        public void ScanAndUpdateGltfTestCases()
        {
            ScanAndUpdateGltfTestCases("*.gl*");
        }

        void ScanAndUpdateGltfTestCases(string searchPattern)
        {
            var dir = new DirectoryInfo(GetSourcePath());
            var dirLength = dir.FullName.Length + 1;

            var newTestCases = new List<GltfTestCase>();

            foreach (var file in dir.GetFiles(searchPattern, SearchOption.AllDirectories))
            {
                var ext = file.Extension;
                if (ext != ".gltf" && ext != ".glb") continue;
                // var i = CreateInstance<GltfTestCase>();
                var i = new GltfTestCase();
                i.relativeUri = file.FullName.Substring(dirLength);
                newTestCases.Add(i);
            }

            m_TestCases = newTestCases.ToArray();
        }

        /// <summary>
        /// Get assets path from the root level of the glTF Test repository
        /// Can be overriden via GLTF_TEST_ASSET_DIR environment variable
        /// </summary>
        /// <returns>Path to glTFastTest project specific assets folder</returns>
        static string GetAssetsPath()
        {
            var assetDir = Environment.GetEnvironmentVariable("GLTF_TEST_ASSET_DIR");
            if (!string.IsNullOrEmpty(assetDir))
            {
                if (Directory.Exists(assetDir))
                {
                    return assetDir;
                }
                throw new InvalidDataException($"GLTF_TEST_ASSET_DIR at {assetDir} is not a valid directory!");
            }
            var dir = new DirectoryInfo(Application.dataPath); // Assets
            dir = dir.Parent; // Project directory
            dir = dir?.Parent; // Projects directory
            dir = dir?.Parent; // Repository root directory
            if (dir != null)
            {
                var path = Path.Combine(dir.FullName, "Assets");
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
    }
}
