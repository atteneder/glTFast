#if KTX_UNITY

using System.Collections;
using KtxUnity;
using Unity.Collections;

namespace GLTFast {
    class KtxLoadContext : KtxLoadContextBase {
        byte[] data;

        public KtxLoadContext(int index,byte[] data) {
            this.imageIndex = index;
            this.data = data;
            ktxTexture = new KtxTexture();
            texture = null;
        }

        public override IEnumerator LoadKtx() {
            ktxTexture.onTextureLoaded += OnKtxLoaded;
            var slice = new NativeArray<byte>(data,KtxNativeInstance.defaultAllocator);
            yield return ktxTexture.LoadBytesRoutine(slice);
            slice.Dispose();
            data = null;
        }
    }
}
#endif // KTX_UNITY
