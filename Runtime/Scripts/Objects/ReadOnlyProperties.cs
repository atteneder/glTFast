// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Allocation-free view into additional properties of a glTF JSON object.
    /// </summary>
    /// <remarks>Mirrors <see cref="IReadOnlyPropertyContainer"/>, but ref structs cannot implement interfaces
    /// in current C# versions older than 13</remarks>
    public readonly ref struct ReadOnlyProperties
    {
        internal static readonly Dictionary<string, JsonElement> Empty = new();

        readonly Dictionary<string, JsonElement> m_Data;

        internal ReadOnlyProperties(Dictionary<string, JsonElement> data) => m_Data = data;

        /// <inheritdoc cref="IReadOnlyPropertyContainer.Count"/>
        public int Count => m_Data.Count;

        /// <inheritdoc cref="IReadOnlyPropertyContainer.ContainsKey"/>
        public bool ContainsKey(string key) => m_Data.ContainsKey(key);

        /// <inheritdoc cref="IReadOnlyPropertyContainer.TryGetValue{T}"/>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (m_Data.TryGetValue(key, out var token))
            {
                try
                {
                    value = token.Deserialize<T>(JsonOptions.Options);
                    return true;
                }
                // JsonException: the value does not fit T. NotSupportedException: T is not
                // deserializable at all, which for interfaces and abstract types depends on the
                // value's kind.
                catch (Exception exception) when (exception is JsonException or NotSupportedException)
                {
                    value = default;
                    return false;
                }
            }
            value = default; return false;
        }

        /// <inheritdoc cref="IReadOnlyPropertyContainer.Keys"/>
        [JsonIgnore]
        public IEnumerable<string> Keys => m_Data.Keys;

        /// <summary>Gets the JSON <see cref="Value"/> for <paramref name="key"/>.</summary>
        /// <param name="key">The property name.</param>
        /// <value>The property's value.</value>
        public Value this[string key] => new(m_Data[key]);

        /// <summary>Returns an enumerator over the properties.</summary>
        /// <returns>An enumerator yielding each <see cref="Property"/>.</returns>
        public PropertyEnumerator GetEnumerator() => new(m_Data);
    }

    /// <summary>Enumerates the properties of <see cref="ReadOnlyProperties"/> as <see cref="Property"/> items.</summary>
    public struct PropertyEnumerator
    {
        Dictionary<string, JsonElement>.Enumerator m_Enumerator;

        internal PropertyEnumerator(Dictionary<string, JsonElement> data)
        {
            m_Enumerator = data.GetEnumerator();
        }

        /// <summary>Advances the enumerator to the next property.</summary>
        /// <returns><c>true</c> if there is another property; otherwise <c>false</c>.</returns>
        public bool MoveNext() => m_Enumerator.MoveNext();

        /// <summary>The property at the current position of the enumerator.</summary>
        public Property Current => new(m_Enumerator.Current.Key, m_Enumerator.Current.Value);
    }
}
