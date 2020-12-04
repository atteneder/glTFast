// Copyright 2020 Andreas Atteneder
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
    public class GameObjectInstantiator : IInstantiator {

        protected Transform parent;

        protected GameObject[] nodes;

        public GameObjectInstantiator(Transform parent) {
            this.parent = parent;
        }

        public virtual void Init(int nodeCount) {
            nodes = new GameObject[nodeCount];
        }

        public void CreateNode(
            uint nodeIndex,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale
        ) {
            var go = new GameObject();
            go.transform.localScale = scale;
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            nodes[nodeIndex] = go;
        }

        public void SetParent(uint nodeIndex, uint parentIndex) {
            if(nodes[nodeIndex]==null || nodes[parentIndex]==null ) {
                Debug.LogError("Invalid hierarchy");
                return;
            }
            nodes[nodeIndex].transform.SetParent(nodes[parentIndex].transform,false);
        }

        public void SetNodeName(uint nodeIndex, string name) {
            nodes[nodeIndex].name = name ?? "Node";
        }

        public virtual void AddPrimitive(
            uint nodeIndex,
            string meshName,
            UnityEngine.Mesh mesh,
            UnityEngine.Material[] materials,
            int[] joints = null,
            bool first = true
        ) {

            GameObject meshGo;
            if(first) {
                // Use Node GameObject for first Primitive
                meshGo = nodes[nodeIndex];
            } else {
                meshGo = new GameObject( meshName ?? "Primitive" );
                meshGo.transform.SetParent(nodes[nodeIndex].transform,false);
            }

            Renderer renderer;

            if(joints==null) {
                var mf = meshGo.AddComponent<MeshFilter>();
                mf.mesh = mesh;
                var mr = meshGo.AddComponent<MeshRenderer>();
                renderer = mr;
            } else {
                var smr = meshGo.AddComponent<SkinnedMeshRenderer>();
                var bones = new Transform[joints.Length];
                for (int j = 0; j < bones.Length; j++)
                {
                    var jointIndex = joints[j];
                    bones[j] = nodes[jointIndex].transform;
                }
                smr.bones = bones;
                smr.sharedMesh = mesh;
                renderer = smr;
            }

            renderer.sharedMaterials = materials;
        }

        public void AddScene(string name, uint[] nodeIndices) {
            var go = new GameObject(name ?? "Scene");
            go.transform.SetParent( parent, false);

            foreach(var nodeIndex in nodeIndices) {
                if (nodes[nodeIndex] != null) {
                    nodes[nodeIndex].transform.SetParent( go.transform, false );
                }
            }
        }
    }
}
