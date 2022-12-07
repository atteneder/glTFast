// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using UnityEngine;

namespace GLTFast.Export
{
    readonly struct MeshMaterialCombination
    {
        readonly int m_MeshId;
        readonly int[] m_MaterialIds;

        public MeshMaterialCombination(int meshId, int[] materialIds)
        {
            m_MeshId = meshId;
            m_MaterialIds = materialIds;
        }

        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((MeshMaterialCombination)obj);
        }

        bool Equals(MeshMaterialCombination other)
        {
            return m_MeshId == other.m_MeshId && Equals(m_MaterialIds, other.m_MaterialIds);
        }

        static bool Equals(int[] a, int[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null ^ b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
#if NET_STANDARD
            var hash = new HashCode();
            hash.Add(m_MeshId);
            if (m_MaterialIds != null) {
                foreach (var id in m_MaterialIds) {
                    hash.Add(id);
                }
            }
            return hash.ToHashCode();
#else
            var hash = 17;
            hash = hash * 31 + m_MeshId.GetHashCode();
            if (m_MaterialIds != null)
            {
                hash = hash * 31 + m_MaterialIds.Length;
                foreach (var id in m_MaterialIds)
                {
                    hash = hash * 31 + id;
                }
            }
            return hash;
#endif
        }
    }
}
