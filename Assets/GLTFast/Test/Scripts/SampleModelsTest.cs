#if !NO_TEST
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Networking;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GLTFast;

public class SampleModelsTest
{
    const string prefix = "glTF-Sample-Models/2.0/";

	[Test]
	public void SampleModelsTestCheckFiles()
	{
        var glbFiles = GltfSampleModels.GetTestGlbFileUrls();

#if !(UNITY_ANDROID && !UNITY_EDITOR)
		foreach (var file in glbFiles)
		{
			var path = Path.Combine(Application.streamingAssetsPath, prefix, file);
			Assert.IsTrue(
				File.Exists(path)
				, "glb file {0} not found"
				, path
			);
		}
#else
		// See https://docs.unity3d.com/Manual/StreamingAssets.html
		Debug.Log("File access doesn't work on Android");
#endif
	}
 
	[UnityTest]
	public IEnumerator SampleModelsTestLoadAllGlb()
	{      
        var glbFiles = GltfSampleModels.GetTestGlbFileUrls();

		foreach (var file in glbFiles)
		{
			var path = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
				"{0}"
#else
				"file://{0}"
#endif
			    ,Path.Combine(Application.streamingAssetsPath, prefix, file)
			);
                             
			Debug.LogFormat("Testing {0}", path);

			var webRequest = UnityWebRequest.Get(path);
			yield return webRequest.SendWebRequest();
			Assert.Null(webRequest.error);
			var bytes = webRequest.downloadHandler.data;

			Assert.NotNull(bytes);
			Assert.Greater(bytes.Length, 0);

			var go = new GameObject();         
			var glTFast = new GLTFast.GLTFast();
            var success = glTFast.LoadGlb(bytes, go.transform);
            Assert.True(success);
			yield return null;
			Object.Destroy(go);
        }
	}

    [UnityTest]
    public IEnumerator SampleModelsTestLoadAllGltf()
    {
        var gltfFiles = GltfSampleModels.GetTestGltfFileUrls();

        foreach (var file in gltfFiles)
        {
            var path = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
                "{0}"
#else
                "file://{0}"
#endif
                ,Path.Combine(Application.streamingAssetsPath, prefix, file)
            );
                             
            Debug.LogFormat("Testing {0}", path);

            var webRequest = UnityWebRequest.Get(path);
            yield return webRequest.SendWebRequest();
            Assert.Null(webRequest.error,webRequest.error);
			Assert.IsFalse(webRequest.isNetworkError);
			Assert.IsFalse(webRequest.isHttpError);
            var json = webRequest.downloadHandler.text;

            Assert.NotNull(json);
            Assert.Greater(json.Length, 0);

            var go = new GameObject();    
            var glTFast = new GLTFast.GLTFast();
            Assert.IsTrue(glTFast.LoadGltf(json, path));
            Debug.LogError("Test not finished. Need to load all things (not just parse)");
            yield return null;
            Object.Destroy(go);
        }
    }
}
#endif
