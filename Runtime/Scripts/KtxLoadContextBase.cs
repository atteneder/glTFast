// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if KTX_UNITY_2_2_OR_NEWER || (!UNITY_2021_2_OR_NEWER && KTX_UNITY_1_3_OR_NEWER)
#define KTX
#endif

#if KTX

using System.Threading.Tasks;
using KtxUnity;
using UnityEngine;

namespace GLTFast {
    abstract class KtxLoadContextBase {
        public int imageIndex;
        protected KtxTexture m_KtxTexture;

        public abstract Task<TextureResult> LoadTexture2D(bool linear);
    }
}
#endif // KTX_UNITY
