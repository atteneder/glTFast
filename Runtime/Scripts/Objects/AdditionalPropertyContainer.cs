// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// glTF object base class
    /// </summary>
    public class AdditionalPropertyContainer : IPropertyContainer
    {
        /// <summary>
        /// JSON properties without a matching member.
        /// </summary>
        [JsonExtensionData, JsonInclude]
        internal Dictionary<string, JsonElement> ExtensionData { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public int Count => ExtensionData?.Count ?? 0;

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return ExtensionData?.ContainsKey(key) ?? false;
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public IEnumerable<string> Keys => ExtensionData?.Keys ?? (IEnumerable<string>)Array.Empty<string>();

        /// <inheritdoc/>
        public Value this[string key] => ExtensionData != null
            ? new Value(ExtensionData[key])
            : throw new KeyNotFoundException();

        /// <inheritdoc/>
        public bool Remove(string key) => ExtensionData?.Remove(key) ?? false;

        /// <inheritdoc/>
        public virtual void Clear() => ExtensionData?.Clear();

        /// <summary>Sets the property <paramref name="key"/> to <paramref name="value"/>, serialized to JSON.</summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The value to store.</param>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        public virtual void Set<T>(string key, T value)
        {
            ExtensionData ??= new Dictionary<string, JsonElement>();
            ExtensionData[key] = JsonSerializer.SerializeToElement(value, JsonOptions.Options);
        }

        /// <inheritdoc/>
        public PropertyEnumerator GetEnumerator() => new(ExtensionData ?? ReadOnlyProperties.Empty);

        /// <inheritdoc/>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (ExtensionData != null && ExtensionData.TryGetValue(key, out var token))
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
    }
}
