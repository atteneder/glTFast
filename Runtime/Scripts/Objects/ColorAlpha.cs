// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Mathematics;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Linear RGBA color.
    /// </summary>
    public struct ColorAlpha : IEquatable<ColorAlpha>
    {
        /// <summary>Red component.</summary>
        public float R { get; set; }

        /// <summary>Green component.</summary>
        public float G { get; set; }

        /// <summary>Blue component.</summary>
        public float B { get; set; }

        /// <summary>Alpha component.</summary>
        public float A { get; set; }

        /// <summary>
        /// Creates a new <see cref="ColorAlpha"/>.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <param name="a">Alpha component. Defaults to 1.</param>
        public ColorAlpha(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>Opaque white color (1, 1, 1, 1).</summary>
        public static ColorAlpha White => new ColorAlpha(1f, 1f, 1f);

        /// <summary>
        /// Implicit conversion to <see cref="UnityEngine.Color"/>.
        /// </summary>
        /// <param name="c">Source color.</param>
        /// <returns>Unity Color.</returns>
        public static implicit operator UnityEngine.Color(ColorAlpha c)
        {
            return new UnityEngine.Color(c.R, c.G, c.B, c.A);
        }

        /// <summary>
        /// Implicit conversion from <see cref="UnityEngine.Color"/>.
        /// </summary>
        /// <param name="c">Source color.</param>
        /// <returns>glTF Color.</returns>
        public static implicit operator ColorAlpha(UnityEngine.Color c)
        {
            return new ColorAlpha(c.r, c.g, c.b, c.a);
        }

        /// <summary>Component-wise equality with floating point tolerance of <see cref="Constants.epsilon"/>.</summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color.</param>
        /// <returns>True if all components match within tolerance.</returns>
        public static bool operator ==(ColorAlpha a, ColorAlpha b)
        {
            return a.Equals(b);
        }

        /// <summary>Component-wise inequality with floating point tolerance of <see cref="Constants.epsilon"/>.</summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color.</param>
        /// <returns>True if any component differs beyond tolerance.</returns>
        public static bool operator !=(ColorAlpha a, ColorAlpha b)
        {
            return !a.Equals(b);
        }

        /// <inheritdoc/>
        public bool Equals(ColorAlpha other)
        {
            return math.abs(R - other.R) <= Constants.epsilon
                && math.abs(G - other.G) <= Constants.epsilon
                && math.abs(B - other.B) <= Constants.epsilon
                && math.abs(A - other.A) <= Constants.epsilon;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is ColorAlpha other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }
    }
}
