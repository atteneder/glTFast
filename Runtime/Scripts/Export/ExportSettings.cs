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

using UnityEngine;

namespace GLTFast.Export
{

    /// <summary>
    /// glTF format
    /// </summary>
    public enum GltfFormat
    {
        /// <summary>
        /// JSON-based glTF (.gltf file extension)
        /// </summary>
        Json,
        /// <summary>
        /// glTF-binary (.glb file extension)
        /// </summary>
        Binary
    }

    /// <summary>
    /// Destination for image files
    /// </summary>
    public enum ImageDestination
    {
        /// <summary>
        /// Automatic decision. Main buffer for glTF-binary, separate files for JSON-based glTFs.
        /// </summary>
        Automatic,
        /// <summary>
        /// Embeds images in main buffer
        /// </summary>
        MainBuffer,
        /// <summary>
        /// Saves images as separate files relative to glTF file
        /// </summary>
        SeparateFile
    }

    /// <summary>
    /// Resolutions to existing file conflicts
    /// </summary>
    public enum FileConflictResolution
    {
        /// <summary>
        /// Abort and keep existing files
        /// </summary>
        Abort,
        /// <summary>
        /// Replace existing files with newly created ones
        /// </summary>
        Overwrite
    }

    /// <summary>
    /// glTF export settings
    /// </summary>
    public class ExportSettings
    {
        /// <summary>
        /// Export to JSON-based or binary format glTF files
        /// </summary>
        public GltfFormat Format { get; set; } = GltfFormat.Json;

        /// <inheritdoc cref="Export.ImageDestination"/>
        public ImageDestination ImageDestination { get; set; } = ImageDestination.Automatic;

        /// <inheritdoc cref="Export.FileConflictResolution"/>
        public FileConflictResolution FileConflictResolution { get; set; } = FileConflictResolution.Abort;

        /// <summary>
        /// Light intensity values are multiplied by this factor.
        /// </summary>
        [field: Tooltip("Light intensity values are multiplied by this factor")]
        public float LightIntensityFactor { get; set; } = 1.0f;

        /// <summary>
        /// Component type flags to include or exclude components from export
        /// based on type.
        /// </summary>
        public ComponentType ComponentMask { get; set; } = ComponentType.All;
    }
}
