// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using GLTFast.Schema;
using UnityEngine;
#if USING_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

namespace GLTFast.Export
{
    public static class KhrLightsPunctual
    {
        public static LightPunctual ConvertToLight(Light uLight)
        {
            var light = new LightPunctual
            {
                name = uLight.name
            };

            var renderPipeline = RenderPipelineUtils.RenderPipeline;

            var lightType = uLight.type;

#if USING_HDRP
            HDAdditionalLightData lightHd = null;

            if (renderPipeline == RenderPipeline.HighDefinition) {
                lightHd = uLight.gameObject.GetComponent<HDAdditionalLightData>();
#if !UNITY_2023_2_OR_NEWER
                lightType = lightHd != null ? lightHd.TryGetLightType() ?? lightType : lightType;
#endif
            }
#endif

            switch (lightType)
            {
                case LightType.Spot:
                    light.SetLightType(LightPunctual.Type.Spot);
                    light.spot = new SpotLight
                    {
                        outerConeAngle = uLight.spotAngle * Mathf.Deg2Rad * .5f,
                        innerConeAngle = uLight.innerSpotAngle * Mathf.Deg2Rad * .5f
                    };
                    break;
                case LightType.Directional:
                    light.SetLightType(LightPunctual.Type.Directional);
                    break;
                case LightType.Point:
                    light.SetLightType(LightPunctual.Type.Point);
                    break;
                case LightType.Rectangle:
                case LightType.Disc:
                default:
                    light.SetLightType(LightPunctual.Type.Spot);
                    light.spot = new SpotLight
                    {
                        outerConeAngle = 45 * Mathf.Deg2Rad * .5f,
                        innerConeAngle = 35 * Mathf.Deg2Rad * .5f
                    };
                    break;
            }

            light.LightColor = uLight.color.linear;
            light.range = GetLightRange(uLight, lightType);

            // Set Light intensity
            switch (renderPipeline)
            {
                case RenderPipeline.BuiltIn:
                    light.intensity = uLight.intensity * Mathf.PI;
                    break;
                case RenderPipeline.Universal:
                    light.intensity = uLight.intensity;
                    break;
#if USING_HDRP
                case RenderPipeline.HighDefinition:

                    if (lightHd == null)
                    {
                        light.intensity = uLight.intensity;
                    }
                    else
                    {
#if UNITY_2023_2_OR_NEWER
                        switch (lightType)
                        {
                            case LightType.Spot:
                            case LightType.Point:
#if RENDER_PIPELINES_CORE_17_OR_NEWER
                                light.intensity = LightUnitUtils.ConvertIntensity(uLight, uLight.intensity, uLight.lightUnit, LightUnit.Candela);
#else
                                light.intensity = GetIntensity(LightUnit.Candela);
#endif
                                break;
                            case LightType.Directional:
#if RENDER_PIPELINES_CORE_17_OR_NEWER
                                light.intensity = LightUnitUtils.ConvertIntensity(uLight, uLight.intensity, uLight.lightUnit, LightUnit.Lux);
#else
                                light.intensity = GetIntensity(LightUnit.Lux);
#endif
                                break;
                            case LightType.Rectangle:
                            default:
                                light.intensity = uLight.intensity;
                                break;
                        }
#else
                        switch (lightHd.type) {
                            case HDLightType.Spot:
                            case HDLightType.Point:
                                light.intensity = GetIntensity(LightUnit.Candela);
                                break;
                            case HDLightType.Directional:
                                light.intensity = GetIntensity(LightUnit.Lux);
                                break;
                            case HDLightType.Area:
                            default:
                                light.intensity = uLight.intensity;
                                break;
                        }
#endif
                    }

                    break;

#if !RENDER_PIPELINES_CORE_17_OR_NEWER
                    float GetIntensity(LightUnit unit)
                    {
#if UNITY_2023_3_OR_NEWER
                        if (uLight.lightUnit == unit)
                        {
                            return uLight.intensity;
                        }

                        // Workaround to get intensity in candela
                        var oldUnit = uLight.lightUnit;
                        uLight.lightUnit = unit;
                        var result = uLight.intensity;
                        uLight.lightUnit = oldUnit;
                        return result;
#else
                        if (lightHd.lightUnit == unit)
                        {
                            return lightHd.intensity;
                        }

                        // Workaround to get intensity in candela
                        var oldUnit = lightHd.lightUnit;
                        lightHd.lightUnit = unit;
                        var result = lightHd.intensity;
                        lightHd.lightUnit = oldUnit;
                        return result;
#endif
                    }
#endif // !RENDER_PIPELINES_CORE_17_OR_NEWER

#endif // USING_HDRP
                default:
                    light.intensity = uLight.intensity;
                    break;
            }

            return light;
        }

        /// <summary>
        /// Retrieves the light's range.
        /// In Unity 2023.1 and older `Light.range` would return what now is <see cref="Light.dilatedRange"/>, which is
        /// the range extended by a certain area light size factor. This method removes that addition in that case to
        /// get the original value that's shown in the inspector.
        /// </summary>
        /// <param name="light">Unity Light</param>
        /// <param name="lightType">Actual light type (might differ from light.type in HDRP).</param>
        /// <returns>The light's range.</returns>
        static float GetLightRange(Light light, LightType lightType)
        {
#if UNITY_2023_2_OR_NEWER
            return light.range;
#else
            var range = light.range;
            switch (lightType)
            {
#if !USING_HDRP // And, of course, it behaves correctly in the particular case HDRP+Rectangle.
                case LightType.Rectangle:
#if UNITY_EDITOR
                    var longestSide = light.areaSize.magnitude;
#else
                    // At runtime, assume default magnitude(vec2(1,1))
                    var longestSide = 1.4142135624f;
#endif
                    range -= longestSide * .5f;
                    break;
#endif // !USING_HDRP
                case LightType.Disc:
#if UNITY_EDITOR
                    var radius = light.areaSize.x;
#else
                    // At runtime, assume default
                    const float radius = 1f;
#endif
                    range -= radius * .5f;
                    break;
            }
            return range;
#endif
        }
    }
}
