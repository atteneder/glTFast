// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// A single property (key/value pair) of a JSON object <see cref="Value"/>.
    /// </summary>
    public readonly ref struct Property
    {
        readonly string m_Key;
        readonly JsonElement m_Value;

        internal Property(string key, JsonElement value)
        {
            m_Key = key;
            m_Value = value;
        }

        internal Property(JsonProperty property) : this(property.Name, property.Value) { }

        /// <summary>The property name.</summary>
        public string Key => m_Key;

        /// <summary>The property value.</summary>
        public Value Value => new(m_Value);
    }
}
