// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast.Export
{

    using Logging;

    /// <summary>
    /// Converts a Unity Material into a glTF material
    /// </summary>
    public interface IMaterialExport
    {

        /// <summary>
        /// Converts a Unity material to a glTF material.
        /// </summary>
        /// <param name="uMaterial">Source material</param>
        /// <param name="material">Resulting material</param>
        /// <param name="gltf">Associated IGltfWriter. Is used for adding images and textures.</param>
        /// <param name="logger">Logger used for reporting</param>
        /// <returns>True if no errors occured, false otherwise</returns>
        bool ConvertMaterial(
            Material uMaterial,
            out Schema.Material material,
            IGltfWritable gltf,
            ICodeLogger logger
            );
    }
}
