// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Read-only view over a single JSON value, used to traverse arbitrary JSON data.
    /// </summary>
    /// <remarks>
    /// Except for <see cref="Kind"/> and <see cref="TryGetValue{T}(out T)"/>, every member requires the
    /// value to be of a particular <see cref="ValueKind"/> and throws an
    /// <see cref="InvalidOperationException"/> otherwise, including for
    /// <see cref="ValueKind.Undefined"/>. A <see cref="Value"/> is <see cref="ValueKind.Undefined"/>
    /// when it stands for "no value at all": <c>default</c>, the result of a failed
    /// <see cref="TryGetValue(string,out Value)"/>, or <see cref="ExtrasContainer.RawValue"/> of an
    /// <c>extras</c> that is a JSON object. Check <see cref="Kind"/> before reading.
    /// </remarks>
    public readonly ref struct Value
    {
        readonly JsonElement m_Element;

        /// <summary>
        /// Initializes a new <see cref="Value"/> wrapping the given JSON element.
        /// </summary>
        /// <param name="element">The JSON element to wrap.</param>
        internal Value(JsonElement element)
        {
            m_Element = element;
        }

        /// <summary>The <see cref="ValueKind"/> of this value.</summary>
        public ValueKind Kind => (ValueKind)m_Element.ValueKind;

        /// <summary>Tries to get this value as a double-precision floating point number.</summary>
        /// <param name="value">The resulting number, if successful.</param>
        /// <returns><c>true</c> if the number is representable as a <see cref="double"/>; otherwise
        /// <c>false</c>. The <c>Try</c> covers the number's range only, not this value's kind.</returns>
        /// <exception cref="InvalidOperationException">This value is not of kind
        /// <see cref="ValueKind.Number"/>.</exception>
        public bool TryGetDouble(out double value) => m_Element.TryGetDouble(out value);

        /// <summary>Tries to get this value as a 64-bit signed integer.</summary>
        /// <param name="value">The resulting integer, if successful.</param>
        /// <returns><c>true</c> if the number is representable as a <see cref="long"/>; otherwise
        /// <c>false</c>. The <c>Try</c> covers the number's range only, not this value's kind.</returns>
        /// <exception cref="InvalidOperationException">This value is not of kind
        /// <see cref="ValueKind.Number"/>.</exception>
        public bool TryGetInt64(out long value) => m_Element.TryGetInt64(out value);

        /// <summary>Gets this value as a string.</summary>
        /// <returns>The string value, or <c>null</c> if the value is a JSON <c>null</c>.</returns>
        /// <exception cref="InvalidOperationException">This value is of a kind other than
        /// <see cref="ValueKind.String"/> or <see cref="ValueKind.Null"/>.</exception>
        public string GetString() => m_Element.GetString();

        /// <summary>Gets this value as a boolean.</summary>
        /// <returns>The boolean value.</returns>
        /// <exception cref="InvalidOperationException">This value is of a kind other than
        /// <see cref="ValueKind.True"/> or <see cref="ValueKind.False"/>.</exception>
        public bool GetBoolean() => m_Element.GetBoolean();

        /// <summary>Gets the value of the object property named <paramref name="key"/>.</summary>
        /// <param name="key">The property name.</param>
        /// <value>The property's value.</value>
        /// <exception cref="InvalidOperationException">This value is not of kind
        /// <see cref="ValueKind.Object"/>.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">No property named
        /// <paramref name="key"/> exists. Use <see cref="TryGetValue(string,out Value)"/> to test for
        /// it.</exception>
        public Value this[string key] => new(m_Element.GetProperty(key));

        /// <summary>Gets the array element at <paramref name="index"/>.</summary>
        /// <param name="index">The zero-based element index.</param>
        /// <value>The element's value.</value>
        /// <exception cref="InvalidOperationException">This value is not of kind
        /// <see cref="ValueKind.Array"/>.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        public Value this[int index] => new(m_Element[index]);

        /// <summary>The number of elements, when this value is a <see cref="ValueKind.Array"/>.</summary>
        /// <exception cref="InvalidOperationException">This value is not of kind
        /// <see cref="ValueKind.Array"/>.</exception>
        public int ArrayLength => m_Element.GetArrayLength();

        /// <summary>Tries to get the value of the object property named <paramref name="key"/>.</summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The resulting value, if the property exists. Of kind
        /// <see cref="ValueKind.Undefined"/> otherwise.</param>
        /// <returns><c>true</c> if the property exists; otherwise <c>false</c>. The <c>Try</c> covers
        /// the property's absence only, not this value's kind.</returns>
        /// <exception cref="InvalidOperationException">This value is not of kind
        /// <see cref="ValueKind.Object"/>.</exception>
        public bool TryGetValue(string key, out Value value)
        {
            if (m_Element.TryGetProperty(key, out var element))
            {
                value = new Value(element);
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>Returns an enumerator over the properties of this object value.</summary>
        /// <returns>An enumerator yielding each <see cref="Property"/>.</returns>
        /// <exception cref="InvalidOperationException">This value is not of kind
        /// <see cref="ValueKind.Object"/>.</exception>
        public ObjectEnumerator EnumerateObject() => new(m_Element.EnumerateObject());

        /// <summary>Tries to deserialize this value to type <typeparamref name="T"/>.</summary>
        /// <param name="value">The resulting value, if successful.</param>
        /// <typeparam name="T">Desired target type.</typeparam>
        /// <returns><c>true</c> if the value was successfully deserialized; otherwise <c>false</c>.
        /// This includes target types that cannot be deserialized at all, like delegates or
        /// interfaces, and a value of kind <see cref="ValueKind.Undefined"/>, which holds nothing to
        /// convert.</returns>
        public bool TryGetValue<T>(out T value)
        {
            // Unlike every other member, this one is not tied to a single kind, so it has to report
            // "no value at all" as a failed conversion rather than throw. Deserialize would raise an
            // InvalidOperationException here, which is indistinguishable from a converter defect.
            if (m_Element.ValueKind == JsonValueKind.Undefined)
            {
                value = default;
                return false;
            }

            try
            {
                value = m_Element.Deserialize<T>(JsonOptions.Options);
                return true;
            }
            // JsonException: the value does not fit T. NotSupportedException: T is not deserializable
            // at all, which for interfaces and abstract types depends on the value's kind.
            catch (Exception exception) when (exception is JsonException or NotSupportedException)
            {
                value = default;
                return false;
            }
        }

        /// <summary>Enumerates the properties of an object <see cref="Value"/> as <see cref="Property"/> items.</summary>
        public ref struct ObjectEnumerator
        {
            JsonElement.ObjectEnumerator m_Enumerator;

            internal ObjectEnumerator(JsonElement.ObjectEnumerator enumerator)
            {
                m_Enumerator = enumerator;
            }

            /// <summary>Returns this enumerator, enabling use in a <c>foreach</c> loop.</summary>
            /// <returns>This enumerator.</returns>
            public ObjectEnumerator GetEnumerator() => this;

            /// <summary>Advances the enumerator to the next property.</summary>
            /// <returns><c>true</c> if there is another property; otherwise <c>false</c>.</returns>
            public bool MoveNext() => m_Enumerator.MoveNext();

            /// <summary>The property at the current position of the enumerator.</summary>
            public Property Current => new(m_Enumerator.Current);
        }
    }
}
