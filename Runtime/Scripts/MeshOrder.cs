// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Unity.Cloud.Gltfast
{
    readonly struct MeshOrder : IDisposable
    {
        public readonly MeshGeneratorBase generator;
        readonly List<MeshSubset> m_Recipients;

        public MeshOrder(MeshGeneratorBase generator)
        {
            this.generator = generator;
            m_Recipients = new List<MeshSubset>();
        }

        public void AddRecipient(MeshSubset subset) => m_Recipients.Add(subset);

        public IReadOnlyList<MeshSubset> Recipients => m_Recipients;

        public void Dispose()
        {
            generator?.Dispose();
        }
    }
}
