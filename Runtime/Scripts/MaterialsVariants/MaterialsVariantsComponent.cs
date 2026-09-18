// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Wraps a <see cref="MaterialsVariantsControl"/> and provides access to it.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public class MaterialsVariantsComponent : MonoBehaviour
    {
        /// <summary>
        /// Materials variants control instance.
        /// </summary>
        public MaterialsVariantsControl Control { get; set; }
    }
}
