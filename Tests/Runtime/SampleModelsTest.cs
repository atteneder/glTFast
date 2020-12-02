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

using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Networking;
using NUnit.Framework;
using System.Collections;
using System.IO;
using GLTFast;

public class SampleModelsTest
{
    const string prefix = "glTF-Sample-Models/2.0/";

    [UnityTest]
    public IEnumerator SampleModelsTestCheckFiles()
    {
        yield return GltfSampleModels.LoadGlbFileUrls();
        Assert.AreEqual(GltfSampleModels.glbFileUrls.Length,44);
        CheckFileExist(GltfSampleModels.glbFileUrls);

        yield return GltfSampleModels.LoadGltfFileUrls();
        Assert.AreEqual(GltfSampleModels.gltfFileUrls.Length,128);
        CheckFileExist(GltfSampleModels.gltfFileUrls);
    }

    void CheckFileExist(string[] files)
    {
#if !(UNITY_ANDROID && !UNITY_EDITOR)
        foreach (var path in files)
        {
            Assert.IsTrue(
                File.Exists(path)
                , "file {0} not found"
                , path
            );
        }
#else
		// See https://docs.unity3d.com/Manual/StreamingAssets.html
		Debug.Log("File access doesn't work on Android");
#endif
    }

    [UnityTest]
    [Timeout(1000000)]
    public IEnumerator SampleModelsTestLoadAllGlb()
    {
        yield return GltfSampleModels.LoadGlbFileUrls();

        var deferAgent = new UninterruptedDeferAgent();

        foreach (var file in GltfSampleModels.glbFileUrls)
        {
            var path = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
				"{0}"
#else
                "file://{0}"
#endif
                ,file
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

    [UnityTest]
    [Timeout(1000000)]
    public IEnumerator SampleModelsTestLoadAllGltf()
    {
        yield return GltfSampleModels.LoadGltfFileUrls();

        var deferAgent = new UninterruptedDeferAgent();

        foreach (var file in GltfSampleModels.gltfFileUrls)
        {
            var path = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
                "{0}"
#else
                "file://{0}"
#endif
                ,file
            );

            Debug.LogFormat("Testing {0}", path);

            var webRequest = UnityWebRequest.Get(path);
            yield return webRequest.SendWebRequest();
            Assert.Null(webRequest.error, webRequest.error);
#if UNITY_2020_1_OR_NEWER
            Assert.AreEqual(webRequest.result,UnityWebRequest.Result.Success);
#else
            Assert.IsFalse(webRequest.isNetworkError);
            Assert.IsFalse(webRequest.isHttpError);
#endif
            var json = webRequest.downloadHandler.text;

            Assert.NotNull(json);
            Assert.Greater(json.Length, 0);

            var go = new GameObject(GltfSampleModels.GetNameFromPath(path));
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
