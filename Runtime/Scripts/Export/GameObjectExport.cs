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

namespace GLTFast.Export {

    public class GameObjectExport {

        GltfWriter m_Writer;
        
        public GameObjectExport(ExportSettings exportSettings = null, ICodeLogger logger = null) {
            m_Writer = new GltfWriter(exportSettings, logger);
        }

        /// <summary>
        /// Adds a scene to the glTF
        /// </summary>
        /// <param name="gameObjects">Root level GameObjects (will get added recursively)</param>
        /// <param name="name">Name of the scene</param>
        /// <returns>True if the scene was added flawlessly, false otherwise</returns>
        public bool AddScene(GameObject[] gameObjects, string name = null) {
            var rootNodes = new List<uint>(gameObjects.Length);
            var tempMaterials = new List<Material>();
            var success = true;
            for (var index = 0; index < gameObjects.Length; index++) {
                var gameObject = gameObjects[index];
                if(!gameObject.activeInHierarchy) continue;
                success &= AddGameObject(gameObject,tempMaterials, out var nodeId);
                if (nodeId >= 0) {
                    rootNodes.Add((uint)nodeId);
                }
            }
            if (rootNodes.Count > 0) {
                m_Writer.AddScene(rootNodes.ToArray(), name);
            }

            return success;
        }

        public bool SaveToFile(string path) {
            return m_Writer.SaveToFile(path);
        }

        bool AddGameObject(GameObject gameObject, List<Material> tempMaterials, out int nodeId ) {
            if (!gameObject.activeInHierarchy) {
                nodeId = -1;
                return true;
            }

            var success = true;
            var childCount = gameObject.transform.childCount;
            uint[] children = null;
            if (childCount > 0) {
                var childList = new List<uint>(gameObject.transform.childCount);
                for (var i = 0; i < childCount; i++) {
                    var child = gameObject.transform.GetChild(i);
                    success &= AddGameObject(child.gameObject, tempMaterials, out var childNodeId);
                    if (childNodeId >= 0) {
                        childList.Add((uint)childNodeId);
                    }
                }
                if (childList.Count > 0) {
                    children = childList.ToArray();
                }
            }

            var transform = gameObject.transform;
            nodeId = (int) m_Writer.AddNode(
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
                success &= m_Writer.AddMeshToNode(nodeId,mesh,tempMaterials);
            }
            return success;
        }
    }
}