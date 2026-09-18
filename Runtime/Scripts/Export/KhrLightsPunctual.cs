// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Objects;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using LightType = UnityEngine.LightType;
#if USING_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Unity.Cloud.Gltfast.Export
{
    /// <summary>
    /// Provides conversion from Unity light components to glTF lights.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Export", sourceAssembly: "glTFast.Export")]
    public static class KhrLightsPunctual
    {
        /// <summary>
        /// Converts a Unity light component to a glTF light.
        /// </summary>
        /// <param name="uLight">Unity light component.</param>
        /// <returns>glTF light.</returns>
        public static LightPunctual ConvertToLight(Light uLight)
        {
            var light = new LightPunctual
            {
                Name = uLight.name
            };

            var renderPipeline = RenderPipelineUtils.RenderPipeline;

            var lightType = uLight.type;

#if USING_HDRP
            HDAdditionalLightData lightHd = null;

            if (renderPipeline == RenderPipeline.HighDefinition)
            {
                lightHd = uLight.gameObject.GetComponent<HDAdditionalLightData>();
            }
#endif

            switch (lightType)
            {
                case LightType.Spot:
                    light.Type = Objects.LightType.Spot;
                    light.Spot = new SpotLight
                    {
                        OuterConeAngle = uLight.spotAngle * Mathf.Deg2Rad * .5f,
                    };

#if USING_HDRP && !UNITY_6000_3_OR_NEWER
                    if (renderPipeline == RenderPipeline.HighDefinition)
                    {
                        // Up until Unity 6.2/HDRP 17.2 lightHd.innerSpotPercent was used
                        // instead of uLight.innerSpotAngle.
                        light.Spot.InnerConeAngle = lightHd != null
                            ? uLight.spotAngle * Mathf.Deg2Rad * .5f * lightHd.innerSpotPercent01
                            : 0;
                    }
                    else
#endif
                    {
                        light.Spot.InnerConeAngle = uLight.innerSpotAngle * Mathf.Deg2Rad * .5f;
                    }
                    break;
                case LightType.Directional:
                    light.Type = Objects.LightType.Directional;
                    break;
                case LightType.Point:
                    light.Type = Objects.LightType.Point;
                    break;
                case LightType.Rectangle:
                case LightType.Disc:
                default:
                    light.Type = Objects.LightType.Spot;
                    light.Spot = new SpotLight
                    {
                        OuterConeAngle = 45 * Mathf.Deg2Rad * .5f,
                        InnerConeAngle = 35 * Mathf.Deg2Rad * .5f
                    };
                    break;
            }

            light.Color = uLight.color.linear;
            if (lightType != LightType.Directional)
            {
                light.Range = uLight.range;
            }

            // Set Light intensity
            switch (renderPipeline)
            {
                case RenderPipeline.BuiltIn:
                    light.Intensity = uLight.intensity * Mathf.PI;
                    break;
                case RenderPipeline.Universal:
                    light.Intensity = uLight.intensity;
                    break;
#if USING_HDRP
                case RenderPipeline.HighDefinition:

                    if (lightHd == null)
                    {
                        light.Intensity = uLight.intensity;
                    }
                    else
                    {
                        switch (lightType)
                        {
                            case LightType.Spot:
                            case LightType.Point:
                                light.Intensity = LightUnitUtils.ConvertIntensity(uLight, uLight.intensity, uLight.lightUnit, LightUnit.Candela);
                                break;
                            case LightType.Directional:
                                light.Intensity = LightUnitUtils.ConvertIntensity(uLight, uLight.intensity, uLight.lightUnit, LightUnit.Lux);
                                break;
                            case LightType.Rectangle:
                            default:
                                light.Intensity = uLight.intensity;
                                break;
                        }
                    }
                    break;
#endif // USING_HDRP
                default:
                    light.Intensity = uLight.intensity;
                    break;
            }

            return light;
        }
    }
}
