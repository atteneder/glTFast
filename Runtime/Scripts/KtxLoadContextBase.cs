#if KTX_UNITY

using System.Collections;
using KtxUnity;
using UnityEngine;

namespace GLTFast {
    abstract class KtxLoadContextBase {
        public int imageIndex;
        public Texture2D texture;
        protected KtxTexture ktxTexture;
        
        public abstract IEnumerator LoadKtx();

        protected void OnKtxLoaded(Texture2D newTexture) {
            ktxTexture.onTextureLoaded -= OnKtxLoaded;
            texture = newTexture;
        }
    }
}
#endif // KTX_UNITY
