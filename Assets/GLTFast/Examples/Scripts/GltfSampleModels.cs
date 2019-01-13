#if !(UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
#define LOCAL_LOADING
#endif

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class GltfSampleModels {

	public const string baseUrl = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/";
	
	public const string baseUrlLocal = "http://localhost:8080/glTF-Sample-Models/2.0/";

    public static string localPath {
		get {
			var path = Path.Combine(Application.streamingAssetsPath, "glTF-Sample-Models/2.0");
#if LOCAL_LOADING
			path = string.Format( "file://{0}", path );
#endif
			return path;
		}
	}
	
	public static string[] gltfFileUrls;
	public static string[] glbFileUrls;

	public static IEnumerator LoadGltfFileUrls() {
        var path = Path.Combine(Application.streamingAssetsPath, "test-gltf-file-list.txt");
        yield return LoadStreamingAssetFileBlocking(path, (arr) => gltfFileUrls = arr );
    }

    public static IEnumerator LoadGlbFileUrls() {
		if(glbFileUrls!=null) yield break;
        var path = Path.Combine(Application.streamingAssetsPath, "test-glb-file-list.txt");
        yield return LoadStreamingAssetFileBlocking(path, (arr) => glbFileUrls = arr );
    }

    static IEnumerator LoadStreamingAssetFileBlocking( string path, UnityAction<string[]> callback ) {
        var uri = path;
		
#if LOCAL_LOADING
		uri = string.Format( "file://{0}", uri);
#endif

		Debug.LogFormat("Trying to load file list from {0}",uri);
        var webRequest = UnityWebRequest.Get(uri);
        yield return webRequest.SendWebRequest();
        callback( webRequest.downloadHandler.text.Split('\n') );
    }
}
