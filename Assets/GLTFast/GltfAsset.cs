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
        protected IDeferAgent deferAgent;

        public UnityAction<bool> onLoadComplete;

        // Use this for initialization
        void Start()
        {
            if(!string.IsNullOrEmpty(url)){
                Load();
            }
        }

        public void Load( string url = null, IDeferAgent deferAgent=null ) {
            if(url!=null) {
                this.url = url;
            }
            if(gLTFastInstance==null && loadRoutine==null) {
                this.deferAgent = deferAgent ?? new DeferTimer();
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
            deferAgent.Reset();
            gLTFastInstance = new GLTFast();

            bool allFine = true;

            LoadContentPrimary(dlh);
            
            allFine = !gLTFastInstance.LoadingError;

            if(allFine) {
                if( deferAgent.ShouldDefer() ) {
                    yield return null;
                }
                var routineBuffers = StartCoroutine( gLTFastInstance.WaitForBufferDownloads() );
                var routineTextures = StartCoroutine( gLTFastInstance.WaitForTextureDownloads() );

                yield return routineBuffers;
                yield return routineTextures;
            }

            allFine = !gLTFastInstance.LoadingError;

            if(allFine) {
                deferAgent.Reset();
                var prepareRoutine = gLTFastInstance.Prepare();
                while(prepareRoutine.MoveNext()) {
                    allFine = !gLTFastInstance.LoadingError;
                    if(!allFine) {
                        break;
                    }
                    if( deferAgent.ShouldDefer() ) {
                        yield return null;
                    }
                }
            }
            
            allFine = !gLTFastInstance.LoadingError;
            if(allFine) {
                if( deferAgent.ShouldDefer() ) {
                    yield return null;
                }
                allFine = gLTFastInstance.InstanciateGltf(transform);
            }

            if(onLoadComplete!=null) {
                onLoadComplete(allFine);
            }
        }

        protected virtual void LoadContentPrimary(DownloadHandler dlh) {
            string json = dlh.text;
            gLTFastInstance.LoadGltf(json,url);
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