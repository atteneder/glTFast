using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassLoader : MonoBehaviour {

    public bool local = false;

	// Use this for initialization
	IEnumerator Start () {

        var names = GltfSampleModels.GetTestGltfFileUrls();
        var baseUrl = local ? GltfSampleModels.baseUrlLocal : GltfSampleModels.baseUrl;

        // Wait a bit to make sure profiling works
        yield return new WaitForSeconds(1);

		foreach( var n in names ) {
            var url = string.Format(
                "{0}{1}"
                ,baseUrl
                ,n
                );
            var go = new GameObject(System.IO.Path.GetFileNameWithoutExtension(url));

#if UNITY_GLTF
            var gltf = go.AddComponent<UnityGLTF.GLTFComponent>();
            gltf.GLTFUri = url;
#endif
            
#if !NO_GLTFAST
            // GLTFast.GLTFast.LoadGlbFile( url, go.transform );
            var gltfAsset = go.AddComponent<GLTFast.GltfAsset>();
            gltfAsset.url = url;
#endif
        }
	}
}
