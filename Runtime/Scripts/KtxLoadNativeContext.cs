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

#if KTX_UNITY_2_2_OR_NEWER || (!UNITY_2021_2_OR_NEWER && KTX_UNITY_1_3_OR_NEWER)
#define KTX
#endif

#if KTX

using System.Threading.Tasks;
using KtxUnity;
using Unity.Collections;

namespace GLTFast {
    class KtxLoadNativeContext : KtxLoadContextBase {
        NativeSlice<byte> slice;

        public KtxLoadNativeContext(int index,NativeSlice<byte> slice) {
            layer = index;
            this.slice = slice;
            ktxTexture = new KtxTexture();
        }

        public override async Task<ErrorCode> LoadAndTranscode(bool linear) {
            var result = ktxTexture.Load(slice);
            if (result != ErrorCode.Success) {
                return result;
            }

            result = await ktxTexture.Transcode(linear);
            return result;
        }

        public override async Task<TextureResult> CreateTextureAndDispose() {
            var result = await ktxTexture.CreateTexture();
            ktxTexture.Dispose();
            return result;
        }
    }
}
#endif // KTX_UNITY
