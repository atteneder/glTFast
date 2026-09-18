// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// The image's MIME type enumeration specifying the encoding of the image
    /// data.
    /// </summary>
    /// <remarks>
    /// <see cref="Undefined"/> doubles as the "not specified" state when used via
    /// <see cref="EnumOrRawValue{TEnum}"/>: glTF JSON never produces the
    /// literal <c>Unknown</c> MIME string, so the collapse between
    /// "absent <c>mimeType</c>" and "explicitly <c>Unknown</c>" is safe.
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter<ImageMimeType>))]
    public enum ImageMimeType
    {
        /// <summary>MIME type not specified or unrecognized.</summary>
        Undefined,

        /// <summary>JPEG image (<c>image/jpeg</c>).</summary>
        [JsonStringEnumMemberName("image/jpeg")]
        Jpeg,

        /// <summary>PNG image (<c>image/png</c>).</summary>
        [JsonStringEnumMemberName("image/png")]
        Png,

        /// <summary>KTX2 image (<c>image/ktx2</c>).</summary>
        [JsonStringEnumMemberName("image/ktx2")]
        Ktx2,

        /// <summary>WebP image (<c>image/webp</c>).</summary>
        [JsonStringEnumMemberName("image/webp")]
        WebP,
    }
}
