// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEngine;

namespace GLTFast
{

    using Logging;

    /// <summary>
    /// Generates a GameObject hierarchy from a glTF scene and provides its bounding box
    /// </summary>
    public class GameObjectBoundsInstantiator : GameObjectInstantiator
    {

        Dictionary<uint, Bounds> m_NodeBounds;

        /// <inheritdoc cref="GameObjectInstantiator(IGltfReadable,Transform,ICodeLogger,InstantiationSettings)"/>
        public GameObjectBoundsInstantiator(
            IGltfReadable gltf,
            Transform parent,
            ICodeLogger logger = null,
            InstantiationSettings settings = null
            ) : base(gltf, parent, logger, settings) { }

        /// <inheritdoc />
        public override void BeginScene(
            string name,
            uint[] rootNodeIndices
            )
        {
            base.BeginScene(
                name,
                rootNodeIndices
                );
            m_NodeBounds = new Dictionary<uint, Bounds>();
        }

        /// <inheritdoc />
        public override void AddPrimitive(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint[] joints = null,
            uint? rootJoint = null,
            float[] morphTargetWeights = null,
            int primitiveNumeration = 0
        )
        {
            base.AddPrimitive(
                nodeIndex,
                meshName,
                meshResult,
                joints,
                rootJoint,
                morphTargetWeights,
                primitiveNumeration
            );

            if (m_NodeBounds != null)
            {
                var meshBounds = GetTransformedBounds(meshResult.mesh.bounds, m_Parent.worldToLocalMatrix * m_Nodes[nodeIndex].transform.localToWorldMatrix);
                if (m_NodeBounds.TryGetValue(nodeIndex, out var prevBounds))
                {
                    meshBounds.Encapsulate(prevBounds);
                    m_NodeBounds[nodeIndex] = meshBounds;
                }
                else
                {
                    m_NodeBounds[nodeIndex] = meshBounds;
                }
            }
        }

        /// <summary>
        /// Attempts to calculate the instance's bounds
        /// </summary>
        /// <returns>Instance's bounds, if calculation succeeded</returns>
        public Bounds? CalculateBounds()
        {

            if (m_NodeBounds == null) { return null; }

            var sceneBoundsSet = false;
            var sceneBounds = new Bounds();

            foreach (var nodeBound in m_NodeBounds.Values)
            {
                if (sceneBoundsSet)
                {
                    sceneBounds.Encapsulate(nodeBound);
                }
                else
                {
                    sceneBounds = nodeBound;
                    sceneBoundsSet = true;
                }
            }

            return sceneBoundsSet ? sceneBounds : (Bounds?)null;
        }

        static Bounds GetTransformedBounds(Bounds b, Matrix4x4 transform)
        {
            var corners = new Vector3[8];
            var ext = b.extents;
            for (int i = 0; i < 8; i++)
            {
                var c = b.center;
                c.x += (i & 1) == 0 ? ext.x : -ext.x;
                c.y += (i & 2) == 0 ? ext.y : -ext.y;
                c.z += (i & 4) == 0 ? ext.z : -ext.z;
                corners[i] = c;
            }

            return GeometryUtility.CalculateBounds(corners, transform);
        }
    }
}
