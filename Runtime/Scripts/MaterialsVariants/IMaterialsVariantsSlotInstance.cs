// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast
{
    interface IMaterialsVariantsSlotInstance
    {
        Task ApplyMaterialsVariantAsync(
            int variantIndex,
            IMaterialProvider materialProvider,
            List<Material> materials,
            CancellationToken cancellationToken
            );
    }
}
