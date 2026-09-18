// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

#if USING_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Unity.Cloud.Gltfast
{

    using Objects;

    /// <summary>
    /// Extension methods for <see cref="LightPunctual"/>
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
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
            switch (lightSource.Type.Value)
            {
                case LightType.Undefined:
                    break;
                case LightType.Spot:
                    lightDestination.type = UnityEngine.LightType.Spot;
                    break;
                case LightType.Directional:
                    lightDestination.type = UnityEngine.LightType.Directional;
                    break;
                case LightType.Point:
                    lightDestination.type = UnityEngine.LightType.Point;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            lightDestination.useColorTemperature = false;
            lightDestination.color = ((UnityEngine.Color)lightSource.Color).gamma;

            LightAssignIntensity(lightDestination, lightSource, lightIntensityFactor);

            lightDestination.range = lightSource.Range > 0
                ? lightSource.Range
                : 100_000; // glTF 2.0 spec says infinite, but float.MaxValue
                           // breaks spot lights in URP.

            if (lightSource.Type == LightType.Spot)
            {
                lightDestination.spotAngle = lightSource.Spot.OuterConeAngle * Mathf.Rad2Deg * 2f;
                lightDestination.innerSpotAngle = lightSource.Spot.InnerConeAngle * Mathf.Rad2Deg * 2f;
#if USING_HDRP && !UNITY_6000_3_OR_NEWER
                var lightHd = lightDestination.gameObject.GetComponent<HDAdditionalLightData>();
                lightHd.SetSpotAngle(
                    lightSource.Spot.OuterConeAngle * Mathf.Rad2Deg * 2f,
                    100 * lightSource.Spot.InnerConeAngle / lightSource.Spot.OuterConeAngle
                );
#endif
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
                case UnityEngine.LightType.Spot:
                    lightDestination.Type = LightType.Spot;
                    break;
                case UnityEngine.LightType.Directional:
                    lightDestination.Type = LightType.Directional;
                    break;
                case UnityEngine.LightType.Point:
                    lightDestination.Type = LightType.Point;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            lightDestination.Color = lightSource.color;

            LightAssignIntensity(lightDestination, lightSource, lightIntensityFactor);

            lightDestination.Range = lightSource.range > 0
                ? lightSource.range
                : 100_000; // glTF 2.0 spec says infinite, but float.MaxValue
                           // breaks spot lights in URP.

            if (lightSource.type == UnityEngine.LightType.Spot)
            {
                lightDestination.Spot = lightDestination.Spot ?? new SpotLight();
                lightDestination.Spot.OuterConeAngle = lightSource.spotAngle / Mathf.Rad2Deg * 0.5f;
                lightDestination.Spot.InnerConeAngle = lightSource.innerSpotAngle / Mathf.Rad2Deg * 0.5f;
            }
        }

        static void LightAssignIntensity(Light lightDestination, LightPunctual lightSource, float lightIntensityFactor)
        {
            var intensity = lightSource.Intensity * lightIntensityFactor;
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
                    var lightUnit = lightSource.Type == LightType.Directional
                        ? LightUnit.Lux
                        : LightUnit.Candela;
                    lightDestination.gameObject.AddComponent<HDAdditionalLightData>();
                    lightDestination.lightUnit = lightUnit;
                    lightDestination.intensity = lightSource.Intensity;
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
                    lightDestination.Intensity = intensity * Mathf.PI;
                    break;
                case RenderPipeline.Universal:
                    lightDestination.Intensity = intensity;
                    break;
#if USING_HDRP
                case RenderPipeline.HighDefinition:
                    lightDestination.Intensity = lightSource.intensity;
                    break;
#endif
                default:
                    lightDestination.Intensity = intensity;
                    break;
            }
        }
    }
}
