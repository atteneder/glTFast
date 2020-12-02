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

using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace GLTFast.Tests {

    public class SampleModelsTest {

        [Test]
        public void CheckFiles()
        {
            var sampleSet = AssetDatabase.LoadAssetAtPath<GltfSampleSet>("Assets/SampleSets/glTF-Sample-Models.asset");
            Assert.AreEqual(174, sampleSet.itemCount);

            foreach (var item in sampleSet.GetItems()) {
                CheckFileExists(item.path);
            }
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

        [UnityTest]
        [UseGltfSampleSetTestCase("Assets/SampleSets/glTF-Sample-Models.asset")]
        public IEnumerator Load(GltfSampleSetItem testCase)
        {
            Debug.LogFormat("Testing {0}", testCase);

            var deferAgent = new UninterruptedDeferAgent();
        
            var path = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
			"{0}"
#else
                "file://{0}"
#endif
                ,testCase.path
            );

            Debug.LogFormat("Testing {0}", path);

            var go = new GameObject();
            var gltfAsset = go.AddComponent<GltfAsset>();

            bool done = false;

            gltfAsset.onLoadComplete += (asset,success) => { done = true; Assert.IsTrue(success); };
            gltfAsset.loadOnStartup = false;
            gltfAsset.Load(path,null,deferAgent);

            while (!done)
            {
                yield return null;
            }
            Object.Destroy(go);
        }
    }
}
