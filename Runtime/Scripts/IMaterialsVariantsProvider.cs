// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Objects;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Provides access to glTF materials variants.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public interface IMaterialsVariantsProvider
    {
        /// <inheritdoc cref="Root.MaterialsVariantsCount"/>
        int MaterialsVariantsCount { get; }

        /// <inheritdoc cref="Root.GetMaterialsVariantName"/>
        string GetMaterialsVariantName(int index);
    }
}
