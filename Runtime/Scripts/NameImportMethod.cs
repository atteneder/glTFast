// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Defines how node names are created
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public enum NameImportMethod
    {
        /// <summary>
        /// Use original node names, passing null to <see cref="IInstantiator.CreateNode"/> for a node without one.
        /// </summary>
        /// <remarks>Both shipped instantiators then fall back to the first valid mesh name, else "Node-&lt;index&gt;",
        /// but a custom <see cref="IInstantiator"/> owns that choice.</remarks>
        Original,
        /// <summary>
        /// Identical to <see cref="Original">Original</see>, but
        /// names are made unique (within their hierarchical position)
        /// by supplementing a continuous number.
        /// This is required for correct animation target lookup and import continuity.
        /// </summary>
        OriginalUnique
    }
}
