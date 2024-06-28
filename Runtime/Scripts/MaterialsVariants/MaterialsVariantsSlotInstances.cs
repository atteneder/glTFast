// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast
{
    readonly struct MaterialsVariantsSlotInstances : IMaterialsVariantsSlotInstance
    {
        readonly Renderer m_Renderer;

        readonly IMaterialsVariantsSlot[] m_Slots;

        public MaterialsVariantsSlotInstances(Renderer renderer, IMaterialsVariantsSlot[] slots)
        {
            m_Renderer = renderer;
            m_Slots = slots;
        }

        public async Task ApplyMaterialsVariantAsync(
            int variantIndex,
            IMaterialProvider materialProvider,
            List<Material> materials,
            CancellationToken cancellationToken
            )
        {
            m_Renderer.GetSharedMaterials(materials);
            Dictionary<Task<Material>, int> getMaterialTasks = null;
            Task<Material> getDefaultMaterialTask = null;
            for (var subMesh = 0; subMesh < m_Slots.Length; subMesh++)
            {
                var slot = m_Slots[subMesh];
                var materialId = slot.GetMaterialIndex(variantIndex);
                materials[subMesh] = null;
                Task<Material> task;
                if (materialId < 0)
                {
                    getDefaultMaterialTask ??= materialProvider.GetDefaultMaterialAsync(cancellationToken);
                    task = getDefaultMaterialTask;
                }
                else
                {
                    task = materialProvider.GetMaterialAsync(materialId, cancellationToken);
                }
                getMaterialTasks ??= new Dictionary<Task<Material>, int>();
                getMaterialTasks[task] = subMesh;
            }

            if (getMaterialTasks != null)
            {
                while (getMaterialTasks.Count > 0)
                {
                    var task = await Task.WhenAny(getMaterialTasks.Keys);
                    materials[getMaterialTasks[task]] = task.Result;
                    getMaterialTasks.Remove(task);
                }
            }
#if UNITY_2022_2_OR_NEWER
            m_Renderer.SetSharedMaterials(materials);
#else
            m_Renderer.sharedMaterials = materials.ToArray();
#endif
        }
    }
}
