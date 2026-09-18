// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast
{
    class NodeHierarchyInfo : INodeHierarchyInfo
    {
        readonly string[] m_NodeNames;
        readonly int[] m_ParentIndices;

        public NodeHierarchyInfo(string[] nodeNames, int[] parentIndices)
        {
            m_ParentIndices = parentIndices;
            m_NodeNames = nodeNames;
        }

        public string GetNodeName(int nodeIndex)
        {
            return m_NodeNames[nodeIndex];
        }

        public int GetParentIndex(int nodeIndex)
        {
            return m_ParentIndices[nodeIndex];
        }
    }
}
