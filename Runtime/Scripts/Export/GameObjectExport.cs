﻿// Copyright 2020-2022 Andreas Atteneder
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast.Export {

    using Logging;
    
    /// <summary>
    /// Creates glTF files from GameObject hierarchies 
    /// </summary>
    public class GameObjectExport {

        GltfWriter m_Writer;
        IMaterialExport m_MaterialExport;
        GameObjectExportSettings m_Settings;
        ICodeLogger m_Logger;

        /// <summary>
        /// Provides glTF export of GameObject based scenes and hierarchies.
        /// </summary>
        /// <param name="exportSettings">Export settings</param>
        /// <param name="gameObjectExportSettings">GameObject export settings</param>
        /// <param name="materialExport">Provides material conversion</param>
        /// <param name="deferAgent">Defer agent; decides when/if to preempt
        /// export to preserve a stable frame rate <seealso cref="IDeferAgent"/></param>
        /// <param name="logger">Interface for logging (error) messages
        /// <seealso cref="ConsoleLogger"/></param>
        public GameObjectExport(
            ExportSettings exportSettings = null,
            GameObjectExportSettings gameObjectExportSettings = null,
            IMaterialExport materialExport = null,
            IDeferAgent deferAgent = null,
            ICodeLogger logger = null
        ) {
            m_Settings = gameObjectExportSettings ?? new GameObjectExportSettings();
            m_Writer = new GltfWriter(exportSettings, deferAgent, logger);
            m_MaterialExport = materialExport ?? MaterialExport.GetDefaultMaterialExport();
            m_Logger = logger;
        }

        /// <summary>
        /// Adds a scene to the glTF.
        /// If the conversion to glTF was not flawless (i.e. parts of the scene
        /// were not converted 100% correctly) you still might be able to
        /// export a glTF. You may use the <seealso cref="CollectingLogger"/>
        /// to analyze what exactly went wrong. 
        /// </summary>
        /// <param name="gameObjects">Root level GameObjects (will get added recursively)</param>
        /// <param name="name">Name of the scene</param>
        /// <returns>True if the scene was added flawlessly, false otherwise</returns>
        public bool AddScene(GameObject[] gameObjects, string name = null) {
            CertifyNotDisposed();
            var rootNodes = new List<uint>(gameObjects.Length);
            var tempMaterials = new List<Material>();
            var success = true;
            for (var index = 0; index < gameObjects.Length; index++) {
                var gameObject = gameObjects[index];
                if(m_Settings.onlyActiveInHierarchy && !gameObject.activeInHierarchy) continue;
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
        
        /// <summary>
        /// Exports the collected scenes/content as glTF, writes it to a file
        /// and disposes this object.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="path">glTF destination file path</param>
        /// <returns>True if the glTF file was created successfully, false otherwise</returns>
        public async Task<bool> SaveToFileAndDispose(string path) {
            CertifyNotDisposed();
            var success = await m_Writer.SaveToFileAndDispose(path);
            m_Writer = null;
            return success;
        }
        
        /// <summary>
        /// Exports the collected scenes/content as glTF, writes it to a Stream
        /// and disposes this object. Only works for self-contained glTF-Binary.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="stream">glTF destination stream</param>
        /// <returns>True if the glTF file was written successfully, false otherwise</returns>
        public async Task<bool> SaveToStreamAndDispose(Stream stream) {
            CertifyNotDisposed();
            var success = await m_Writer.SaveToStreamAndDispose(stream);
            m_Writer = null;
            return success;
        }

        void CertifyNotDisposed() {
            if (m_Writer == null) {
                throw new InvalidOperationException("GameObjectExport was already disposed");
            }
        }
        bool AddGameObject(GameObject gameObject, List<Material> tempMaterials, out int nodeId ) {
            if (m_Settings.onlyActiveInHierarchy && !gameObject.activeInHierarchy) {
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
                transform.localPosition,
                transform.localRotation,
                transform.localScale,
                children,
                gameObject.name
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

            var materialIds = new int[tempMaterials.Count];
            for (var i = 0; i < tempMaterials.Count; i++) {
                var uMaterial = tempMaterials[i];
                if ( uMaterial!=null && m_Writer.AddMaterial(uMaterial, out var materialId, m_MaterialExport) ) {
                    materialIds[i] = materialId;
                } else {
                    materialIds[i] = -1;
                }
            }

            if (mesh != null) {
                m_Writer.AddMeshToNode(nodeId,mesh,materialIds);
            }
            
            if (gameObject.TryGetComponent(out Light light)) {
                if (light.type == LightType.Point || 
                    light.type == LightType.Spot || 
                    light.type == LightType.Directional) {
                    m_Writer.AddLightToNode(nodeId, light);
                }
            }
            
            return success;
        }
    }
}
