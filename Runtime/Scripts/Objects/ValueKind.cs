// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// The kind of JSON value a <see cref="Value"/> represents.
    /// </summary>
    public enum ValueKind : byte
    {
        /// <summary>No value (as distinct from <see cref="Null"/>).</summary>
        Undefined,

        /// <summary>A JSON object (a set of key/value properties).</summary>
        Object,

        /// <summary>A JSON array.</summary>
        Array,

        /// <summary>A JSON string.</summary>
        String,

        /// <summary>A JSON number.</summary>
        Number,

        /// <summary>The JSON literal <c>true</c>.</summary>
        True,

        /// <summary>The JSON literal <c>false</c>.</summary>
        False,

        /// <summary>The JSON literal <c>null</c>.</summary>
        Null,
    }
}
