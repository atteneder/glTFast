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

		GLTFast gLTFastInstance;
		Coroutine loadRoutine;

        public UnityAction<bool> onLoadComplete;

        // Use this for initialization
        void Start()
        {
            if(!string.IsNullOrEmpty(url)){
                Load();
            }
        }

        public void Load( string url = null ) {
			if(url!=null) {
				this.url = url;
			}
			if(gLTFastInstance==null && loadRoutine==null) {
				loadRoutine = StartCoroutine(LoadRoutine());
            }
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
				gLTFastInstance = new GLTFast();
                var success = gLTFastInstance.LoadGlb(results,transform);
				if(onLoadComplete!=null) {
					onLoadComplete(success);
				}
            }
			loadRoutine = null;
        }

		public IEnumerator WaitForLoaded() {
			while(loadRoutine!=null) {
				yield return loadRoutine;         
            }
		}

		private void OnDestroy()
		{
			gLTFastInstance.Destroy();
		}
	}
}