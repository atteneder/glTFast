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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {

    public enum ReportCode : uint {
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
        MissingImageURL,
        NamingOverride,
        PackageMissing,
        PrimitiveModeUnsupported,
        SkinMissing,
        TextureLoadFailed,
        TypeUnsupported,
    }

    
    [Serializable]
    public class ReportItem {

        public LogType type = LogType.Error;
        public ReportCode code;
        public string[] messages;
        
        public ReportItem(LogType type, ReportCode code, params string[] messages) {
            this.type = type;
            this.code = code;
            this.messages = messages;
        }

        public void Log() {
            Debug.LogFormat(type, LogOption.NoStacktrace,null,Report.messages[code],messages);
        }

        public override string ToString() {
            return messages != null
                ? string.Format(Report.messages[code], messages)
                : Report.messages[code];
        }
    }

    [Serializable]
    public class Report {

        public static readonly Dictionary<ReportCode, string> messages = new Dictionary<ReportCode, string>() {
            { ReportCode.AccessorAttributeTypeUnknown, "Unknown GLTFAccessorAttributeType" },
            { ReportCode.AccessorInconsistentUsage, "Inconsistent accessor usage {0} != {1}" },
            { ReportCode.AccessorsShared, @"glTF file uses certain vertex attributes/accessors across multiple meshes!
                    This may result in low performance and high memory usage. Try optimizing the glTF file.
                    See details in corresponding issue at https://github.com/atteneder/glTFast/issues/52" },
            { ReportCode.AnimationChannelNodeInvalid, "Animation channel {0} has invalid node id" },
            { ReportCode.AnimationChannelSamplerInvalid, "Animation channel {0} has invalid sampler id" },
            { ReportCode.AnimationFormatInvalid, "Invalid animation format {0}" },
            { ReportCode.AnimationTargetPathUnsupported, "Unsupported animation target path {0}" },
            { ReportCode.BufferLoadFailed, "Download buffer {1} failed: {0}" },
            { ReportCode.BufferMainInvalidType, "Invalid mainBufferType {0}" },
            { ReportCode.ChunkJsonInvalid, "Invalid JSON chunk" },
            { ReportCode.ColorFormatUnsupported, "Unsupported color format {0}" },
            { ReportCode.Download, "Download URL {1} failed: {0}" },
            { ReportCode.EmbedBufferLoadFailed, "Loading embed buffer failed" },
            { ReportCode.EmbedImageInconsistentType, "Inconsistent embed image type {0}!={1}" },
            { ReportCode.EmbedImageLoadFailed, "Loading embedded image failed" },
            { ReportCode.EmbedImageUnsupportedType, "Unsupported embed image format {0}" },
            { ReportCode.EmbedSlow, "JSON embed buffers are slow! consider using glTF binary" },
            { ReportCode.ExtensionUnsupported, "glTF extension {0} is not supported" },
            { ReportCode.GltfNotBinary, "Not a glTF-binary file" },
            { ReportCode.GltfUnsupportedVersion, "Unsupported glTF version {0}" },
            { ReportCode.HierarchyInvalid, "Invalid hierarchy" },
            { ReportCode.ImageFormatUnknown, "Unknown image format (image {0};uri:{1})" },
            { ReportCode.ImageMultipleSamplers, "Have to create copy of image {0} due to different samplers. This is harmless, but requires more memory." },
            { ReportCode.IndexFormatInvalid, "Invalid index format {0}" },
            { ReportCode.MissingImageURL, "Image URL missing" },
            { ReportCode.NamingOverride, "Overriding naming method to be OriginalUnique (animation requirement)" },
            { ReportCode.PackageMissing, "{0} package needs to be installed in order to support glTF extension {1}!\nSee https://github.com/atteneder/glTFast#installing for instructions" },
            { ReportCode.PrimitiveModeUnsupported, "Primitive mode {0} is untested" },
            { ReportCode.SkinMissing, "Skin missing" },
            { ReportCode.TextureLoadFailed, "Download texture {1} failed: {0}" },
            { ReportCode.TypeUnsupported, "Unsupported {0} type {1}" },
        };
        
        public List<ReportItem> items = new List<ReportItem>();

        public void Error(ReportCode code, params string[] messages) {
            items.Add(new ReportItem(LogType.Error, code, messages));
        }
        
        public void Warning(ReportCode code, params string[] messages) {
            items.Add(new ReportItem(LogType.Warning, code, messages));
        }
        
        public void Info(ReportCode code, params string[] messages) {
            items.Add(new ReportItem(LogType.Log, code, messages));
        }
        
        public void LogAll() {
            foreach (var item in items) {
                item.Log();
            }
        }
    }
}

