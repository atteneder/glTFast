// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Texture sampler properties for filtering and wrapping modes.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Sampler : NamedObject, IAdditionalPropertyContainer
    {
        /// <summary>
        /// Magnification filter.
        /// Valid values correspond to WebGL enums: `9728` (NEAREST) and `9729` (LINEAR).
        /// </summary>
        [JsonPropertyName("magFilter")]
        public MagFilterMode MagFilter { get; set; } = MagFilterMode.Undefined;

        /// <summary>
        /// Minification filter. All valid values correspond to WebGL enums.
        /// </summary>
        [JsonPropertyName("minFilter")]
        public MinFilterMode MinFilter { get; set; } = MinFilterMode.Undefined;

        /// <summary>
        /// s wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        [JsonIgnore]
        public WrapMode WrapS { get; set; } = WrapMode.Repeat;

        [JsonPropertyName("wrapS"), JsonInclude]
        internal WrapMode? WrapSSerialized
        {
            get => WrapS is WrapMode.Repeat or WrapMode.Undefined ? null : WrapS;
            set => WrapS = value ?? WrapMode.Repeat;
        }

        /// <summary>
        /// t wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        [JsonIgnore]
        public WrapMode WrapT { get; set; } = WrapMode.Repeat;

        [JsonPropertyName("wrapT"), JsonInclude]
        internal WrapMode? WrapTSerialized
        {
            get => WrapT is WrapMode.Repeat or WrapMode.Undefined ? null : WrapT;
            set => WrapT = value ?? WrapMode.Repeat;
        }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public SamplerExtensions Extensions { get; set; }

        /// <inheritdoc cref="Root.Extras"/>
        [JsonPropertyName("extras")]
        [JsonConverter(typeof(ExtrasConverter))]
        public ExtrasContainer Extras { get; set; }

        /// <summary>JSON properties without a matching member.</summary>
        [JsonExtensionData, JsonInclude]
        internal Dictionary<string, JsonElement> ExtensionData { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public ReadOnlyProperties AdditionalProperties => new(ExtensionData ?? ReadOnlyProperties.Empty);


        /// <summary>
        /// Unity filter mode, derived from glTF's
        /// <see cref="MinFilter"/> and <see cref="MagFilter"/>.
        /// </summary>
        [JsonIgnore]
        public FilterMode FilterMode => ConvertFilterMode(MinFilter, MagFilter);

        /// <summary>
        /// Unity texture wrap mode (horizontal), derived from glTF's
        /// <see cref="WrapS"/> value.
        /// </summary>
        [JsonIgnore]
        public TextureWrapMode WrapU => ConvertWrapMode(WrapS);

        /// <summary>
        /// Unity texture wrap mode (vertical), derived from glTF's
        /// <see cref="WrapT"/> value.
        /// </summary>
        [JsonIgnore]
        public TextureWrapMode WrapV => ConvertWrapMode(WrapT);

        static FilterMode ConvertFilterMode(MinFilterMode minFilterToConvert, MagFilterMode magFilterToConvert)
        {
            switch (minFilterToConvert)
            {
                case MinFilterMode.LinearMipmapLinear:
                    return FilterMode.Trilinear;
                case MinFilterMode.Nearest:
                case MinFilterMode.NearestMipmapNearest:
                case MinFilterMode.NearestMipmapLinear: // incorrect mip-map filtering in this case!
                    return FilterMode.Point;
            }
            switch (magFilterToConvert)
            {
                case MagFilterMode.Nearest:
                    return FilterMode.Point;
                default:
                    return FilterMode.Bilinear;
            }
        }

        static TextureWrapMode ConvertWrapMode(WrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case WrapMode.Undefined:
                case WrapMode.Repeat:
                default:
                    return TextureWrapMode.Repeat;
                case WrapMode.ClampToEdge:
                    return TextureWrapMode.Clamp;
                case WrapMode.MirroredRepeat:
                    return TextureWrapMode.Mirror;
            }
        }

        static WrapMode ConvertWrapMode(TextureWrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case TextureWrapMode.Clamp:
                    return WrapMode.ClampToEdge;
                case TextureWrapMode.Mirror:
                case TextureWrapMode.MirrorOnce:
                    return WrapMode.MirroredRepeat;
                case TextureWrapMode.Repeat:
                default:
                    return WrapMode.Repeat;
            }
        }


        /// <summary>
        /// Parameter-less constructor
        /// </summary>
        public Sampler() { }

        /// <summary>
        /// Constructs a Sampler with filter and wrap modes.
        /// </summary>
        /// <param name="filterMode">Unity texture filter mode</param>
        /// <param name="wrapModeU">Unity texture wrap mode (horizontal)</param>
        /// <param name="wrapModeV">Unity texture wrap mode (vertical)</param>
        public Sampler(FilterMode filterMode, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV)
        {
            switch (filterMode)
            {
                case FilterMode.Point:
                    MagFilter = MagFilterMode.Nearest;
                    MinFilter = MinFilterMode.Nearest;
                    break;
                case FilterMode.Bilinear:
                    MagFilter = MagFilterMode.Linear;
                    MinFilter = MinFilterMode.Linear;
                    break;
                case FilterMode.Trilinear:
                    MagFilter = MagFilterMode.Linear;
                    MinFilter = MinFilterMode.LinearMipmapLinear;
                    break;
            }

            WrapS = ConvertWrapMode(wrapModeU);
            WrapT = ConvertWrapMode(wrapModeV);
        }

        /// <summary>
        /// Applies the Sampler's settings to a Unity texture.
        /// </summary>
        /// <param name="image">Texture to apply the settings to</param>
        /// <param name="defaultMinFilter">Fallback minification filter</param>
        /// <param name="defaultMagFilter">Fallback magnification filter</param>
        public void Apply(Texture2D image,
                          MinFilterMode defaultMinFilter = MinFilterMode.Linear,
                          MagFilterMode defaultMagFilter = MagFilterMode.Linear)
        {
            if (image == null) return;
            image.wrapModeU = WrapU;
            image.wrapModeV = WrapV;

            // Use the default filtering mode for textures that have no such specification in data
            image.filterMode = ConvertFilterMode(
                MinFilter == MinFilterMode.Undefined ? defaultMinFilter : MinFilter,
                MagFilter == MagFilterMode.Undefined ? defaultMagFilter : MagFilter
            );
        }
    }
}
