// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast
{
    /// <summary>
    /// Wraps a <see cref="MaterialsVariantsControl"/> and provides access to it.
    /// </summary>
    public class MaterialsVariantsComponent : MonoBehaviour
    {
        /// <summary>
        /// Materials variants control instance.
        /// </summary>
        [field: SerializeField]
        public MaterialsVariantsControl Control { get; set; }
    }
}
