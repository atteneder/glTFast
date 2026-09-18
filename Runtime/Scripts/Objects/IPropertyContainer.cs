// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Provides read-only access to additional properties on glTF extension or extras objects.
    /// Those are neither defined in the glTF specification (the currently supported version)
    /// nor any extension supported by glTFast.
    /// </summary>
    public interface IReadOnlyPropertyContainer
    {
        /// <summary>
        /// Gets the number of additional properties.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Determines whether the <see cref="IPropertyContainer"/> contains a property with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="IPropertyContainer"/></param>
        /// <returns>true if the <see cref="IReadOnlyPropertyContainer"/> contains a property with the key;
        /// otherwise, false.</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Gets an <see cref="IEnumerable{string}"/> containing the names of all properties.
        /// </summary>
        IEnumerable<string> Keys { get; }

        /// <summary>Gets the JSON <see cref="Value"/> for <paramref name="key"/>.</summary>
        /// <param name="key">The property name.</param>
        /// <value>The property's value.</value>
        Value this[string key] { get; }

        /// <summary>Returns an enumerator over the properties.</summary>
        /// <returns>An enumerator yielding each <see cref="Property"/>.</returns>
        PropertyEnumerator GetEnumerator();

        /// <summary>
        /// Tries to find a property of a <paramref name="key"/>
        /// and deserializes its <paramref name="value"/> to type <c>T</c>.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Resulting value</param>
        /// <typeparam name="T">Desired target type</typeparam>
        /// <returns>True if the property was found and successfully cast to type T. False otherwise.
        /// This includes target types that cannot be deserialized at all, like delegates or
        /// interfaces.</returns>
        bool TryGetValue<T>(string key, out T value);
    }

    /// <summary>
    /// Provides access to additional properties on glTF extension or extras objects.
    /// Those are neither defined in the glTF specification (the currently supported version)
    /// nor any extension supported by glTFast.
    /// </summary>
    public interface IPropertyContainer : IReadOnlyPropertyContainer
    {
        /// <summary>Removes the property <paramref name="key"/>.</summary>
        /// <param name="key">The property name.</param>
        /// <returns><c>true</c> if the property existed and was removed; otherwise <c>false</c>.</returns>
        bool Remove(string key);

        /// <summary>Removes all properties.</summary>
        void Clear();

        /// <summary>Sets the property <paramref name="key"/> to <paramref name="value"/>, serialized to JSON.</summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The value to store.</param>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        void Set<T>(string key, T value);

        /// <summary>Sets the property <paramref name="key"/> to a string <paramref name="value"/>.</summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The value to store.</param>
        void Set(string key, string value) => Set<string>(key, value);

        /// <summary>Sets the property <paramref name="key"/> to a numeric <paramref name="value"/>.</summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The value to store.</param>
        void Set(string key, double value) => Set<double>(key, value);

        /// <summary>Sets the property <paramref name="key"/> to an integer <paramref name="value"/>.</summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The value to store.</param>
        void Set(string key, long value) => Set<long>(key, value);

        /// <summary>Sets the property <paramref name="key"/> to a boolean <paramref name="value"/>.</summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The value to store.</param>
        void Set(string key, bool value) => Set<bool>(key, value);
    }
}
