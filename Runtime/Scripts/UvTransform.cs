// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Mathematics;
using UnityEngine;

namespace GLTFast
{
    struct UvTransform
    {
        public float rotation;
        public float2 scale;

        public static UvTransform FromMaterial(Material material, int scaleTransformPropertyId, int rotationPropertyId)
        {
            var st = material.GetVector(scaleTransformPropertyId);
            var r = material.GetVector(rotationPropertyId);

            return FromMatrix(
                new float2x2(st.x, st.y, r.x, r.y)
            );
        }

        public static UvTransform FromMatrix(float2x2 scaleRotation)
        {
            var result = new UvTransform
            {
                scale = new float2(
                    Mathematics.Normalize(new float2(scaleRotation.c0.x, scaleRotation.c1.y), out var r1),
                    Mathematics.Normalize(new float2(scaleRotation.c0.y, scaleRotation.c1.x), out var r2)
                )
            };

            var acos = math.acos(r1.x);
            if (r2.x < 0) acos = math.PI * 2 - acos;
            result.rotation = acos * math.TODEGREES;
            return result;
        }
    }
}
