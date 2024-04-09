// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_HDRP && !UNITY_2023_2_OR_NEWER

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace GLTFast.Export
{
    static class HDAdditionalLightDataExtensions
    {
        /// <summary>
        /// In older HDRP versions Light.type may not reflect the actual light type.
        /// To get the actual light type, the HDAdditionalLightData component has to be queried.
        /// </summary>
        /// <param name="lightHd">HDRP specific light component.</param>
        /// <returns>The HDRP specific light type or null otherwise.</returns>
        public static LightType? TryGetLightType(this HDAdditionalLightData lightHd)
        {
            switch (lightHd.type)
            {
                case HDLightType.Area:
                    switch (lightHd.areaLightShape)
                    {
                        case AreaLightShape.Rectangle:
                            return LightType.Rectangle;
                        case AreaLightShape.Tube:
                        case AreaLightShape.Disc:
                        default:
                            return LightType.Disc;
                    }
            }

            return null;
        }
    }
}
#endif
