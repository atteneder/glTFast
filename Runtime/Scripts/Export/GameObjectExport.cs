// Copyright 2020-2022 Andreas Atteneder
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast.Export
{

    using Logging;

    /// <summary>
    /// Creates glTF files from GameObject hierarchies
    /// </summary>
    public class GameObjectExport
    {

        GltfWriter m_Writer;
        IMaterialExport m_MaterialExport;
        GameObjectExportSettings m_Settings;

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
        )
        {
            m_Settings = gameObjectExportSettings ?? new GameObjectExportSettings();
            m_Writer = new GltfWriter(exportSettings, deferAgent, logger);
            m_MaterialExport = materialExport ?? MaterialExport.GetDefaultMaterialExport();
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
        public bool AddScene(GameObject[] gameObjects, string name = null)
        {
            CertifyNotDisposed();
            var rootNodes = new List<uint>(gameObjects.Length);
            var tempMaterials = new List<Material>();
            var success = true;
            List<GameObject> gameObjectList = gameObjects.ToList();
            GameObject skybox = CreateSkyboxObject();
            gameObjectList.Add(skybox);

            for (var index = 0; index < gameObjectList.Count; index++)
            {
                var gameObject = gameObjectList[index];
                success &= AddGameObject(gameObject, tempMaterials, out var nodeId);
                if (nodeId >= 0)
                {
                    rootNodes.Add((uint)nodeId);
                }
            }
            if (rootNodes.Count > 0)
            {
                m_Writer.AddScene(rootNodes.ToArray(), name);
            }

            GameObject.DestroyImmediate(skybox);

            return success;
        }

        GameObject CreateSkyboxObject()
        {
            GameObject skybox =
                GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Material skyboxMaterial = RenderSettings.skybox;
            skybox.name = skyboxMaterial.shader.name.Replace("/", "_");
            skybox.GetComponent<MeshRenderer>().sharedMaterial = skyboxMaterial;

            if (skyboxMaterial.HasProperty("_Rotation"))
            {
                Vector3 rotation =
                    new Vector3(0, skyboxMaterial.GetFloat("_Rotation"), 0);

                // Cube map has different rotation when exported as 'Panoramic'
                if (skyboxMaterial.shader.name == "Skybox/Cubemap")
                {
                    rotation.y += 90;
                }

                skybox.transform.eulerAngles = rotation;
            }

            if (skybox.name.Contains("Procedural"))
            {
                // Serialise procedural skybox properties into Transform (hack)
                float sunSize =
                    skyboxMaterial.IsKeywordEnabled("_SUNDISK_NONE") ? 0 :
                    skyboxMaterial.GetFloat("_SunSize");
                Debug.Log("Sun size: " + sunSize);
                skybox.transform.position = new Vector3(
                    sunSize,
                    skyboxMaterial.GetFloat("_Exposure"),
                    skyboxMaterial.GetFloat("_AtmosphereThickness"));
                Color skyTint = skyboxMaterial.GetColor("_SkyTint");
                skybox.transform.eulerAngles =
                    new Vector3(skyTint.r, skyTint.g, skyTint.b);
                Color ground = skyboxMaterial.GetColor("_GroundColor");
                skybox.transform.localScale =
                    new Vector3(ground.r, ground.g, ground.b);
            }

            return skybox;
        }

        /// <summary>
        /// Exports the collected scenes/content as glTF, writes it to a file
        /// and disposes this object.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="path">glTF destination file path</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the glTF file was created successfully, false otherwise</returns>
        public async Task<bool> SaveToFileAndDispose(
            string path,
            CancellationToken cancellationToken = default
            )
        {
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
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the glTF file was written successfully, false otherwise</returns>
        public async Task<bool> SaveToStreamAndDispose(
            Stream stream,
            CancellationToken cancellationToken = default
            )
        {
            CertifyNotDisposed();
            var success = await m_Writer.SaveToStreamAndDispose(stream);
            m_Writer = null;
            return success;
        }

        void CertifyNotDisposed()
        {
            if (m_Writer == null)
            {
                throw new InvalidOperationException("GameObjectExport was already disposed");
            }
        }
        bool AddGameObject(GameObject gameObject, List<Material> tempMaterials, out int nodeId)
        {
            if (m_Settings.OnlyActiveInHierarchy && !gameObject.activeInHierarchy
                || gameObject.CompareTag("EditorOnly"))
            {
                nodeId = -1;
                return true;
            }

            var success = true;
            var childCount = gameObject.transform.childCount;
            uint[] children = null;
            if (childCount > 0)
            {
                var childList = new List<uint>(gameObject.transform.childCount);
                for (var i = 0; i < childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i);
                    success &= AddGameObject(child.gameObject, tempMaterials, out var childNodeId);
                    if (childNodeId >= 0)
                    {
                        childList.Add((uint)childNodeId);
                    }
                }
                if (childList.Count > 0)
                {
                    children = childList.ToArray();
                }
            }

            var transform = gameObject.transform;

            var onIncludedLayer = ((1 << gameObject.layer) & m_Settings.LayerMask) != 0;

            if (onIncludedLayer || children != null)
            {
                gameObject.TryGetComponent<A28.GuidComponent>(out A28.GuidComponent guidComponent);
                string guid = guidComponent?.GuidString;
                nodeId = (int)m_Writer.AddNode(
                    transform.localPosition,
                    transform.localRotation,
                    transform.localScale,
                    children,
                    gameObject.name,
                    guid
                    );

                if (onIncludedLayer)
                {
                    AddNodeComponents(gameObject, tempMaterials, nodeId);
                }
            }
            else
            {
                nodeId = -1;
            }

            return success;
        }

        void AddNodeComponents(GameObject gameObject, List<Material> tempMaterials, int nodeId)
        {
            tempMaterials.Clear();
            Mesh mesh = null;
            if (gameObject.TryGetComponent(out MeshFilter meshFilter))
            {
                if (gameObject.TryGetComponent(out Renderer renderer))
                {
                    if (renderer.enabled || m_Settings.DisabledComponents)
                    {
                        mesh = meshFilter.sharedMesh;
                        renderer.GetSharedMaterials(tempMaterials);
                    }
                }
            }
            else
            if (gameObject.TryGetComponent(out SkinnedMeshRenderer smr))
            {
                if (smr.enabled || m_Settings.DisabledComponents)
                {
                    mesh = smr.sharedMesh;
                    smr.GetSharedMaterials(tempMaterials);
                }
            }

            var materialIds = new int[tempMaterials.Count];
            for (var i = 0; i < tempMaterials.Count; i++)
            {
                var uMaterial = tempMaterials[i];
                if (uMaterial != null && m_Writer.AddMaterial(uMaterial, out var materialId, m_MaterialExport))
                {
                    materialIds[i] = materialId;
                }
                else
                {
                    materialIds[i] = -1;
                }
            }

            if (mesh != null)
            {
                m_Writer.AddMeshToNode(nodeId, mesh, materialIds);
            }

            if (gameObject.TryGetComponent(out Camera camera))
            {
                if (camera.enabled || m_Settings.DisabledComponents)
                {
                    if (m_Writer.AddCamera(camera, out var cameraId))
                    {
                        m_Writer.AddCameraToNode(nodeId, cameraId);
                    }
                }
            }

            if (gameObject.TryGetComponent(out Light light))
            {
                if (light.enabled || m_Settings.DisabledComponents)
                {
                    if (m_Writer.AddLight(light, out var lightId))
                    {
                        m_Writer.AddLightToNode(nodeId, lightId);
                    }
                }
            }
        }
    }
}
