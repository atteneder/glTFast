#if KTX_UNITY

using System.Collections;
using KtxUnity;
using Unity.Collections;

namespace GLTFast {
    class KtxLoadNativeContext : KtxLoadContextBase {
        NativeSlice<byte> slice;

        public KtxLoadNativeContext(int index,NativeSlice<byte> slice) {
            this.imageIndex = index;
            this.slice = slice;
            ktxTexture = new KtxTexture();
            texture = null;
        }

        public override IEnumerator LoadKtx() {
            ktxTexture.onTextureLoaded += OnKtxLoaded;
            return ktxTexture.LoadBytesRoutine(slice);
        }
    }
}
#endif // KTX_UNITY
