// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Cloud.Gltfast.Objects;

namespace Unity.Cloud.Gltfast
{
    class MeshComparer
        : IEqualityComparer<MeshPrimitive>
        , IEqualityComparer<IReadOnlyList<MeshPrimitive>>
    {
        public bool Equals(IReadOnlyList<MeshPrimitive> x, IReadOnlyList<MeshPrimitive> y)
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

        public int GetHashCode(IReadOnlyList<MeshPrimitive> obj)
        {
            var hashCode = new HashCode();
            for (var index = 0; index < obj.Count; index++)
            {
                hashCode.Add(GetHashCode(obj[index]));
            }

            return hashCode.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(MeshPrimitive x, MeshPrimitive y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Indices == y.Indices
                && PrimitiveComparer.Equals(x.Attributes, y.Attributes)
                && PrimitiveComparer.Equals(x.Targets, y.Targets);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(MeshPrimitive primitive)
        {
            return HashCode.Combine(
                primitive.Indices,
                PrimitiveComparer.GetHashCode(primitive.Attributes),
                GetHashCode(primitive.Targets)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetHashCode(IReadOnlyList<MorphTarget> x)
        {
            if (x == null) return 0;
            HashCode hash = new();
            hash.Add(x.Count);
            for (var index = 0; index < x.Count; index++)
            {
                var target = x[index];
                if (target == null)
                {
                    hash.Add(0);
                    continue;
                }

                hash.Add(target.Position);
                hash.Add(target.Normal);
                hash.Add(target.Tangent);
            }

            return hash.ToHashCode();
        }
    }
}
