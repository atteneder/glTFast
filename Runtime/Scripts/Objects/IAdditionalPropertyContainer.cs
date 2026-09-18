// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Provides access to additional properties on glTF JSON objects.
    /// </summary>
    interface IAdditionalPropertyContainer
    {
        /// <summary>
        /// Additional properties on glTF JSON objects.
        /// Those properties may have been added by a new, unsupported version of the glTF specification.
        /// For extending glTF, please use extensions or extras instead.
        /// </summary>
        ReadOnlyProperties AdditionalProperties { get; }
    }
}
