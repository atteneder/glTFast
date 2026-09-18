// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>Shared <see cref="JsonSerializerOptions"/> for (de)serializing
    /// additional-property values.</summary>
    static class JsonOptions
    {
        /// <summary>Serializer options that include public fields, used to
        /// round-trip user types stored as additional properties.</summary>
        public static readonly JsonSerializerOptions Options = new() { IncludeFields = true };
    }
}
