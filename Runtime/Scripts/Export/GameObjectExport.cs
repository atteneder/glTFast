// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
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
        /// <param name="deferAgent">Defer agent (&lt;see cref="IDeferAgent"/&gt;); decides when/if to preempt
        /// export to preserve a stable frame rate.</param>
        /// <param name="logger">Interface for logging (error) messages.</param>
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
        /// Adds a scene to the glTF which consists of a collection of GameObjects.
        /// </summary>
        /// <param name="gameObjects">GameObjects to be added (recursively) as root level nodes.</param>
        /// <param name="name">Name of the scene</param>
        /// <returns>True, if the scene was added flawlessly. False, otherwise</returns>
        public bool AddScene(GameObject[] gameObjects, string name = null)
        {
            return AddScene(gameObjects, float4x4.identity, name);
        }

        /// <summary>
        /// Creates a glTF scene from a collection of GameObjects. The GameObjects will be converted into glTF nodes.
        /// The nodes' positions within the glTF scene will be their GameObjects' world position transformed by the
        /// <see cref="origin"/> matrix, essentially allowing you to set an arbitrary scene center.
        /// </summary>
        /// <param name="gameObjects">Root level GameObjects (will get added recursively)</param>
        /// <param name="origin">Inverse scene origin matrix. This transform will be applied to all nodes.</param>
        /// <param name="name">Name of the scene</param>
        /// <returns>True if the scene was added successfully, false otherwise</returns>
        public bool AddScene(ICollection<GameObject> gameObjects, float4x4 origin, string name)
        {
            CertifyNotDisposed();
            var rootNodes = new List<uint>(gameObjects.Count);
            var tempMaterials = new List<Material>();
            var success = true;

            var nodesQueue = new Queue<Transform>();
            var transformNodeId = new Dictionary<Transform, uint>();

            foreach (var gameObject in gameObjects)
            {
                success &= AddGameObject(
                    gameObject,
                    origin,
                    nodesQueue,
                    transformNodeId,
                    out var nodeId
                );
                if (nodeId >= 0)
                {
                    rootNodes.Add((uint)nodeId);
                }
            }

            while (nodesQueue.Count > 0)
            {
                var transform = nodesQueue.Dequeue();
                AddNodeComponents(
                    transform,
                    transformNodeId,
                    tempMaterials
                    );
            }
            if (rootNodes.Count > 0)
            {
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
        bool AddGameObject(
            GameObject gameObject,
            float4x4? sceneOrigin,
            Queue<Transform> nodesQueue,
            Dictionary<Transform, uint> transformNodeId,
            out int nodeId)
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
                    success &= AddGameObject(
                        child.gameObject,
                        null,
                        nodesQueue,
                        transformNodeId,
                        out var childNodeId
                        );
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
                float3 translation;
                quaternion rotation;
                float3 scale;

                if (sceneOrigin.HasValue)
                {
                    // root level node - calculate transform based on scene origin
                    var trans = math.mul(sceneOrigin.Value, transform.localToWorldMatrix);
                    trans.Decompose(out translation, out rotation, out scale);
                }
                else
                {
                    // nested node - use local transform
                    translation = transform.localPosition;
                    rotation = transform.localRotation;
                    scale = transform.localScale;
                }

                var newNodeId = m_Writer.AddNode(
                    translation,
                    rotation,
                    scale,
                    children,
                    gameObject.name
                    );

                if (onIncludedLayer)
                {
                    nodesQueue.Enqueue(transform);
                }
                transformNodeId[transform] = newNodeId;

                nodeId = (int)newNodeId;
            }
            else
            {
                nodeId = -1;
            }

            return success;
        }

        void AddNodeComponents(
            Transform transform,
            Dictionary<Transform, uint> transformNodeId,
            List<Material> tempMaterials
            )
        {
            var gameObject = transform.gameObject;
            var nodeId = transformNodeId[transform];
            tempMaterials.Clear();
            Mesh mesh = null;
            Transform[] bones = null;
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
                    bones = smr.bones;
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
                uint[] joints = null;
                if (bones != null)
                {
                    joints = new uint[bones.Length];
                    for (var i = 0; i < bones.Length; i++)
                    {
                        var bone = bones[i];
                        if (!transformNodeId.TryGetValue(bone, out joints[i]))
                        {
#if DEBUG
                            Debug.LogError($"Skip skin on {transform.name}: No node ID for bone transform {bone.name} found!");
                            joints = null;
                            break;
#endif
                        }
                    }
                }
                m_Writer.AddMeshToNode((int)nodeId, mesh, materialIds, joints);
            }

            if (gameObject.TryGetComponent(out Camera camera))
            {
                if (camera.enabled || m_Settings.DisabledComponents)
                {
                    if (m_Writer.AddCamera(camera, out var cameraId))
                    {
                        m_Writer.AddCameraToNode((int)nodeId, cameraId);
                    }
                }
            }

            if (gameObject.TryGetComponent(out Light light))
            {
                if (light.enabled || m_Settings.DisabledComponents)
                {
                    if (m_Writer.AddLight(light, out var lightId))
                    {
                        m_Writer.AddLightToNode((int)nodeId, lightId);
                    }
                }
            }
        }
    }
}
