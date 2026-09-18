// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GLTFast.Schema;

namespace GLTFast
{
    class MeshComparer
        : IEqualityComparer<MeshPrimitiveBase>
        , IEqualityComparer<IReadOnlyList<MeshPrimitiveBase>>
    {
        public bool Equals(IReadOnlyList<MeshPrimitiveBase> x, IReadOnlyList<MeshPrimitiveBase> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.Count != y.Count) return false;
            for (var index = 0; index < x.Count; index++)
            {
                if (!Equals(x[index], y[index]))
                    return false;
            }
            return true;
        }

        public int GetHashCode(IReadOnlyList<MeshPrimitiveBase> obj)
        {
            var hashCode = new HashCode();
            foreach (var primitive in obj)
            {
                hashCode.Add(GetHashCode(primitive));
            }
            return hashCode.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(MeshPrimitiveBase x, MeshPrimitiveBase y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.indices == y.indices
                && Equals(x.attributes, y.attributes)
                && Equals(x.targets, y.targets);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(MeshPrimitiveBase primitive)
        {
            return HashCode.Combine(
                primitive.indices,
                GetHashCode(primitive.attributes),
                GetHashCode(primitive.targets)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetHashCode(Attributes x)
        {
            if (x == null) return 0;
            HashCode hash = new();
            hash.Add(x.POSITION);
            hash.Add(x.NORMAL);
            hash.Add(x.TANGENT);
            hash.Add(x.TEXCOORD_0);
            hash.Add(x.TEXCOORD_1);
            hash.Add(x.TEXCOORD_2);
            hash.Add(x.TEXCOORD_3);
            hash.Add(x.TEXCOORD_4);
            hash.Add(x.TEXCOORD_5);
            hash.Add(x.TEXCOORD_6);
            hash.Add(x.TEXCOORD_7);
            hash.Add(x.COLOR_0);
            hash.Add(x.JOINTS_0);
            hash.Add(x.WEIGHTS_0);
            return hash.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetHashCode(MorphTarget[] x)
        {
            if (x == null) return 0;
            HashCode hash = new();
            hash.Add(x.Length);
            foreach (var target in x)
            {
                if (target == null)
                {
                    hash.Add(0);
                    continue;
                }
                hash.Add(target.POSITION);
                hash.Add(target.NORMAL);
                hash.Add(target.TANGENT);
            }
            return hash.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Equals(MorphTarget[] x, MorphTarget[] y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;
            for (var i = 0; i < x.Length; i++)
            {
                if (!Equals(x[i], y[i]))
                    return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Equals(MorphTarget x, MorphTarget y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.POSITION == y.POSITION
                && x.NORMAL == y.NORMAL
                && x.TANGENT == y.TANGENT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Equals(Attributes x, Attributes y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.POSITION == y.POSITION
                && x.NORMAL == y.NORMAL
                && x.TANGENT == y.TANGENT
                && x.TEXCOORD_0 == y.TEXCOORD_0
                && x.TEXCOORD_1 == y.TEXCOORD_1
                && x.TEXCOORD_2 == y.TEXCOORD_2
                && x.TEXCOORD_3 == y.TEXCOORD_3
                && x.TEXCOORD_4 == y.TEXCOORD_4
                && x.TEXCOORD_5 == y.TEXCOORD_5
                && x.TEXCOORD_6 == y.TEXCOORD_6
                && x.TEXCOORD_7 == y.TEXCOORD_7
                && x.COLOR_0 == y.COLOR_0
                && x.JOINTS_0 == y.JOINTS_0
                && x.WEIGHTS_0 == y.WEIGHTS_0;
        }
    }
}
