// Copyright 2020-2021 Andreas Atteneder
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

namespace GLTFast {

    public enum LogCode : uint {
        None,
        AccessorAttributeTypeUnknown,
        AccessorInconsistentUsage,
        AccessorsShared,
        AnimationChannelNodeInvalid,
        AnimationChannelSamplerInvalid,
        AnimationFormatInvalid,
        AnimationTargetPathUnsupported,
        BufferLoadFailed,
        BufferMainInvalidType,
        ChunkJsonInvalid,
        ColorFormatUnsupported,
        Download,
        EmbedBufferLoadFailed,
        EmbedImageInconsistentType,
        EmbedImageLoadFailed,
        EmbedImageUnsupportedType,
        EmbedSlow,
        ExtensionUnsupported,
        GltfNotBinary,
        GltfUnsupportedVersion,
        HierarchyInvalid,
        ImageFormatUnknown,
        ImageMultipleSamplers ,
        IndexFormatInvalid,
        MaterialTransmissionApprox,
        MaterialTransmissionApproxURP,
        MeshBoundsMissing,
        MeshNotReadable,
        MissingImageURL,
        MorphTargetContextFail,
        NamingOverride,
        PackageMissing,
        PrimitiveModeUnsupported,
        ShaderMissing,
        SkinMissing,
        SparseAccessor,
        TextureDownloadFailed,
        TextureInvalidType,
        TextureLoadFailed,
        TextureNotFound,
        TopologyUnsupported,
        TypeUnsupported,
        UVMulti,
    }
    
    public static class LogMessages {
#if GLTFAST_REPORT
        public static readonly Dictionary<LogCode, string> fullMessages = new Dictionary<LogCode, string>() {
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
            { LogCode.ColorFormatUnsupported, "Unsupported color format {0}" },
            { LogCode.Download, "Download URL {1} failed: {0}" },
            { LogCode.EmbedBufferLoadFailed, "Loading embed buffer failed" },
            { LogCode.EmbedImageInconsistentType, "Inconsistent embed image type {0}!={1}" },
            { LogCode.EmbedImageLoadFailed, "Loading embedded image failed" },
            { LogCode.EmbedImageUnsupportedType, "Unsupported embed image format {0}" },
            { LogCode.EmbedSlow, "JSON embed buffers are slow! consider using glTF binary" },
            { LogCode.ExtensionUnsupported, "glTF extension {0} is not supported" },
            { LogCode.GltfNotBinary, "Not a glTF-binary file" },
            { LogCode.GltfUnsupportedVersion, "Unsupported glTF version {0}" },
            { LogCode.HierarchyInvalid, "Invalid hierarchy" },
            { LogCode.ImageFormatUnknown, "Unknown image format (image {0};uri:{1})" },
            { LogCode.ImageMultipleSamplers, "Have to create copy of image {0} due to different samplers. This is harmless, but requires more memory." },
            { LogCode.IndexFormatInvalid, "Invalid index format {0}" },
            { LogCode.MaterialTransmissionApprox, "Chance of incorrect materials! glTF transmission is approximated when using built-in render pipeline!" },
            { LogCode.MaterialTransmissionApproxURP, "Chance of incorrect materials! glTF transmission"
                + " is approximated. Enable Opaque Texture access in Universal Render Pipeline!" },
            { LogCode.MeshBoundsMissing, "No bounds for mesh {0} => calculating them." },
            { LogCode.MeshNotReadable, "Skipping non-readable mesh {0}" },
            { LogCode.MissingImageURL, "Image URL missing" },
            { LogCode.MorphTargetContextFail, "Retrieving morph target failed" },
            { LogCode.NamingOverride, "Overriding naming method to be OriginalUnique (animation requirement)" },
            { LogCode.PackageMissing, "{0} package needs to be installed in order to support glTF extension {1}!\nSee https://github.com/atteneder/glTFast#installing for instructions" },
            { LogCode.PrimitiveModeUnsupported, "Primitive mode {0} is untested" },
            { LogCode.ShaderMissing, "Shader \"{0}\" is missing. Make sure to include it in the build (see https://github.com/atteneder/glTFast/blob/main/Documentation%7E/glTFast.md#materials-and-shader-variants )" },
            { LogCode.SkinMissing, "Skin missing" },
            { LogCode.SparseAccessor, "Sparse Accessor not supported ({0})" },
            { LogCode.TextureDownloadFailed, "Download texture {1} failed: {0}" },
            { LogCode.TextureInvalidType, "Invalid {0} texture type (material: {1})" },
            { LogCode.TextureLoadFailed, "Texture #{0} not loaded" },
            { LogCode.TextureNotFound, "Texture #{0} not found" },
            { LogCode.TopologyUnsupported, "Unsupported topology {0}" },
            { LogCode.TypeUnsupported, "Unsupported {0} type {1}" },
            { LogCode.UVMulti, "UV set index {0} is not supported in current render pipeline!" },
        };
#endif

        public static string GetFullMessage(LogCode code, params string[] messages) {
            if (code == LogCode.None) {
                var sb = new StringBuilder();
                foreach (var message in messages) {
                    if (sb.Length > 0) {
                        sb.Append(";");
                    }
                    sb.Append(message);
                }
                return sb.ToString();
            }
#if GLTFAST_REPORT
            return messages != null
                ? string.Format(fullMessages[code], messages)
                : fullMessages[code];
#else
            if (messages == null) {
                return code.ToString();
            } else {
                var sb = new StringBuilder(code.ToString());
                foreach (var message in messages) {
                    sb.Append(";");
                    sb.Append(message);
                }
                return sb.ToString();
            }
#endif
        }
    }
}

