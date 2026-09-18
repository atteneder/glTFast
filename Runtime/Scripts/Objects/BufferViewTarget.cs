// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// GPU buffer type.
    /// Relates to WebGL's bindBuffer.
    /// </summary>
    /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#_bufferview_target"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public enum BufferViewTarget
    {
        /// <summary>Undefined</summary>
        Undefined = 0,
        /// <summary>ARRAY_BUFFER</summary>
        ArrayBuffer = 34962,
        /// <summary>ELEMENT_ARRAY_BUFFER</summary>
        ElementArrayBuffer = 34963,
    }
}
