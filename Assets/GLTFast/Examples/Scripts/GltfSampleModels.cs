using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class GltfSampleModels {

	public const string baseUrl = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/";
	
	public const string baseUrlLocal = "http://localhost:8080/glTF-Sample-Models/2.0/";

    public static string localPath {
		get {
			return string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
            "{0}"
#else
            "file://{0}"
#endif
            ,Path.Combine(Application.streamingAssetsPath, "glTF-Sample-Models/2.0")
        );
		}
	}
	
	public static string[] GetTestGltfFileUrls() {
        var path = Path.Combine(Application.streamingAssetsPath, "test-gltf-file-list.txt");
        return  LoadStreamingAssetFileBlocking(path);
    }

    public static string[] GetTestGlbFileUrls() {
        var path = Path.Combine(Application.streamingAssetsPath, "test-glb-file-list.txt");
        return LoadStreamingAssetFileBlocking(path);
    }

    static string[] LoadStreamingAssetFileBlocking(string path) {
        var uri = string.Format(
#if UNITY_ANDROID && !UNITY_EDITOR
                "{0}"
#else
                "file://{0}"
#endif
                ,path
            );
        var webRequest = UnityWebRequest.Get(uri);
        webRequest.SendWebRequest();
        while( !webRequest.isDone ) {} // blocking wait until done
        return webRequest.downloadHandler.text.Split('\n');
    }
}
