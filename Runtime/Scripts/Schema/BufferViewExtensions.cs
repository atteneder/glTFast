// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast.Schema
{
    /// <summary>
    /// BufferView extensions
    /// </summary>
    /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#reference-bufferview"/>
    [Serializable]
    public class BufferViewExtensions
    {
#if MESHOPT
        // ReSharper disable InconsistentNaming
        public BufferViewMeshoptExtension EXT_meshopt_compression;
        // ReSharper restore InconsistentNaming
#endif
    }
}
