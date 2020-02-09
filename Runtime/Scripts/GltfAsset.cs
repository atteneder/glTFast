using UnityEngine;
using UnityEngine.Events;

namespace GLTFast
{
    public class GltfAsset : MonoBehaviour
    {
        public string url;

        protected GLTFast gLTFastInstance;

        public UnityAction<bool> onLoadComplete;

        protected virtual bool isGltfBinary {
            get { return false; }
        }

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
            gLTFastInstance = new GLTFast(this);
            gLTFastInstance.onLoadComplete += OnLoadComplete;
            gLTFastInstance.Load(this.url,isGltfBinary,deferAgent);
        }

        void OnLoadComplete(bool success) {
            gLTFastInstance.onLoadComplete -= OnLoadComplete;
            if(success) {
                gLTFastInstance.InstantiateGltf(transform);
            }
            onLoadComplete(success);
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