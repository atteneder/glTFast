using UnityEngine;
using UnityEngine.Events;
#if !NO_GLTFAST
using GLTFast;
#endif

public class TestLoader : MonoBehaviour {

#if !NO_GLTFAST && UNITY_GLTF
    public const float variantDistance = 1;
#else
    public const float variantDistance = 0;
#endif

    public UnityAction<string> urlChanged;
    public UnityAction<float> time1Update;
    public UnityAction<float> time2Update;

    GameObject go1;
    GameObject go2;

#if !NO_GLTFAST
    GlbAsset gltf1;
#endif
#if UNITY_GLTF
    UnityGLTFLoader gltf2;
#endif

    float startTime = -1;

	// Use this for initialization
	void Start () {
		//LoadUrl( "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Duck/glTF-Binary/Duck.glb" );
	}
	
	public void LoadUrl(string url) {

#if !NO_GLTFAST
        if(gltf1!=null) {
            gltf1.onLoadComplete-= GLTFast_onLoadComplete;
            gltf1 = null;
        }
#endif
#if UNITY_GLTF
        if(gltf2!=null) {
            gltf2.onLoadComplete-= UnityGltf_OnLoadComplete;
            gltf2 = null;
        }
#endif

        if(go1!=null) {
            Destroy(go1);
        }
        if(go2!=null) {
            Destroy(go2);
        }

        Debug.Log("loading "+url);

        startTime = Time.realtimeSinceStartup;
#if !NO_GLTFAST
        time1Update(-1);
#endif
#if UNITY_GLTF
        time2Update(-1);
#endif

        go1 = new GameObject();
        go2 = new GameObject();

#if !NO_GLTFAST
        gltf1 = go1.AddComponent<GLTFast.GlbAsset>();
        gltf1.url = url;
        gltf1.onLoadComplete += GLTFast_onLoadComplete;
#endif
#if UNITY_GLTF
        go2.transform.rotation = Quaternion.Euler(0,180,0);
        gltf2 = go2.AddComponent<UnityGLTFLoader>();
        gltf2.GLTFUri = url;
        gltf2.onLoadComplete += UnityGltf_OnLoadComplete;
#endif

        urlChanged(url);
    }

#if UNITY_GLTF
    void UnityGltf_OnLoadComplete()
    {
        time2Update((Time.realtimeSinceStartup-startTime)*1000);
        var bounds = CalculateLocalBounds(go2.transform);
        
        float targetSize = 2.0f;
        
        float scale = Mathf.Min(
            targetSize / bounds.extents.x,
            targetSize / bounds.extents.y,
            targetSize / bounds.extents.z
            );

        go2.transform.localScale = Vector3.one * scale;
        Vector3 pos = bounds.center;
        pos.x -= bounds.extents.x * variantDistance;
        pos *= -scale;
        go2.transform.position = pos;
    }
#endif

#if !NO_GLTFAST
    void GLTFast_onLoadComplete(bool success)
    {
        time1Update((Time.realtimeSinceStartup-startTime)*1000);

        if(success) {
            var bounds = CalculateLocalBounds(gltf1.transform);
            
            float targetSize = 2.0f;
            
            float scale = Mathf.Min(
                targetSize / bounds.extents.x,
                targetSize / bounds.extents.y,
                targetSize / bounds.extents.z
                );
    
            gltf1.transform.localScale = Vector3.one * scale;
            Vector3 pos = bounds.center;
            pos.x += bounds.extents.x * variantDistance;;
            pos *= -scale;
            gltf1.transform.position = pos;
        } else {
            Debug.LogError("TestLoader: loading failed!");
        }
    }
#endif

    static Bounds CalculateLocalBounds(Transform transform)
    {
        Quaternion currentRotation = transform.rotation;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        Vector3 localCenter = bounds.center - transform.position;
        bounds.center = localCenter;
        transform.rotation = currentRotation;
        return bounds;
    }
}
