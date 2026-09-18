// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Represents a material slot.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public interface IMaterialsVariantsSlot
    {
        /// <summary>
        /// Provides the glTF material index, given a materials variant index.
        /// If variantIndex is invalid (e.g. negative) or the slot does not have a material override for the given
        /// variantIndex, it returns the default material index.
        /// </summary>
        /// <param name="variantIndex">Materials variant index.</param>
        /// <returns>Corresponding glTF material index, or null when no material is assigned.</returns>
        int? GetMaterialIndex(int variantIndex);
    }
}
