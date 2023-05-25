// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast
{
    /// <summary>
    /// Defines how node names are created
    /// </summary>
    public enum NameImportMethod
    {
        /// <summary>
        /// Use original node names.
        /// Fallback to mesh's name (if present)
        /// Fallback to "Node_&lt;index&gt;" as last resort.
        /// </summary>
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
