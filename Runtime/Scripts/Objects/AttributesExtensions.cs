// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Bounds-checked, allocation-aware index accessors for the indexed
    /// attribute families on <see cref="Attributes"/> (<c>TEXCOORD_n</c>,
    /// <c>COLOR_n</c>, <c>JOINTS_n</c>, <c>WEIGHTS_n</c>).
    /// </summary>
    public static class AttributesExtensions
    {
        /// <summary>Returns the number of texture coordinate sets.</summary>
        /// <param name="attributes">Vertex attributes.</param>
        /// <returns>Number of texture coordinate sets.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTexCoordsCount(this Attributes attributes) => attributes.TexCoords?.Count ?? 0;

        /// <summary>
        /// Returns <c>TEXCOORD_&lt;n&gt;</c>'s accessor index (where <c>n</c> is <paramref name="index"/>), or <see langword="null"/> if unset.
        /// </summary>
        /// <param name="attributes">Vertex attributes.</param>
        /// <param name="index">Texture coordinates index.</param>
        /// <returns><see cref="Accessor"/> index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int? GetTexCoord(this Attributes attributes, int index) => GetAt(attributes.TexCoords, index);

        /// <summary>Returns <c>COLOR_&lt;n&gt;</c>'s accessor index (where <c>n</c> is <paramref name="index"/>), or <see langword="null"/> if unset.</summary>
        /// <param name="attributes">Vertex attributes.</param>
        /// <param name="index">Color set index.</param>
        /// <returns><see cref="Accessor"/> index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int? GetColor(this Attributes attributes, int index) => GetAt(attributes.Colors, index);

        /// <summary>Returns <c>JOINTS_&lt;n&gt;</c>'s accessor index (where <c>n</c> is <paramref name="index"/>), or <see langword="null"/> if unset.</summary>
        /// <param name="attributes">Vertex attributes.</param>
        /// <param name="index">Joints set index.</param>
        /// <returns><see cref="Accessor"/> index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int? GetJoint(this Attributes attributes, int index) => GetAt(attributes.Joints, index);

        /// <summary>Returns <c>WEIGHTS_&lt;n&gt;</c>'s accessor index (where <c>n</c> is <paramref name="index"/>), or <see langword="null"/> if unset.</summary>
        /// <param name="attributes">Vertex attributes.</param>
        /// <param name="index">Weights set index.</param>
        /// <returns><see cref="Accessor"/> index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int? GetWeight(this Attributes attributes, int index) => GetAt(attributes.Weights, index);

        /// <summary>
        /// Sets <c>TEXCOORD_&lt;n&gt;</c>'s accessor index (where <c>n</c> is <paramref name="index"/>). Lazily allocates
        /// <see cref="Attributes.TexCoords"/> and pads with <see langword="null"/> as needed.
        /// </summary>
        /// <param name="attributes">Vertex attributes.</param>
        /// <param name="index">Texture coordinates index.</param>
        /// <param name="value"><see cref="Accessor"/> index, or <see langword="null"/> to clear.</param>
        public static void SetTexCoord(this Attributes attributes, int index, int? value)
            => attributes.TexCoords = AssignSparse(attributes.TexCoords, index, value);

        /// <summary>Sets <c>COLOR_&lt;n&gt;</c>'s accessor index (where <c>n</c> is <paramref name="index"/>). See <see cref="SetTexCoord"/>.</summary>
        /// <param name="attributes">Vertex attributes.</param>
        /// <param name="index">Color set index.</param>
        /// <param name="value"><see cref="Accessor"/> index, or <see langword="null"/> to clear.</param>
        public static void SetColor(this Attributes attributes, int index, int? value)
            => attributes.Colors = AssignSparse(attributes.Colors, index, value);

        /// <summary>Sets <c>JOINTS_&lt;n&gt;</c>'s accessor index (where <c>n</c> is <paramref name="index"/>). See <see cref="SetTexCoord"/>.</summary>
        /// <param name="attributes">Vertex attributes.</param>
        /// <param name="index">Joints set index.</param>
        /// <param name="value"><see cref="Accessor"/> index, or <see langword="null"/> to clear.</param>
        public static void SetJoint(this Attributes attributes, int index, int? value)
            => attributes.Joints = AssignSparse(attributes.Joints, index, value);

        /// <summary>Sets <c>WEIGHTS_&lt;n&gt;</c>'s accessor index (where <c>n</c> is <paramref name="index"/>). See <see cref="SetTexCoord"/>.</summary>
        /// <param name="attributes">Vertex attributes.</param>
        /// <param name="index">Weights set index.</param>
        /// <param name="value"><see cref="Accessor"/> index, or <see langword="null"/> to clear.</param>
        public static void SetWeight(this Attributes attributes, int index, int? value)
            => attributes.Weights = AssignSparse(attributes.Weights, index, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int? GetAt(List<int?> list, int index)
        {
            return index < list?.Count ? list[index] : null;
        }

        static List<int?> AssignSparse(List<int?> list, int index, int? value)
        {
            if (value.HasValue)
            {
                list ??= new List<int?>(index + 1);
                if (list.Count <= index)
                {
                    if (list.Capacity <= index)
                    {
                        list.Capacity = index + 1;
                    }
                    while (list.Count <= index) list.Add(null);
                }
                list[index] = value;
            }
            else if (list != null && (uint)index < (uint)list.Count)
            {
                list[index] = null;
            }
            return list;
        }
    }
}
