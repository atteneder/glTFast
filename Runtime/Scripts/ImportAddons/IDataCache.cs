// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Addons
{
    /// <summary>
    /// Marker interface for objects that hold data produced during the conversion phase of a glTF
    /// import and that must be explicitly disposed when no longer needed.
    /// </summary>
    /// <remarks>
    /// Implementations typically own native or unmanaged resources (e.g.
    /// <see cref="Unity.Collections.NativeArray{T}"/>) and release them in
    /// <see cref="IDisposable.Dispose"/>.
    /// </remarks>
    [MovedFrom(true, sourceNamespace: "GLTFast.Addons", sourceAssembly: "glTFast")]
    public interface IDataCache : IDisposable { }
}
