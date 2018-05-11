using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GLTFast
{
    public class GlbAsset : MonoBehaviour
    {
        public string url;

        public UnityAction onLoadComplete;

        // Use this for initialization
        void Start()
        {
            if(!string.IsNullOrEmpty(url)){
                Load();
            }
        }

        public void Load() {
            StartCoroutine(LoadRoutine());
        }

		IEnumerator LoadRoutine()
		{
    		UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
     
            if(www.isNetworkError || www.isHttpError) {
                Debug.LogError(www.error);
            }
            else {
                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;
                new GLTFast(results,transform);
                if(onLoadComplete!=null) {
                    onLoadComplete();
                }
            }
        }
    }
}