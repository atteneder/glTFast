// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if DEBUG

using System;
using System.Collections.Generic;
using GLTFast.Schema;

namespace GLTFast
{
    /// <summary>
    /// This is similar to <see cref="MeshComparer"/>, except it does not take the indices into account.
    /// That's useful to detect meshes that share large vertex buffers, but have different indices, which is
    /// inefficient (in Unity) and discouraged.
    /// </summary>
    class PrimitivesComparer : IEqualityComparer<IReadOnlyList<MeshPrimitiveBase>>
    {
        public bool Equals(IReadOnlyList<MeshPrimitiveBase> x, IReadOnlyList<MeshPrimitiveBase> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.Count != y.Count) return false;
            for (var index = 0; index < x.Count; index++)
            {
                if (!PrimitiveComparer.HaveEqualVertexBuffers(x[index], y[index]))
                    return false;
            }
            return true;
        }

        public int GetHashCode(IReadOnlyList<MeshPrimitiveBase> obj)
        {
            var hashCode = new HashCode();
            foreach (var primitive in obj)
            {
                hashCode.Add(PrimitiveComparer.CalculateHashCode(primitive));
            }
            return hashCode.ToHashCode();
        }
    }
}
#endif
