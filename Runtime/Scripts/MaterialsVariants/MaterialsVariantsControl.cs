// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast
{
    /// <summary>
    /// Allows switching the materials variant of a glTF scene instance.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_variants">KHR_materials_variants extension</seealso>
    public class MaterialsVariantsControl : IMaterialsVariantsProvider
    {
        IMaterialProvider m_MaterialProvider;
        IReadOnlyCollection<IMaterialsVariantsSlotInstance> m_Slots;

        int m_CurrentVariantIndex;

        internal MaterialsVariantsControl(IMaterialProvider materialProvider, IReadOnlyCollection<IMaterialsVariantsSlotInstance> slots)
        {
            m_MaterialProvider = materialProvider;
            m_Slots = slots;
        }

        /// <summary>
        /// Applies a materials variant.
        /// </summary>
        /// <param name="variantIndex">glTF materials variant index.</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the completion of the method.</returns>
        public async Task ApplyMaterialsVariantAsync(int variantIndex, CancellationToken cancellationToken = default)
        {
            var materials = new List<Material>();
            var tasks = new List<Task>();
            foreach (var instanceSlot in m_Slots)
            {
                tasks.Add(instanceSlot.ApplyMaterialsVariantAsync(variantIndex, m_MaterialProvider, materials, cancellationToken));
            }

            await Task.WhenAll(tasks);
            m_CurrentVariantIndex = variantIndex;
        }

        /// <inheritdoc cref="IMaterialProvider.MaterialsVariantsCount"/>
        public int MaterialsVariantsCount => m_MaterialProvider.MaterialsVariantsCount;

        /// <inheritdoc cref="IMaterialProvider.GetMaterialsVariantName"/>
        public string GetMaterialsVariantName(int index)
        {
            return m_MaterialProvider.GetMaterialsVariantName(index);
        }
    }
}
