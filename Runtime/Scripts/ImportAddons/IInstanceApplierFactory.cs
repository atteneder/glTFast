// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Addons
{
    /// <summary>
    /// Creates <see cref="IInstanceApplier"/> instances bound to a specific
    /// <see cref="IInstantiator"/>.
    /// </summary>
    /// <remarks>
    /// One factory may be invoked multiple times during an import to produce an applier per scene
    /// instance. Implementations should return <see langword="null"/> when the supplied
    /// <see cref="IInstantiator"/> is not supported.
    /// </remarks>
    [MovedFrom(true, sourceNamespace: "GLTFast.Addons", sourceAssembly: "glTFast")]
    public interface IInstanceApplierFactory
    {
        /// <summary>
        /// Creates an <see cref="IInstanceApplier"/> bound to the given instantiator.
        /// </summary>
        /// <param name="instantiator">Instantiator that produced the scene instance the applier
        /// will operate on.</param>
        /// <returns>An applier bound to <paramref name="instantiator"/>, or <see langword="null"/>
        /// if the instantiator type is not supported.</returns>
        IInstanceApplier CreateInstanceApplier(IInstantiator instantiator);
    }
}
