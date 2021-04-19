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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GLTFast.Tests {

    using Samples;
    using Utils;

    public class SampleModelsTest {
        const int k_Repetitions = 10;
        
        const string glTFSampleSetAssetPath = "Assets/SampleSets/glTF-Sample-Models.asset";
        const string glTFSampleSetJsonPath = "glTF-Sample-Models.json";
        const string glTFSampleSetBinaryJsonPath = "glTF-Sample-Models-glb.json";

        [Test]
        public void CheckFiles()
        {
#if UNITY_EDITOR
            var sampleSet = AssetDatabase.LoadAssetAtPath<SampleSet>(glTFSampleSetAssetPath);
            Assert.AreEqual(182, sampleSet.itemCount);

            foreach (var item in sampleSet.GetItemsPrefixed()) {
                CheckFileExists(item.path);
            }
#else
            Debug.Log("Editor only test");
#endif
        }

        void CheckFileExists(string path) {
#if !(UNITY_ANDROID && !UNITY_EDITOR)
            Assert.IsTrue(
                File.Exists(path)
                , "file {0} not found"
                , path
            );
#else
		    // See https://docs.unity3d.com/Manual/StreamingAssets.html
		    Debug.Log("File access doesn't work on Android");
#endif
        }

        /// <summary>
        /// Editor-only test to quickly load all models once
        /// </summary>
        [UnityTest]
        [UseGltfSampleSetTestCase(glTFSampleSetJsonPath)]
        public IEnumerator QuickLoad(SampleSetItem testCase)
        {
#if UNITY_EDITOR
            Debug.Log($"Testing {testCase.path}");
            var go = new GameObject();
            var deferAgent = new UninterruptedDeferAgent();
            var task = LoadGltfSampleSetItem(testCase, go, deferAgent);
            yield return WaitForTask(task);
            Object.Destroy(go);
#else
            yield break;            
#endif
        }

        [UnityTest]
        [UseGltfSampleSetTestCase(glTFSampleSetJsonPath)]
        [Performance]
        public IEnumerator UninterruptedLoading(SampleSetItem testCase)
        {
            Debug.Log($"Testing {testCase.path}");
            var go = new GameObject();
            var deferAgent = new UninterruptedDeferAgent();
            SampleGroup loadTime = new SampleGroup("LoadTime", SampleUnit.Millisecond);
            // First time without measuring
            var task = LoadGltfSampleSetItem(testCase, go, deferAgent, loadTime);
            yield return WaitForTask(task);
            using (Measure.Frames().Scope()) {
                for (int i = 0; i < k_Repetitions; i++) {
                    task = LoadGltfSampleSetItem(testCase, go, deferAgent, loadTime);
                    yield return WaitForTask(task);
                }
            }
            
            Object.Destroy(go);
        }

        [UnityTest]
        [UseGltfSampleSetTestCase(glTFSampleSetJsonPath)]
        [Performance]
        public IEnumerator SmoothLoading(SampleSetItem testCase)
        {
            Debug.Log($"Testing {testCase.path}");
            var go = new GameObject();
            var deferAgent = go.AddComponent<TimeBudgetPerFrameDeferAgent>();
            SampleGroup loadTime = new SampleGroup("LoadTime", SampleUnit.Millisecond);
            // First time without measuring
            var task = LoadGltfSampleSetItem(testCase, go, deferAgent);
            yield return WaitForTask(task);
            using (Measure.Frames().Scope()) {
                for (int i = 0; i < k_Repetitions; i++) {
                    task = LoadGltfSampleSetItem(testCase, go, deferAgent, loadTime);
                    yield return WaitForTask(task);
                    // Wait one more frame. Usually some more action happens in this one.
                    yield return null;
                }
            }
            Object.Destroy(go);
        }
        
        /// <summary>
        /// Load glTF-binary files from memory
        /// </summary>
        [UnityTest]
        [UseGltfSampleSetTestCase(glTFSampleSetBinaryJsonPath)]
        public IEnumerator LoadGlbFromMemory(SampleSetItem testCase)
        {
            Debug.Log($"Testing {testCase.path}");
            var data = File.ReadAllBytes(testCase.path);
            var go = new GameObject();
            var deferAgent = new UninterruptedDeferAgent();
            var gltf = new GltfImport();
            var task = gltf.LoadGltfBinary(data, new Uri(testCase.path));
            yield return WaitForTask(task);
            var success = task.Result;
            Assert.IsTrue(success);
            success = gltf.InstantiateGltf(go.transform);
            Assert.IsTrue(success);
            Object.Destroy(go);
        }

        async Task LoadGltfSampleSetItem(SampleSetItem testCase, GameObject go, IDeferAgent deferAgent, SampleGroup loadTime = null)
        {
            var path = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
			    "{0}"
#else
                "file://{0}"
#endif
                ,testCase.path
            );

            // Debug.LogFormat("Testing {0}", path);
            
            var gltfAsset = go.AddComponent<GltfAsset>();
            var stopWatch = go.AddComponent<StopWatch>();
            stopWatch.StartTime();

            gltfAsset.loadOnStartup = false;
            var success = await gltfAsset.Load(path,null,deferAgent);
            Assert.IsTrue(success);
            
            stopWatch.StopTime();

            if (loadTime != null) {
                Measure.Custom(loadTime, stopWatch.lastDuration);
            }
        }
        
        static IEnumerator WaitForTask(Task task) {
            while(!task.IsCompleted) {
                if (task.Exception != null)
                    throw task.Exception;
                yield return null;
            }
            if (task.Exception != null)
                throw task.Exception;
        }
    }
}
