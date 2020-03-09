#if !(UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
#define LOCAL_LOADING
#endif

using System.Collections;
using System.IO;
using UnityEngine;

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

	public static string GetNameFromPath( string path ) {
		var i = path.LastIndexOf('/');
		return i>=0 ? path.Substring(i+1) : path;
	}

	public static IEnumerator LoadGltfFileUrls() {

		if(gltfFileUrls!=null) yield break;
		var set = ScriptableObject.CreateInstance<GltfSampleSet>();
		set.fileListPath = "glTF_gltf.txt";
		set.streamingAssetsPath = "glTF-Sample-Models/2.0";
		set.baseUrlWeb = baseUrl;
		set.baseUrlLocal = baseUrlLocal;
		yield return set.Load();

		gltfFileUrls = new string[set.itemsLocal.Count];
		for (int i = 0; i < set.itemsLocal.Count; i++)
		{
			gltfFileUrls[i] = set.itemsLocal[i].Item2;
		}
	}

    public static IEnumerator LoadGlbFileUrls() {
		if(glbFileUrls!=null) yield break;
		var set = ScriptableObject.CreateInstance<GltfSampleSet>();
		set.fileListPath = "glTF_glb.txt";
		set.streamingAssetsPath = "glTF-Sample-Models/2.0";
		set.baseUrlWeb = baseUrl;
		set.baseUrlLocal = baseUrlLocal;
		yield return set.Load();

		glbFileUrls = new string[set.itemsLocal.Count];
		for (int i = 0; i < set.itemsLocal.Count; i++)
		{
			glbFileUrls[i] = set.itemsLocal[i].Item2;
		}
	}
}
