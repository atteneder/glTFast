// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION || GLTFAST_ANIMATION

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class AnimationChannelTarget : IAdditionalPropertyContainer
    {
        /// <summary>
        /// The index of the node to target.
        /// </summary>
        /// <remarks>
        /// Optional per the glTF specification. <see langword="null"/> signals an absent target;
        /// when undefined, the animated object may be defined by an extension.
        /// </remarks>
        [JsonPropertyName("node")]
        public int? Node { get; set; }

        /// <inheritdoc cref="AnimationPath"/>
        [JsonPropertyName("path")]
        [JsonConverter(typeof(AnimationPathValueConverter))]
        public EnumOrRawValue<AnimationPath> Path { get; set; }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public AnimationChannelTargetExtensions Extensions { get; set; }

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

    }
}
#endif // UNITY_ANIMATION || GLTFAST_ANIMATION
