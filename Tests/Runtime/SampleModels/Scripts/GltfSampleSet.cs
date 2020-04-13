#if !(UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
#define LOCAL_LOADING
#endif

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine;
using GLTFast;

[CreateAssetMenu(fileName = "glTF-SampleSet", menuName = "ScriptableObjects/GltfSampleSet", order = 1)]
public class GltfSampleSet : ScriptableObject {
    public string fileListPath = "test-gltf-file-list.txt";
    public string baseLocalPath = "";
    public string streamingAssetsPath = "glTF-Sample-Models/2.0";
    public string baseUrlWeb = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/";
    public string baseUrlLocal = "http://localhost:8080/glTF-Sample-Models/2.0/";

    [System.NonSerialized]
    public List<Tuple<string,string>> items;

    [System.NonSerialized]
    public List<Tuple<string,string>> itemsLocal;
    
    public string localPath {
        get {
            string path;
            if(string.IsNullOrEmpty(streamingAssetsPath)) {
                path = baseLocalPath;
            } else {
                path = Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);
            }
            return path;
        }
    }

    public IEnumerator Load() {
        if(fileListPath==null) yield break;
        var path = Path.Combine(Application.streamingAssetsPath, fileListPath);
        yield return GltfSampleSet.LoadStreamingAssetFileBlocking(path, AddPaths );
    }

    void AddPaths(string[] paths) {
        string prefix = string.IsNullOrEmpty(baseUrlWeb) ? GltfSampleModels.baseUrl : baseUrlWeb;
#if UNITY_EDITOR
        if(!string.IsNullOrEmpty(baseUrlLocal)) {
            prefix = baseUrlLocal;
        }
#endif

        var tmpLocalPath = localPath;
        items = new List<Tuple<string, string>>();
        itemsLocal = new List<Tuple<string, string>>();
        foreach(var path in paths) {
            var name = GltfSampleModels.GetNameFromPath(path);
            if(!string.IsNullOrEmpty(prefix)) {
                var p = string.Format(
                    "{0}/{1}"
                    ,prefix
                    ,path
                );
                items.Add(new Tuple<string, string>(name,p));
            }
            var localPath = string.Format(
                "{0}/{1}"
                ,tmpLocalPath
                ,path
            );
            itemsLocal.Add(new Tuple<string, string>(name,localPath));
        }
    }

    public static IEnumerator LoadStreamingAssetFileBlocking( string path, UnityAction<string[]> callback ) {
        var uri = path;
#if LOCAL_LOADING
        uri = string.Format( "file://{0}", uri);
#endif

        Debug.LogFormat("Trying to load file list from {0}",uri);
        var webRequest = UnityWebRequest.Get(uri);
        yield return webRequest.SendWebRequest();
        var lines = webRequest.downloadHandler.text.Split('\n');
        var filteredLines = new List<string>();
        foreach (var line in lines)
        {
            if(!line.StartsWith("#") && !string.IsNullOrEmpty(line)) {
                filteredLines.Add(line.TrimEnd('\r'));
            }
        }
        callback( filteredLines.ToArray() );
    }
}
