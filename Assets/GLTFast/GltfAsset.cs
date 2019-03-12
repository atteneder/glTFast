using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GLTFast
{
    public class GltfAsset : MonoBehaviour
    {
        public string url;

        protected GLTFast gLTFastInstance;
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
                Debug.LogErrorFormat("{0} {1}",www.error,url);
            }
            else {
                yield return StartCoroutine( LoadContent(www.downloadHandler) );
            }
            loadRoutine = null;
        }

        protected virtual IEnumerator LoadContent( DownloadHandler dlh ) {
            string json = dlh.text;
            gLTFastInstance = new GLTFast();

            if(gLTFastInstance.LoadGltf(json,url)) {

                yield return StartCoroutine( gLTFastInstance.WaitForAllDependencies() );
                yield return StartCoroutine( gLTFastInstance.Prepare() );
                
                var success = gLTFastInstance.InstanciateGltf(transform);

                if(onLoadComplete!=null) {
                   onLoadComplete(success);
                }
            }
        }

        public IEnumerator WaitForLoaded() {
            while(loadRoutine!=null) {
                yield return loadRoutine;         
            }
        }

        private void OnDestroy()
        {
            if(gLTFastInstance!=null) {
                gLTFastInstance.Destroy();
                gLTFastInstance=null;
            }
        }
    }
}