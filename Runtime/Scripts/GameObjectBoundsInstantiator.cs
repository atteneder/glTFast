// Copyright 2020-2021 Andreas Atteneder
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

using UnityEngine;

namespace GLTFast {

    public class GameObjectBoundsInstantiator : GameObjectInstantiator {

        Bounds?[] nodeBounds;

        public GameObjectBoundsInstantiator(Transform parent) : base(parent) {}
        
        public override void Init(int nodeCount) {
            base.Init(nodeCount);
            nodeBounds = new Bounds?[nodeCount];
        }

        public override void AddPrimitive(
            uint nodeIndex,
            string meshName,
            Mesh mesh,
            Material[] materials,
            int[] joints = null,
            int primitiveNumeration = 0
        ) {
            base.AddPrimitive(
                nodeIndex,
                meshName,
                mesh,
                materials,
                joints,
                primitiveNumeration
                );

            if (nodeBounds!=null) {
                var meshBounds = GetTransformedBounds(mesh.bounds, nodes[nodeIndex].transform.localToWorldMatrix);
                if (nodeBounds[nodeIndex].HasValue) {
                    meshBounds.Encapsulate(nodeBounds[nodeIndex].Value);
                    nodeBounds[nodeIndex] = meshBounds;
                }
                else {
                    nodeBounds[nodeIndex] = meshBounds;
                }
            }
        }

        public Bounds? CalculateBounds() {

            bool sceneBoundsSet = false;
            Bounds sceneBounds = new Bounds();

            var nodesLength = nodes.Length;
            for (int nodeIndex = 0; nodeIndex < nodesLength; nodeIndex++)
            {
                if (nodes[nodeIndex] == null
                    || nodeBounds == null
                    || !nodeBounds[nodeIndex].HasValue) continue;

                if (sceneBoundsSet) {
                    sceneBounds.Encapsulate(nodeBounds[nodeIndex].Value);
                } else {
                    sceneBounds = nodeBounds[nodeIndex].Value;
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
