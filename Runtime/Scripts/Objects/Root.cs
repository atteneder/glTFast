// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// The root object for a glTF asset.
    /// </summary>
    /// <seealso href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#reference-gltf"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Root : IAdditionalPropertyContainer
    {
        /// <summary>
        /// Names of glTF extensions used somewhere in this asset.
        /// </summary>
        /// <remarks>
        /// Recognized extensions deserialize into <see cref="EnumOrRawValue{TEnum}.Value"/>.
        /// Unknown extensions are kept as UTF-8 bytes in <see cref="EnumOrRawValue{TEnum}.RawValue"/>;
        /// in that case <see cref="EnumOrRawValue{TEnum}.Value"/> is not authoritative.
        /// </remarks>
        [JsonPropertyName("extensionsUsed")]
        [JsonConverter(typeof(ExtensionListConverter))]
        public List<EnumOrRawValue<Extension>> ExtensionsUsed { get; set; }

        /// <summary>
        /// Names of glTF extensions required to properly load this asset.
        /// </summary>
        /// <remarks>
        /// Recognized extensions deserialize into <see cref="EnumOrRawValue{TEnum}.Value"/>.
        /// Unknown extensions are kept as UTF-8 bytes in <see cref="EnumOrRawValue{TEnum}.RawValue"/>;
        /// in that case <see cref="EnumOrRawValue{TEnum}.Value"/> is not authoritative.
        /// </remarks>
        [JsonPropertyName("extensionsRequired")]
        [JsonConverter(typeof(ExtensionListConverter))]
        public List<EnumOrRawValue<Extension>> ExtensionsRequired { get; set; }

        /// <summary>
        /// An array of accessors. An accessor is a typed view into a bufferView.
        /// </summary>
        [JsonPropertyName("accessors")]
        public List<Accessor> Accessors { get; set; }

#if UNITY_ANIMATION || GLTFAST_ANIMATION
        /// <summary>
        /// An array of keyframe animations.
        /// </summary>
        [JsonPropertyName("animations")]
        public List<Animation> Animations { get; set; }
#endif

        /// <summary>
        /// Metadata about the glTF asset.
        /// </summary>
        [JsonPropertyName("asset")]
        public Asset Asset { get; set; }

        /// <summary>
        /// An array of buffers. A buffer points to binary geometry, animation, or skins.
        /// </summary>
        [JsonPropertyName("buffers")]
        public List<Buffer> Buffers { get; set; }

        /// <summary>
        /// An array of bufferViews.
        /// A bufferView is a view into a buffer generally representing a subset of the buffer.
        /// </summary>
        [JsonPropertyName("bufferViews")]
        public List<BufferView> BufferViews { get; set; }

        /// <summary>
        /// An array of cameras. A camera defines a projection matrix.
        /// </summary>
        [JsonPropertyName("cameras")]
        public List<Camera> Cameras { get; set; }

        /// <summary>
        /// An array of images. An image defines data used to create a texture.
        /// </summary>
        [JsonPropertyName("images")]
        public List<Image> Images { get; set; }

        /// <summary>
        /// An array of materials. A material defines the appearance of a primitive.
        /// </summary>
        [JsonPropertyName("materials")]
        public List<Material> Materials { get; set; }

        /// <summary>
        /// An array of meshes. A mesh is a set of primitives to be rendered.
        /// </summary>
        [JsonPropertyName("meshes")]
        public List<Mesh> Meshes { get; set; }

        /// <summary>
        /// An array of nodes.
        /// </summary>
        [JsonPropertyName("nodes")]
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// An array of samplers. A sampler contains properties for texture filtering and wrapping modes.
        /// </summary>
        [JsonPropertyName("samplers")]
        public List<Sampler> Samplers { get; set; }

        /// <summary>
        /// The index of the default scene.
        /// </summary>
        [JsonPropertyName("scene")]
        public int? Scene { get; set; }

        /// <summary>
        /// An array of scenes.
        /// </summary>
        [JsonPropertyName("scenes")]
        public List<Scene> Scenes { get; set; }

        /// <summary>
        /// An array of skins. A skin is defined by joints and matrices.
        /// </summary>
        [JsonPropertyName("skins")]
        public List<Skin> Skins { get; set; }

        /// <summary>
        /// An array of textures.
        /// </summary>
        [JsonPropertyName("textures")]
        public List<Texture> Textures { get; set; }

        /// <inheritdoc cref="RootExtensions"/>
        [JsonPropertyName("extensions")]
        public RootExtensions Extensions { get; set; }

        /// <summary>Application-specific data.</summary>
        /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#reference-extras"/>
        [JsonPropertyName("extras")]
        [JsonConverter(typeof(ExtrasConverter))]
        public ExtrasContainer Extras { get; set; }

        /// <summary>JSON properties without a matching member.</summary>
        [JsonExtensionData, JsonInclude]
        internal Dictionary<string, JsonElement> ExtensionData { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public ReadOnlyProperties AdditionalProperties => new(ExtensionData ?? ReadOnlyProperties.Empty);


#if UNITY_ANIMATION || GLTFAST_ANIMATION
        [JsonIgnore]
        public bool HasAnimation => Animations is { Count: > 0 };
#endif // UNITY_ANIMATION || GLTFAST_ANIMATION

        /// <summary>
        /// Looks up if a certain accessor points to interleaved data.
        /// </summary>
        /// <param name="accessorIndex">Accessor index</param>
        /// <returns>True if accessor is interleaved, false if its data is
        /// continuous.</returns>
        public bool IsAccessorInterleaved(int accessorIndex)
        {
            var accessor = Accessors[accessorIndex];
            var bufferView = BufferViews[accessor.BufferView.Value];
            if (!bufferView.ByteStride.HasValue) return false;
            return bufferView.ByteStride.Value > accessor.ElementByteSize;
        }

        /// <summary>
        /// Number of materials variants.
        /// </summary>
        /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants"/>
        [JsonIgnore]
        public int MaterialsVariantsCount => Extensions?.MaterialsVariants?.Variants?.Count ?? 0;

        /// <summary>
        /// Gets the name of a specific materials variant.
        /// </summary>
        /// <param name="index">Materials variant index.</param>
        /// <returns>Name of a materials variant.</returns>
        /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants"/>
        public string GetMaterialsVariantName(int index)
        {
            var variants = Extensions?.MaterialsVariants?.Variants;
            if (variants != null && index >= 0 && index < variants.Count)
            {
                return variants[index].Name;
            }

            return null;
        }

        /// <summary>
        /// Serialization to JSON
        /// </summary>
        /// <param name="stream"><see cref="StreamWriter"/> the JSON string is being written to.</param>
        /// <seealso cref="RootExtension.Serialize"/>
        [Obsolete("Use RootExtension.Serialize instead")]
        public void GltfSerialize(StreamWriter stream)
        {
            this.Serialize(stream.BaseStream);
        }
    }
}
