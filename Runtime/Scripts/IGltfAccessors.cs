// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Collections;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Provides read-only access to typed glTF accessor data.
    /// </summary>
    /// <remarks>
    /// The data is decoded into Unity's coordinate system and value range. For a glTF asset's data
    /// as stored, use <see cref="IGltfBufferData"/> instead.
    /// </remarks>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public interface IGltfAccessors
    {
        /// <summary>
        /// Provides an accessors typed data.
        /// </summary>
        /// <param name="accessorIndex">glTF accessor index.</param>
        /// <typeparam name="T">Accessor member type.</typeparam>
        /// <returns>The requested data or a non-initialized readonly native array
        /// if the request couldn't be handled.</returns>
        NativeArray<T>.ReadOnly GetAccessorData<T>(int accessorIndex)
            where T : unmanaged;
    }
}
