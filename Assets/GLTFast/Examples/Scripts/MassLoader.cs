using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassLoader : MonoBehaviour {

    public bool local = false;

	// Use this for initialization
	IEnumerator Start () {

        yield return GltfSampleModels.LoadGltfFileUrls();

        var baseUrl = local ? GltfSampleModels.localPath : GltfSampleModels.baseUrl;

        // Wait a bit to make sure profiling works
        yield return new WaitForSeconds(1);

		foreach( var n in GltfSampleModels.gltfFileUrls ) {
            var url = string.Format(
                "{0}/{1}"
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
