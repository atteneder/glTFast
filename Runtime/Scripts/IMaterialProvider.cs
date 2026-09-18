// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Scripting.APIUpdating;
using Material = UnityEngine.Material;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Provides access to glTF materials.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public interface IMaterialProvider : IMaterialsVariantsProvider
    {
        /// <inheritdoc cref="IGltfReadable.GetMaterial"/>
        Task<Material> GetMaterialAsync(int index);

        /// <inheritdoc cref="GetMaterialAsync(int)"/>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        Task<Material> GetMaterialAsync(int index, CancellationToken cancellationToken);

        /// <inheritdoc cref="IGltfReadable.GetDefaultMaterial"/>
        Task<Material> GetDefaultMaterialAsync();

        /// <inheritdoc cref="GetDefaultMaterialAsync()"/>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        Task<Material> GetDefaultMaterialAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Returns the material slots that correspond to the given MeshResult's sub-meshes.
        /// </summary>
        /// <param name="meshIndex">glTF mesh index.</param>
        /// <param name="meshNumeration">Per glTF mesh <see cref="MeshResult"/> numeration. A glTF mesh is converted
        /// into one or more MeshResults which are numbered consecutively.</param>
        /// <returns>Corresponding materials variants slots.</returns>
        /// <seealso cref="IMaterialsVariantsSlot"/>
        /// <seealso cref="MeshResult"/>
        IMaterialsVariantsSlot[] GetMaterialsVariantsSlots(int meshIndex, int meshNumeration);
    }
}
