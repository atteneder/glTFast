// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0


#if UNITY_ANDROID || UNITY_IOS || UNITY_TVOS || UNITY_VISIONOS || UNITY_WEBGL || UNITY_WSA || UNITY_LUMIN
#define STREAMING_ASSETS_PLATFORM
#endif

#if !UNITY_EDITOR && STREAMING_ASSETS_PLATFORM
#define LOAD_FROM_STREAMING_ASSETS
#endif

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if LOAD_FROM_STREAMING_ASSETS
using UnityEngine.Networking;
#endif

namespace GLTFast.Tests.Import
{
    [CreateAssetMenu(fileName = "glTF-TestCaseCollection", menuName = "ScriptableObjects/glTFast Test Case Collection")]
    class GltfTestCaseSet : ScriptableObject
    {
        public static bool IsStreamingAssetsPlatform =>
#if STREAMING_ASSETS_PLATFORM
            true;
#else
            false;
#endif

        /// <summary>
        /// Path relative to "Assets", a folder at root level of the repository.
        /// </summary>
        public string assetsRelativePath;

        public string assetsAbsolutePath;

        [SerializeField]
        GltfTestCase[] m_TestCases;

        public int TestCaseCount => m_TestCases?.Length ?? 0;

        public string streamingAssetsPath => $"gltfast/{assetsRelativePath}";

        public IEnumerable<GltfTestCase> IterateTestCases()
        {
            foreach (var testCase in m_TestCases)
            {
                yield return testCase;
            }
        }

        public static GltfTestCaseSet DeserializeFromStreamingAssets(string path)
        {
            var fullPath = Path.Combine(Application.streamingAssetsPath, path);
#if LOAD_FROM_STREAMING_ASSETS
            var request = UnityWebRequest.Get(fullPath);
            var it = request.SendWebRequest();
            while(!it.isDone) {}

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new IOException($"Loading GltfTestCaseSet from {fullPath} failed!");
            }

            var json = request.downloadHandler.text;
#else
            var json = File.ReadAllText(fullPath);
#endif
            var sampleSet = CreateInstance<GltfTestCaseSet>();
            JsonUtility.FromJsonOverwrite(json, sampleSet);
            return sampleSet;
        }

        public string RootPath =>
#if LOAD_FROM_STREAMING_ASSETS
            Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);
#else
#if UNITY_EDITOR
            SourcePath;
#else
            assetsAbsolutePath;
#endif
#endif

#if UNITY_EDITOR
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

        public string SourcePath => Path.Combine(GetAssetsPath(), assetsRelativePath);

        public void SerializeToStreamingAssets()
        {
#if !STREAMING_ASSETS_PLATFORM
            assetsAbsolutePath = SourcePath;
#endif
            var jsonPathAbsolute = Path.Combine(Application.streamingAssetsPath, $"{name}.json");
            File.WriteAllText(jsonPathAbsolute, ToJson());
        }

        public void CopyToStreamingAssets(bool force = false) {
            var srcPath = SourcePath;
            if (string.IsNullOrEmpty(srcPath) || !Directory.Exists(srcPath)) {
                Debug.LogError($"Invalid source path: \"{srcPath}\"");
                return;
            }

            var dstPath = Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);

            if (Directory.Exists(dstPath)) {
                if (force) {
                    Directory.Delete(dstPath);
                }
                else {
                    return;
                }
            }
            else {
                var parent = Directory.GetParent(dstPath).FullName;
                if (!Directory.Exists(parent)) {
                    Directory.CreateDirectory(parent);
                }
            }

            FileUtil.CopyFileOrDirectory(srcPath, dstPath);
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
            var dir = new DirectoryInfo(SourcePath);
            var dirLength = dir.FullName.Length + 1;

            var newTestCases = new List<GltfTestCase>();

            foreach (var file in dir.GetFiles(searchPattern, SearchOption.AllDirectories))
            {
                var ext = file.Extension;
                if (ext != ".gltf" && ext != ".glb") continue;
                // var i = CreateInstance<GltfTestCase>();
                var i = new GltfTestCase
                {
                    relativeUri = file.FullName.Substring(dirLength)
                };
                newTestCases.Add(i);
            }

            m_TestCases = newTestCases.ToArray();
        }
#endif
    }
}
