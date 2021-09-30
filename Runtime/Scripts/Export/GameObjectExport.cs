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

namespace GLTFast.Export {

    public class GameObjectExport {

        GltfWriter m_Writer;
        public GameObjectExport() {
            m_Writer = new GltfWriter();
        }

        public void AddScene(string name, GameObject[] gameObjects) {
            var rootNodes = new uint[gameObjects.Length];
            for (var index = 0; index < gameObjects.Length; index++) {
                var gameObject = gameObjects[index];
                rootNodes[index] = AddGameObject(gameObject);
            }
            m_Writer.AddScene(name,rootNodes);
        }

        public void SaveToFile(string path) {
            m_Writer.SaveToFile(path);
        }

        uint AddGameObject(GameObject gameObject) {
            var childCount = gameObject.transform.childCount;
            uint[] children = null;
            if (childCount > 0) {
                children = new uint[gameObject.transform.childCount];
                for (int i = 0; i < childCount; i++) {
                    var child = gameObject.transform.GetChild(i);
                    children[i] = AddGameObject(child.gameObject);
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
            if (gameObject.TryGetComponent(out MeshFilter meshFilter)) {
                mesh = meshFilter.sharedMesh;
            } else
            if (gameObject.TryGetComponent(out SkinnedMeshRenderer smr)) {
                mesh = smr.sharedMesh;
            }
            if (mesh != null) {
                m_Writer.AddMeshToNode(nodeId,mesh);
            }
            return nodeId;
        }
    }
}
