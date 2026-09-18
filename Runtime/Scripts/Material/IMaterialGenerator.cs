// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Materials
{
    using Logging;
    using Objects;

    /// <summary>
    /// Provides a mechanism to convert glTF materials into Unity Materials
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Materials", sourceAssembly: "glTFast")]
    public interface IMaterialGenerator
    {

        /// <summary>
        /// Get fallback material that is assigned to nodes without a material.
        /// </summary>
        /// <param name="pointsSupport">If true, material has to support meshes with <see cref="MeshTopology.Points">points</see> topology.</param>
        /// <returns>fallback material</returns>
        UnityEngine.Material GetDefaultMaterial(bool pointsSupport = false);

        /// <summary>
        /// Converts a glTF material into a Unity <see cref="Material"/>.
        /// <paramref name="gltfMaterial"/> might reference textures, which can be queried from <paramref name="gltf"/>.
        /// </summary>
        /// <param name="gltfMaterial">Source glTF material</param>
        /// <param name="gltf">Interface to a loaded glTF's resources (e.g. textures)</param>
        /// <param name="pointsSupport">If true, material has to support meshes with <see cref="MeshTopology.Points">points</see> topology.</param>
        /// <returns>Generated Unity Material</returns>
        UnityEngine.Material GenerateMaterial(
            Material gltfMaterial,
            IGltfReadable gltf,
            bool pointsSupport = false
            );

        /// <summary>
        /// Has to be called prior to <see cref="GenerateMaterial"/>. The logger can be used
        /// to inform users about incidents of arbitrary severity (error,warning or info)
        /// during material generation.
        /// </summary>
        /// <param name="logger">Logger to be used.</param>
        void SetLogger(ICodeLogger logger);
    }
}
