// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#if DEBUG
#define GLTFAST_REPORT
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace GLTFast.Logging
{

    /// <summary>
    /// Predefined message code
    /// </summary>
    public enum LogCode : uint
    {
        /// <summary>
        /// No or unknown log code.
        /// </summary>
        None,
        /// <summary>
        /// Unknown GLTFAccessorAttributeType
        /// </summary>
        AccessorAttributeTypeUnknown,
        /// <summary>
        /// Inconsistent accessor usage
        /// </summary>
        AccessorInconsistentUsage,
        /// <summary>
        /// glTF file uses certain vertex attributes/accessors across multiple
        /// meshes! This may result in low performance and high memory usage.
        /// Try optimizing the glTF file.
        /// </summary>
        AccessorsShared,
        /// <summary>
        /// Animation channel has invalid node id
        /// </summary>
        AnimationChannelNodeInvalid,
        /// <summary>
        /// Animation channel has invalid sampler id
        /// </summary>
        AnimationChannelSamplerInvalid,
        /// <summary>
        /// Invalid animation format
        /// </summary>
        AnimationFormatInvalid,
        /// <summary>
        /// Unsupported animation target path
        /// </summary>
        AnimationTargetPathUnsupported,
        /// <summary>
        /// Download buffer failed
        /// </summary>
        BufferLoadFailed,
        /// <summary>
        /// Invalid main buffer type
        /// </summary>
        BufferMainInvalidType,
        /// <summary>
        /// Invalid JSON chunk
        /// </summary>
        ChunkJsonInvalid,
        /// <summary>
        /// Incomplete chunk
        /// </summary>
        ChunkIncomplete,
        /// <summary>
        /// Unknown chunk type
        /// </summary>
        ChunkUnknown,
        /// <summary>
        /// Unsupported color format
        /// </summary>
        ColorFormatUnsupported,
        /// <summary>
        /// Download failed
        /// </summary>
        Download,
        /// <summary>
        /// Loading embed buffer failed
        /// </summary>
        EmbedBufferLoadFailed,
        /// <summary>
        /// Inconsistent embed image type
        /// </summary>
        EmbedImageInconsistentType,
        /// <summary>
        /// Loading embedded image failed
        /// </summary>
        EmbedImageLoadFailed,
        /// <summary>
        /// Unsupported embed image format
        /// </summary>
        EmbedImageUnsupportedType,
        /// <summary>
        /// JSON embed buffers are slow! consider using glTF binary
        /// </summary>
        EmbedSlow,
        /// <summary>
        /// glTF extension is not supported
        /// </summary>
        ExtensionUnsupported,
        /// <summary>
        /// Image could not get exported
        /// </summary>
        ExportImageFailed,
        /// <summary>
        /// Not a glTF-binary file
        /// </summary>
        GltfNotBinary,
        /// <summary>
        /// Unsupported glTF version
        /// </summary>
        GltfUnsupportedVersion,
        /// <summary>
        /// Invalid hierarchy
        /// </summary>
        HierarchyInvalid,
        /// <summary>
        /// Support for Jpeg/PNG texture decoding/encoding is not enabled
        /// </summary>
        ImageConversionNotEnabled,
        /// <summary>
        /// Unknown image format
        /// </summary>
        ImageFormatUnknown,
        /// <summary>
        /// Have to create copy of image {0} due to different samplers. This is harmless, but requires more memory.
        /// </summary>
        ImageMultipleSamplers,
        /// <summary>
        /// Invalid index format
        /// </summary>
        IndexFormatInvalid,
        /// <summary>
        /// Parsing JSON failed
        /// </summary>
        JsonParsingFailed,
        /// <summary>
        /// Chance of incorrect materials! glTF transmission is approximated
        /// when using built-in render pipeline!
        /// </summary>
        MaterialTransmissionApprox,
        /// <summary>
        /// Chance of incorrect materials! glTF transmission is approximated.
        /// Enable Opaque Texture access in Universal Render Pipeline!
        /// </summary>
        MaterialTransmissionApproxUrp,
        /// <summary>
        /// No bounds for mesh {0} => calculating them.
        /// </summary>
        MeshBoundsMissing,
        /// <summary>
        /// Skipping non-readable mesh
        /// </summary>
        MeshNotReadable,
        /// <summary>
        /// Image URL missing
        /// </summary>
        MissingImageURL,
        /// <summary>
        /// Retrieving morph target failed
        /// </summary>
        MorphTargetContextFail,
        /// <summary>
        /// Overriding naming method to be OriginalUnique (animation requirement)
        /// </summary>
        NamingOverride,
        /// <summary>
        /// A certain package needs to be installed in order to support a specific glTF extension
        /// </summary>
        PackageMissing,
        /// <summary>
        /// Primitive mode is untested
        /// </summary>
        PrimitiveModeUnsupported,
        /// <summary>
        /// Remap is not fully supported
        /// </summary>
        RemapUnsupported,
        /// <summary>
        /// Shader is missing. Make sure to include it in the build.
        /// </summary>
        ShaderMissing,
        /// <summary>
        /// Skin missing
        /// </summary>
        SkinMissing,
        /// <summary>
        /// Sparse Accessor not supported
        /// </summary>
        SparseAccessor,
        /// <summary>
        /// Download texture failed
        /// </summary>
        TextureDownloadFailed,
        /// <summary>
        /// Invalid texture type
        /// </summary>
        TextureInvalidType,
        /// <summary>
        /// Texture not loaded
        /// </summary>
        TextureLoadFailed,
        /// <summary>
        /// Texture not found
        /// </summary>
        TextureNotFound,
        /// <summary>
        /// Could not find material that supports points topology
        /// </summary>
        TopologyPointsMaterialUnsupported,
        /// <summary>
        /// Unsupported topology
        /// </summary>
        TopologyUnsupported,
        /// <summary>
        /// Unsupported type
        /// </summary>
        TypeUnsupported,
        /// <summary>
        /// Support for direct Jpeg/PNG texture download is not enabled
        /// </summary>
        UnityWebRequestTextureNotEnabled,
        /// <summary>
        /// Only eight UV sets will get imported
        /// </summary>
        UVLimit,
        /// <summary>
        /// UV set index is not supported in current render pipeline
        /// </summary>
        UVMulti,
    }

    /// <summary>
    /// Converts <seealso cref="LogCode"/> to human readable and understandable message string.
    /// </summary>
    public static class LogMessages
    {
#if GLTFAST_REPORT
        static readonly string k_LinkProjectSetupTextureSupport = $"See {GltfGlobals.GltfPackageName}/Documentation~/ProjectSetup.md#texture-support for details.";

        static readonly Dictionary<LogCode, string> k_FullMessages = new Dictionary<LogCode, string>() {
            { LogCode.AccessorAttributeTypeUnknown, "Unknown GLTFAccessorAttributeType" },
            { LogCode.AccessorInconsistentUsage, "Inconsistent accessor usage {0} != {1}" },
            { LogCode.AccessorsShared, @"glTF file uses certain vertex attributes/accessors across multiple meshes!
This may result in low performance and high memory usage. Try optimizing the glTF file.
See details in corresponding issue at https://github.com/atteneder/glTFast/issues/52" },
            { LogCode.AnimationChannelNodeInvalid, "Animation channel {0} has invalid node id" },
            { LogCode.AnimationChannelSamplerInvalid, "Animation channel {0} has invalid sampler id" },
            { LogCode.AnimationFormatInvalid, "Invalid animation format {0}" },
            { LogCode.AnimationTargetPathUnsupported, "Unsupported animation target path {0}" },
            { LogCode.BufferLoadFailed, "Download buffer {1} failed: {0}" },
            { LogCode.BufferMainInvalidType, "Invalid mainBufferType {0}" },
            { LogCode.ChunkJsonInvalid, "Invalid JSON chunk" },
            { LogCode.ChunkIncomplete, "Incomplete chunk" },
            { LogCode.ChunkUnknown, "Unknown chunk type {0}" },
            { LogCode.ColorFormatUnsupported, "Unsupported color format {0}" },
            { LogCode.Download, "Download URL {1} failed: {0}" },
            { LogCode.EmbedBufferLoadFailed, "Loading embed buffer failed" },
            { LogCode.EmbedImageInconsistentType, "Inconsistent embed image type {0}!={1}" },
            { LogCode.EmbedImageLoadFailed, "Loading embedded image failed" },
            { LogCode.EmbedImageUnsupportedType, "Unsupported embed image format {0}" },
            { LogCode.EmbedSlow, "JSON embed buffers are slow! consider using glTF binary" },
            { LogCode.ExtensionUnsupported, "glTF extension {0} is not supported" },
            { LogCode.ExportImageFailed, "Export image failed" },
            { LogCode.GltfNotBinary, "Not a glTF-binary file" },
            { LogCode.GltfUnsupportedVersion, "Unsupported glTF version {0}" },
            { LogCode.HierarchyInvalid, "Invalid hierarchy" },
            { LogCode.ImageConversionNotEnabled, $"Jpeg/PNG textures failed because required built-in packages \"Image Conversion\"/\"Unity Web Request Texture\" are not enabled. {k_LinkProjectSetupTextureSupport}" },
            { LogCode.ImageFormatUnknown, "Unknown image format (image {0};uri:{1})" },
            { LogCode.ImageMultipleSamplers, "Have to create copy of image {0} due to different samplers. This is harmless, but requires more memory." },
            { LogCode.IndexFormatInvalid, "Invalid index format {0}" },
            { LogCode.JsonParsingFailed, "Parsing JSON failed" },
            { LogCode.MaterialTransmissionApprox, "Chance of incorrect materials! glTF transmission is approximated when using built-in render pipeline!" },
            { LogCode.MaterialTransmissionApproxUrp, @"Chance of incorrect materials! glTF transmission
is approximated. Enable Opaque Texture access in Universal Render Pipeline!" },
            { LogCode.MeshBoundsMissing, "No bounds for mesh {0} => calculating them." },
            { LogCode.MeshNotReadable, "Skipping non-readable mesh {0}" },
            { LogCode.MissingImageURL, "Image URL missing" },
            { LogCode.MorphTargetContextFail, "Retrieving morph target failed" },
            { LogCode.NamingOverride, "Overriding naming method to be OriginalUnique (animation requirement)" },
            { LogCode.PackageMissing, $"{{0}} package needs to be installed in order to support glTF extension {{1}}!\nSee {GltfGlobals.GltfPackageName}/README.md#installing for instructions" },
            { LogCode.PrimitiveModeUnsupported, "Primitive mode {0} is untested" },
            { LogCode.RemapUnsupported, "{0} remap is not fully supported" },
            { LogCode.ShaderMissing, $"Shader \"{{0}}\" is missing. Make sure to include it in the build (see {GltfGlobals.GltfPackageName}/Documentation~/ProjectSetup.md#materials-and-shader-variants )" },
            { LogCode.SkinMissing, "Skin missing" },
            { LogCode.SparseAccessor, "Sparse Accessor not supported ({0})" },
            { LogCode.TextureDownloadFailed, "Download texture {1} failed: {0}" },
            { LogCode.TextureInvalidType, "Invalid {0} texture type (material: {1})" },
            { LogCode.TextureLoadFailed, "Texture #{0} not loaded" },
            { LogCode.TextureNotFound, "Texture #{0} not found" },
            { LogCode.TopologyPointsMaterialUnsupported, "Could not find material that supports points topology" },
            { LogCode.TopologyUnsupported, "Unsupported topology {0}" },
            { LogCode.TypeUnsupported, "Unsupported {0} type {1}" },
            { LogCode.UnityWebRequestTextureNotEnabled, $"PNG/Jpeg textures load slower because built-in package \"Unity Web Request Texture\" is not enabled. {k_LinkProjectSetupTextureSupport}" },
            { LogCode.UVLimit, "Only eight UV sets will get imported" },
            { LogCode.UVMulti, "UV set index {0} is not supported in current render pipeline" },
        };
#endif

        /// <summary>
        /// Converts a <seealso cref="LogCode"/> to human readable and understandable message string.
        /// </summary>
        /// <param name="code">Input LogCode</param>
        /// <param name="messages">Additional message parts (te be filled into final message)</param>
        /// <returns>Human readable and understandable message string</returns>
        public static string GetFullMessage(LogCode code, params string[] messages)
        {
            if (code == LogCode.None)
            {
                var sb = new StringBuilder();
                foreach (var message in messages)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(";");
                    }
                    sb.Append(message);
                }
                return sb.ToString();
            }
#if GLTFAST_REPORT
            return messages != null
                // ReSharper disable once CoVariantArrayConversion
                ? string.Format(k_FullMessages[code], messages)
                : k_FullMessages[code];
#else
            if (messages == null)
            {
                return code.ToString();
            }
            else
            {
                var sb = new StringBuilder(code.ToString());
                foreach (var message in messages)
                {
                    sb.Append(";");
                    sb.Append(message);
                }
                return sb.ToString();
            }
#endif
        }
    }
}
