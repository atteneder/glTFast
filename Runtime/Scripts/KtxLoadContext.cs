// Copyright 2020 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

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
