// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast
{
    readonly struct MultiMaterialsVariantsSlotInstances : IMaterialsVariantsSlotInstance
    {
        readonly IEnumerable<Renderer> m_Renderers;

        readonly IReadOnlyList<IMaterialsVariantsSlot> m_Slots;

        public MultiMaterialsVariantsSlotInstances(IEnumerable<Renderer> renderers, IReadOnlyList<IMaterialsVariantsSlot> slots)
        {
            m_Renderers = renderers;
            m_Slots = slots;
        }

        public async Task ApplyMaterialsVariantAsync(
            int variantIndex,
            IMaterialProvider materialProvider,
            List<Material> materials,
            CancellationToken cancellationToken
            )
        {
            var firstIteration = true;
#if !UNITY_2022_2_OR_NEWER
            Material[] materialsArray = null;
#endif
            foreach (var renderer in m_Renderers)
            {
                if (firstIteration)
                {
                    renderer.GetSharedMaterials(materials);
                    Dictionary<Task<Material>, int> getMaterialTasks = null;
                    Task<Material> getDefaultMaterialTask = null;
                    for (var subMesh = 0; subMesh < m_Slots.Count; subMesh++)
                    {
                        var slot = m_Slots[subMesh];
                        var materialId = slot.GetMaterialIndex(variantIndex);
                        Task<Material> task;
                        materials[subMesh] = null;
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

#if !UNITY_2022_2_OR_NEWER
                    materialsArray = materials.ToArray();
#endif
                    firstIteration = false;
                }
#if UNITY_2022_2_OR_NEWER
                renderer.SetSharedMaterials(materials);
#else
                renderer.sharedMaterials = materialsArray;
#endif
            }
        }
    }
}
