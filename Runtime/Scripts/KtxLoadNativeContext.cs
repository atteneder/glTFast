﻿// Copyright 2020-2022 Andreas Atteneder
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
    class KtxLoadNativeContext : KtxLoadContextBase {
        NativeSlice<byte> slice;

        public KtxLoadNativeContext(int index,NativeSlice<byte> slice) {
            this.imageIndex = index;
            this.slice = slice;
            ktxTexture = new KtxTexture();
        }

        public override async Task<TextureResult> LoadKtx(bool linear) {
            return await ktxTexture.LoadBytesRoutine(slice,linear:linear);
        }
    }
}
#endif // KTX_UNITY
