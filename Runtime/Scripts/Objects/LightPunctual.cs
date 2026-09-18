// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Light
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class LightPunctual : NamedObject
    {
        /// <summary>
        /// Light's color in linear space
        /// </summary>
        [JsonIgnore]
        public Color Color { get; set; } = Color.White;

        [JsonPropertyName("color"), JsonInclude]
        [JsonConverter(typeof(ColorConverter))]
        internal Color? ColorSerialized
        {
            get => Color == Color.White ? null : Color;
            set => Color = value ?? Color.White;
        }

        /// <summary>
        /// Brightness of light in. The units that this is defined in depend on
        /// the type of light. point and spot lights use luminous intensity in
        /// candela (lm/sr) while directional lights use illuminance
        /// in lux (lm/m2)
        /// </summary>
        [JsonIgnore]
        public float Intensity { get; set; } = 1f;

        [JsonPropertyName("intensity"), JsonInclude]
        internal float? IntensitySerialized
        {
            get => Mathematics.ApproximatelyOne(Intensity) ? null : Intensity;
            set => Intensity = value ?? 1f;
        }

        /// <summary>
        /// Hint defining a distance cutoff at which the light's intensity may
        /// be considered to have reached zero. Supported only for point and
        /// spot lights. Must be > 0. When undefined, range is assumed to be
        /// infinite.
        /// </summary>
        [JsonIgnore]
        public float Range { get; set; } = -1f;

        [JsonPropertyName("range"), JsonInclude]
        internal float? RangeSerialized
        {
            get => Mathematics.Approximately(Range, -1f) ? null : Range;
            set => Range = value ?? -1f;
        }

        /// <summary>
        /// Spot light properties (only set on spot lights).
        /// </summary>
        [JsonPropertyName("spot")]
        public SpotLight Spot { get; set; }

        /// <inheritdoc cref="LightType"/>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(LightTypeValueConverter))]
        public EnumOrRawValue<LightType> Type { get; set; }
    }
}
