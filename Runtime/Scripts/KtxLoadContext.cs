// Copyright 2020-2022 Andreas Atteneder
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

using System.Threading.Tasks;
using KtxUnity;
using Unity.Collections;

namespace GLTFast {
    class KtxLoadContext : KtxLoadContextBase {
        byte[] data;

        public KtxLoadContext(int index,byte[] data) {
            this.imageIndex = index;
            this.data = data;
            ktxTexture = new KtxTexture();
        }

        public override async Task<TextureResult> LoadKtx(bool linear) {
            var slice = new NativeArray<byte>(data,KtxNativeInstance.defaultAllocator);
            var result = await ktxTexture.LoadBytesRoutine(slice,linear);
            slice.Dispose();
            data = null;
            return result;
        }
    }
}
#endif // KTX_UNITY
