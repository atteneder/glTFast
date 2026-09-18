// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using Unity.Mathematics;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Linear RGB color without alpha channel.
    /// </summary>
    public struct Color : IEquatable<Color>
    {
        /// <summary>Red component.</summary>
        public float R { get; set; }

        /// <summary>Green component.</summary>
        public float G { get; set; }

        /// <summary>Blue component.</summary>
        public float B { get; set; }

        /// <summary>
        /// Creates a new <see cref="Color"/>.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>Opaque black color (0, 0, 0).</summary>
        public static Color Black => new Color(0f, 0f, 0f);

        /// <summary>Opaque white color (1, 1, 1).</summary>
        public static Color White => new Color(1f, 1f, 1f);

        /// <summary>Returns the maximum color component value: Max(r,g,b).</summary>
        [JsonIgnore]
        public float MaxColorComponent => math.max(math.max(R, G), B);

        /// <summary>
        /// Implicit conversion to <see cref="UnityEngine.Color"/>. Alpha defaults to 1.
        /// </summary>
        /// <param name="c">Source color.</param>
        /// <returns>Unity Color.</returns>
        public static implicit operator UnityEngine.Color(Color c)
        {
            return new UnityEngine.Color(c.R, c.G, c.B, 1f);
        }

        /// <summary>
        /// Implicit conversion from <see cref="UnityEngine.Color"/>. Alpha is dropped.
        /// </summary>
        /// <param name="c">Source color.</param>
        /// <returns>glTF Color.</returns>
        public static implicit operator Color(UnityEngine.Color c)
        {
            return new Color(c.r, c.g, c.b);
        }

        /// <summary>Component-wise equality with floating point tolerance of <see cref="Constants.epsilon"/>.</summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color.</param>
        /// <returns>True if all components match within tolerance.</returns>
        public static bool operator ==(Color a, Color b)
        {
            return a.Equals(b);
        }

        /// <summary>Component-wise inequality with floating point tolerance of <see cref="Constants.epsilon"/>.</summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color.</param>
        /// <returns>True if any component differs beyond tolerance.</returns>
        public static bool operator !=(Color a, Color b)
        {
            return !a.Equals(b);
        }

        /// <inheritdoc/>
        public bool Equals(Color other)
        {
            return math.abs(R - other.R) <= Constants.epsilon
                && math.abs(G - other.G) <= Constants.epsilon
                && math.abs(B - other.B) <= Constants.epsilon;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Color other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B);
        }
    }
}
