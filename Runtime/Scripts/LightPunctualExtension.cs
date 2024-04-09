// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

#if USING_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

namespace GLTFast
{

    using Schema;

    /// <summary>
    /// Extension methods for <see cref="LightPunctual"/>
    /// </summary>
    public static class LightPunctualExtension
    {

        /// <summary>
        /// Initialize a UnityEngine.Light from a LightsPunctual
        /// </summary>
        /// <param name="lightSource">light to get data from</param>
        /// <param name="lightDestination">light to initialize</param>
        /// <param name="lightIntensityFactor">light intensity conversion factor</param>
        public static void ToUnityLight(this LightPunctual lightSource, Light lightDestination, float lightIntensityFactor)
        {
            switch (lightSource.GetLightType())
            {
                case LightPunctual.Type.Unknown:
                    break;
                case LightPunctual.Type.Spot:
                    lightDestination.type = LightType.Spot;
                    break;
                case LightPunctual.Type.Directional:
                    lightDestination.type = LightType.Directional;
                    break;
                case LightPunctual.Type.Point:
                    lightDestination.type = LightType.Point;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            lightDestination.useColorTemperature = false;
            lightDestination.color = lightSource.LightColor.gamma;

            LightAssignIntensity(lightDestination, lightSource, lightIntensityFactor);

            lightDestination.range = lightSource.range > 0
                ? lightSource.range
                : 100_000; // glTF 2.0 spec says infinite, but float.MaxValue
                           // breaks spot lights in URP.

            if (lightSource.GetLightType() == LightPunctual.Type.Spot)
            {
                lightDestination.spotAngle = lightSource.spot.outerConeAngle * Mathf.Rad2Deg * 2f;
                lightDestination.innerSpotAngle = lightSource.spot.innerConeAngle * Mathf.Rad2Deg * 2f;
            }
        }

        /// <summary>
        /// Initialize a LightPunctual from a UnityEngine.Light
        /// </summary>
        /// <param name="lightDestination">light to initialize</param>
        /// <param name="lightSource">light to get data from</param>
        /// <param name="lightIntensityFactor">light intensity conversion factor</param>
        public static void ToLightPunctual(this Light lightSource, LightPunctual lightDestination, float lightIntensityFactor)
        {
            switch (lightSource.type)
            {
                case LightType.Spot:
                    lightDestination.SetLightType(LightPunctual.Type.Spot);
                    break;
                case LightType.Directional:
                    lightDestination.SetLightType(LightPunctual.Type.Directional);
                    break;
                case LightType.Point:
                    lightDestination.SetLightType(LightPunctual.Type.Point);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            lightDestination.LightColor = lightSource.color;

            LightAssignIntensity(lightDestination, lightSource, lightIntensityFactor);

            lightDestination.range = lightSource.range > 0
                ? lightSource.range
                : 100_000; // glTF 2.0 spec says infinite, but float.MaxValue
                           // breaks spot lights in URP.

            if (lightSource.type == LightType.Spot)
            {
                lightDestination.spot = lightDestination.spot ?? new SpotLight();
                lightDestination.spot.outerConeAngle = lightSource.spotAngle / Mathf.Rad2Deg * 0.5f;
                lightDestination.spot.innerConeAngle = lightSource.innerSpotAngle / Mathf.Rad2Deg * 0.5f;
            }
        }

        static void LightAssignIntensity(Light lightDestination, LightPunctual lightSource, float lightIntensityFactor)
        {
            var intensity = lightSource.intensity * lightIntensityFactor;
            var renderPipeline = RenderPipelineUtils.RenderPipeline;
            switch (renderPipeline)
            {
                case RenderPipeline.BuiltIn:
                    lightDestination.intensity = intensity / Mathf.PI;
                    break;
                case RenderPipeline.Universal:
                    lightDestination.intensity = intensity;
                    break;
#if USING_HDRP
                case RenderPipeline.HighDefinition:
                    var lightUnit = lightSource.GetLightType() == LightPunctual.Type.Directional
                        ? LightUnit.Lux
                        : LightUnit.Candela;

#if USING_HDRP_17_OR_NEWER
                    lightDestination.gameObject.AddComponent<HDAdditionalLightData>();
                    lightDestination.lightUnit = lightUnit;
                    lightDestination.intensity = lightSource.intensity;
#else
                    var lightHd = lightDestination.gameObject.AddComponent<HDAdditionalLightData>();
                    lightHd.lightUnit = lightUnit;
                    lightHd.intensity = lightSource.intensity;
#endif
                    break;
#endif
                default:
                    lightDestination.intensity = intensity;
                    break;
            }
        }

        static void LightAssignIntensity(LightPunctual lightDestination, Light lightSource, float lightIntensityFactor)
        {
            var intensity = lightSource.intensity / lightIntensityFactor;
            var renderPipeline = RenderPipelineUtils.RenderPipeline;
            switch (renderPipeline)
            {
                case RenderPipeline.BuiltIn:
                    lightDestination.intensity = intensity * Mathf.PI;
                    break;
                case RenderPipeline.Universal:
                    lightDestination.intensity = intensity;
                    break;
#if USING_HDRP
                case RenderPipeline.HighDefinition:
#if USING_HDRP_17_OR_NEWER
                    lightDestination.intensity = lightSource.intensity;
#else
                    if (lightSource.gameObject.TryGetComponent(out HDAdditionalLightData lightHd))
                        lightDestination.intensity = lightHd.intensity;
                    else
                        lightDestination.intensity = 1;
#endif
                    break;
#endif
                default:
                    lightDestination.intensity = intensity;
                    break;
            }
        }
    }
}
