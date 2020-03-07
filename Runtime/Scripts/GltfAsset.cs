using UnityEngine;
using UnityEngine.Events;

namespace GLTFast
{
    public class GltfAsset : MonoBehaviour
    {
        public string url;
        public bool loadOnStartup = true;

        protected GLTFast gLTFastInstance;

        public UnityAction<GltfAsset,bool> onLoadComplete;


        /// <summary>
        /// Method for manual loading with custom <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="url">URL of the glTF file.</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        public void Load( string url, IDeferAgent deferAgent=null ) {
            this.url = url;
            Load(deferAgent);
        }

        void Start()
        {
            if(loadOnStartup && !string.IsNullOrEmpty(url)){
                // Automatic load on startup
                Load();
            }
        }

        void Load( IDeferAgent deferAgent=null ) {
            gLTFastInstance = new GLTFast(this);
            gLTFastInstance.onLoadComplete += OnLoadComplete;
            gLTFastInstance.Load(url,deferAgent);
        }

        protected virtual void OnLoadComplete(bool success) {
            gLTFastInstance.onLoadComplete -= OnLoadComplete;
            if(success) {
                gLTFastInstance.InstantiateGltf(transform);
            }
            if(onLoadComplete!=null) {
                onLoadComplete(this,success);
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
