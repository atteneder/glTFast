// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast.Tests
{
    class MaterialProviderMock : IMaterialProvider
    {
        public Material GetMaterial(int index = 0)
        {
            return new Material(Shader.Find("Standard"))
            {
                name = index.ToString()
            };
        }

        public Task<Material> GetMaterialAsync(int index)
        {
            return Task.FromResult(GetMaterial(index));
        }

        public Task<Material> GetMaterialAsync(int index, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetMaterial(index));
        }

        public Task<Material> GetDefaultMaterialAsync()
        {
            return Task.FromResult(GetDefaultMaterial());
        }

        public Task<Material> GetDefaultMaterialAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(GetDefaultMaterial());
        }

        public Material GetDefaultMaterial()
        {
            return new Material(Shader.Find("Standard"))
            {
                name = "Default"
            };
        }

        public int MaterialsVariantsCount => 6;

        public string GetMaterialsVariantName(int index)
        {
            return $"Variant {index}";
        }

        public IMaterialsVariantsSlot[] GetMaterialsVariantsSlots(int meshIndex, int meshResultOffset)
        {
            throw new System.NotImplementedException();
        }
    }
}
