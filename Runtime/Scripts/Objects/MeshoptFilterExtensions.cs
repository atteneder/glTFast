// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if MESHOPT_IS_RECENT

using System;
using Meshoptimizer;

namespace Unity.Cloud.Gltfast.Objects
{
    public static class MeshoptFilterExtensions
    {
        public static Filter ToMeshoptimizerFilter(this MeshoptFilter filter)
        {
            return filter switch
            {
                MeshoptFilter.None => Filter.None,
                MeshoptFilter.Octahedral => Filter.Octahedral,
                MeshoptFilter.Quaternion => Filter.Quaternion,
                MeshoptFilter.Exponential => Filter.Exponential,
                _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
            };
        }
    }
}
#endif // MESHOPT_IS_RECENT
