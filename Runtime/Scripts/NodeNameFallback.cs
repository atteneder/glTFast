// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace Unity.Cloud.Gltfast
{
    sealed class NodeNameFallback
    {
        HashSet<uint> m_UnnamedNodes;

        // Concatenation, not interpolation: single-argument interpolation lowers to string.Format(string, object)
        // on this profile, which boxes the index.
        internal static string DefaultName(uint nodeIndex) => "Node-" + nodeIndex.ToString();

        internal void MarkUnnamed(uint nodeIndex)
        {
            m_UnnamedNodes ??= new HashSet<uint>();
            m_UnnamedNodes.Add(nodeIndex);
        }

        internal bool TryTake(uint nodeIndex, MeshResult meshResult, out string meshName)
        {
            meshName = null;
            if (m_UnnamedNodes == null || !m_UnnamedNodes.Contains(nodeIndex))
            {
                return false;
            }

            var name = meshResult.mesh == null ? null : meshResult.mesh.name;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            meshName = name;
            m_UnnamedNodes.Remove(nodeIndex);
            return true;
        }

        internal void Release()
        {
            m_UnnamedNodes = null;
        }
    }
}
