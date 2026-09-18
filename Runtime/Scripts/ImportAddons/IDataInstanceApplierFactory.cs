// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Addons
{
    /// <summary>
    /// Factory that owns conversion-phase data (<see cref="IDataCache"/>) and produces
    /// <see cref="IInstanceApplier"/> instances which apply that data to instantiated scenes.
    /// </summary>
    /// <remarks>
    /// The factory's lifetime spans the import: it is created at the end of the conversion phase
    /// and disposed when the cached data is no longer needed. It may be used to apply the same
    /// data to multiple scene instances.
    /// </remarks>
    [MovedFrom(true, sourceNamespace: "GLTFast.Addons", sourceAssembly: "glTFast")]
    public interface IDataInstanceApplierFactory : IDataCache, IInstanceApplierFactory { }
}
