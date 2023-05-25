// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if GLTFAST_SAFE
using UnityEngine;

namespace GLTFast.FakeSchema {

    [System.Serializable]
    class Accessor {
        /// <summary>
        /// Sparse storage of attributes that deviate from their initialization value.
        /// </summary>
        public AccessorSparse sparse;
    }
}
#endif
