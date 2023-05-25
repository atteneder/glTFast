// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast.FakeSchema
{

    /// <summary>
    /// The material appearance of a primitive.
    /// </summary>
    [System.Serializable]
    class Material : NamedObject
    {
        public MaterialExtension extensions;
    }
}
