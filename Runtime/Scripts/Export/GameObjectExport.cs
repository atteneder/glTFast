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

using System.Collections.Generic;
using UnityEngine;

namespace GLTFast.Export {

    public class GameObjectExport {

        GltfWriter m_Writer;
        public GameObjectExport() {
            m_Writer = new GltfWriter();
        }

        public void AddScene(string name, GameObject[] gameObjects) {
            var rootNodes = new List<uint>(gameObjects.Length);
            var tempMaterials = new List<Material>();
            for (var index = 0; index < gameObjects.Length; index++) {
                var gameObject = gameObjects[index];
                if(!gameObject.activeInHierarchy) continue;
                var nodeId = AddGameObject(gameObject,tempMaterials);
                if (nodeId >= 0) {
                    rootNodes.Add((uint)nodeId);
                }
            }
            if (rootNodes.Count > 0) {
                m_Writer.AddScene(name,rootNodes.ToArray());
            }
        }

        public void SaveToFile(string path) {
            m_Writer.SaveToFile(path);
        }

        int AddGameObject(GameObject gameObject, List<Material> tempMaterials ) {
            if(!gameObject.activeInHierarchy) return -1;
            var childCount = gameObject.transform.childCount;
            uint[] children = null;
            if (childCount > 0) {
                var childList = new List<uint>(gameObject.transform.childCount);
                for (var i = 0; i < childCount; i++) {
                    var child = gameObject.transform.GetChild(i);
                    var childNodeId = AddGameObject(child.gameObject, tempMaterials);
                    if (childNodeId >= 0) {
                        childList.Add((uint)childNodeId);
                    }
                }
                if (childList.Count > 0) {
                    children = childList.ToArray();
                }
            }

            var transform = gameObject.transform;
            var nodeId = m_Writer.AddNode(
                gameObject.name,
                transform.localPosition,
                transform.localRotation,
                transform.localScale,
                children
                );
            Mesh mesh = null;
            
            tempMaterials.Clear();
            
            if (gameObject.TryGetComponent(out MeshFilter meshFilter)) {
                mesh = meshFilter.sharedMesh;
                if (gameObject.TryGetComponent(out Renderer renderer)) {
                    renderer.GetSharedMaterials(tempMaterials);
                }
            } else
            if (gameObject.TryGetComponent(out SkinnedMeshRenderer smr)) {
                mesh = smr.sharedMesh;
                smr.GetSharedMaterials(tempMaterials);
            }
            if (mesh != null) {
                m_Writer.AddMeshToNode(nodeId,mesh,tempMaterials);
            }
            return (int)nodeId;
        }
    }
}
