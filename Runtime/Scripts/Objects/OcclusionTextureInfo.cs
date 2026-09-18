// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Occlusion map specific texture info
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class OcclusionTextureInfo : TextureInfo
    {
        /// <summary>
        /// A scalar multiplier controlling the amount of occlusion applied.
        /// A value of 0.0 means no occlusion.
        /// A value of 1.0 means full occlusion.
        /// This value is ignored if the corresponding texture is not specified.
        /// This value is linear.
        /// </summary>
        [JsonIgnore]
        public float Strength { get; set; } = 1f;

        [JsonPropertyName("strength"), JsonInclude]
        internal float? StrengthSerialized
        {
            get => Mathematics.ApproximatelyOne(Strength) ? null : Strength;
            set => Strength = value ?? 1f;
        }
    }
}
