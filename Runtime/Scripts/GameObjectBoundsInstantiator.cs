﻿// Copyright 2020-2021 Andreas Atteneder
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

using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {

    public class GameObjectBoundsInstantiator : GameObjectInstantiator {

        Dictionary<uint, Bounds> nodeBounds;

        public GameObjectBoundsInstantiator(IGltfReadable gltf, Transform parent, ICodeLogger logger = null) : base(gltf,parent,logger) {}
        
        public override void Init() {
            base.Init();
            nodeBounds = new Dictionary<uint, Bounds>();
        }

        public override void AddPrimitive(
            uint nodeIndex,
            string meshName,
            Mesh mesh,
            int[] materialIndices,
            uint[] joints = null,
            uint? rootJoint = null,
            float[] morphTargetWeights = null,
            int primitiveNumeration = 0
        ) {
            base.AddPrimitive(
                nodeIndex,
                meshName,
                mesh,
                materialIndices,
                joints,
                rootJoint,
                morphTargetWeights,
                primitiveNumeration
            );

            if (nodeBounds!=null) {
                var meshBounds = GetTransformedBounds(mesh.bounds, nodes[nodeIndex].transform.localToWorldMatrix);
                if (nodeBounds.TryGetValue(nodeIndex,out var prevBounds)) {
                    meshBounds.Encapsulate(prevBounds);
                    nodeBounds[nodeIndex] = meshBounds;
                }
                else {
                    nodeBounds[nodeIndex] = meshBounds;
                }
            }
        }

        public Bounds? CalculateBounds() {

            if (nodeBounds == null) { return null; }

            var sceneBoundsSet = false;
            var sceneBounds = new Bounds();
            
            foreach (var nodeBound in nodeBounds.Values) {
                if (sceneBoundsSet) {
                    sceneBounds.Encapsulate(nodeBound);
                } else {
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
