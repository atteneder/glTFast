// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Animation related utility methods.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public static class AnimationUtils
    {
        /// <summary>
        /// Creates an animation path compatible with the animation module.
        /// </summary>
        /// <param name="nodeIndex">Index of the target node.</param>
        /// <param name="nodeHierarchyInfo">Can be used to query hierarchical information and
        /// build an animation path string.</param>
        /// <param name="subPath">Optional part that will be appended to the result.</param>
        /// <returns>Animation path.</returns>
        public static string CreateAnimationPath(
            int nodeIndex,
            INodeHierarchyInfo nodeHierarchyInfo,
            string subPath = null
            )
        {
            Profiler.BeginSample("AnimationUtils.CreateAnimationPath");
            var nodes = new Stack<string>();
            var length = subPath != null ? subPath.Length + 1 : 0;
            do
            {
                var nodeName = nodeHierarchyInfo.GetNodeName(nodeIndex);
                Assert.IsNotNull(nodeName, $"Node at index {nodeIndex} has no name");
                length += nodeName.Length + 1;
                nodes.Push(nodeName);
                nodeIndex = nodeHierarchyInfo.GetParentIndex(nodeIndex);
            } while (nodeIndex >= 0);

            var sb = new StringBuilder(length);
            var node = nodes.Pop();
            sb.Append(node);
            while (nodes.TryPop(out node))
            {
                sb.Append('/');
                sb.Append(node);
            }

            if (!string.IsNullOrEmpty(subPath))
            {
                sb.Append('/');
                sb.Append(subPath);
            }
            Profiler.EndSample();
            return sb.ToString();
        }
    }
}
