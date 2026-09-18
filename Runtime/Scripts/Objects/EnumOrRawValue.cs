// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Wrapper for glTF string properties that the specification has a limited set of valid values for.
    /// If the value is unknown (may have been introduced by a glTF extension), <see cref="Value"/> will have the
    /// default value and <see cref="RawValue"/> will hold the original UTF-8 encoded string value.
    /// </summary>
    /// <typeparam name="TEnum">Enum type the string is deserialized to.</typeparam>
    [DebuggerDisplay("{ToString(),nq}")]
    public readonly struct EnumOrRawValue<TEnum> : IEquatable<TEnum>, IEquatable<EnumOrRawValue<TEnum>> where TEnum : struct, Enum
    {
        /// <summary>Enum value</summary>
        public TEnum Value { get; }

        /// <summary>UTF-8 encoded raw string value</summary>
        public byte[] RawValue { get; }

        /// <summary>
        /// Construct from enum value.
        /// </summary>
        /// <param name="value">Enum value</param>
        public EnumOrRawValue(TEnum value)
        {
            Value = value;
            RawValue = null;
        }

        /// <summary>
        /// Construct from UTF-8 string value.
        /// </summary>
        /// <param name="rawValue">UTF-8 string value</param>
        public EnumOrRawValue(byte[] rawValue)
        {
            Value = default;
            RawValue = rawValue;
        }

        /// <summary>
        /// Indicates whether the current instance is equal to another instance of the same type.
        /// </summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if both instances have equal <see cref="Value"/> and <see cref="RawValue"/>; false otherwise.</returns>
        public bool Equals(EnumOrRawValue<TEnum> other)
        {
            return System.Collections.Generic.EqualityComparer<TEnum>.Default.Equals(Value, other.Value)
                && (RawValue == other.RawValue
                    || (RawValue != null && other.RawValue != null
                        && RawValue.AsSpan().SequenceEqual(other.RawValue)));
        }

        /// <summary>
        /// Indicates whether the current instance is equal to the given object.
        /// </summary>
        /// <param name="obj">Object to compare with this instance.</param>
        /// <returns>True if <paramref name="obj"/> is an <see cref="EnumOrRawValue{TEnum}"/> equal to this instance; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is EnumOrRawValue<TEnum> other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>Hash code derived from <see cref="Value"/> and <see cref="RawValue"/>.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Value, RawValue);
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>The UTF-8 decoded <see cref="RawValue"/> when set; otherwise <see cref="Value"/>'s string representation.</returns>
        public override string ToString()
        {
            return RawValue == null ? Value.ToString() : System.Text.Encoding.UTF8.GetString(RawValue);
        }

        /// <summary>
        /// Indicates whether this instance represents the given enum value.
        /// </summary>
        /// <param name="other">Enum value to compare with this instance's <see cref="Value"/>.</param>
        /// <returns>True if <see cref="RawValue"/> is null and <see cref="Value"/> equals <paramref name="other"/>; false otherwise.</returns>
        public bool Equals(TEnum other)
        {
            return RawValue == null
                && System.Collections.Generic.EqualityComparer<TEnum>.Default.Equals(Value, other);
        }

        /// <summary>
        /// Equality operator comparing an <see cref="EnumOrRawValue{TEnum}"/> to an enum value.
        /// </summary>
        /// <param name="lhs">Left-hand side instance.</param>
        /// <param name="rhs">Right-hand side enum value.</param>
        /// <returns>True if <paramref name="lhs"/> represents <paramref name="rhs"/>; false otherwise.</returns>
        public static bool operator ==(EnumOrRawValue<TEnum> lhs, TEnum rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Inequality operator comparing an <see cref="EnumOrRawValue{TEnum}"/> to an enum value.
        /// </summary>
        /// <param name="lhs">Left-hand side instance.</param>
        /// <param name="rhs">Right-hand side enum value.</param>
        /// <returns>True if <paramref name="lhs"/> does not represent <paramref name="rhs"/>; false otherwise.</returns>
        public static bool operator !=(EnumOrRawValue<TEnum> lhs, TEnum rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Implicit conversion from an enum value, so call sites can assign a bare enum to a wrapped property.
        /// </summary>
        /// <param name="value">Enum value to wrap.</param>
        /// <returns>EnumOrRawValue&lt;TEnum&gt; wrapper around enum value.</returns>
        public static implicit operator EnumOrRawValue<TEnum>(TEnum value) => new EnumOrRawValue<TEnum>(value);
    }
}
