// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Loading
{
    /// <summary>
    /// Provides a mechanism to inspect the progress and result of a download
    /// or file access request
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Loading", sourceAssembly: "glTFast")]
    public interface IDownload : IDisposable
    {
        /// <summary>
        /// True, if the request was successful
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// Error message in case the request failed. Null otherwise.
        /// </summary>
        string Error { get; }

        /// <summary>
        /// Resulting data as NativeArray (does not allocate memory).
        /// </summary>
        NativeArray<byte>.ReadOnly Data { get; }
    }
}
