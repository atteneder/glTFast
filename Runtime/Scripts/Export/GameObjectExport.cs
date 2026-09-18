// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Export
{

    using Logging;

    /// <summary>
    /// Creates glTF files from GameObject hierarchies
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Export", sourceAssembly: "glTFast.Export")]
    public class GameObjectExport
    {

#if UNITY_EDITOR
        static bool s_SyncWarningRaised;
#endif

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
        /// <param name="logger">Custom logger for reporting messages. Default behavior is inherited from the <see cref="Unity.Cloud.Gltfast.Export.GltfWriter(Unity.Cloud.Gltfast.Export.ExportSettings, Unity.Cloud.Gltfast.IDeferAgent, Unity.Cloud.Gltfast.Logging.ICodeLogger)"/> constructor that this method forwards to.</param>
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
            return AddScene(gameObjects, double4x4.identity, name);
        }

        /// <summary>
        /// Creates a glTF scene from a collection of GameObjects. The GameObjects will be converted into glTF nodes.
        /// The nodes' positions within the glTF scene will be their GameObjects' world position transformed by the
        /// <paramref name="origin"/> matrix, essentially allowing you to set an arbitrary scene center.
        /// </summary>
        /// <param name="gameObjects">Root level GameObjects (will get added recursively)</param>
        /// <param name="origin">Inverse scene origin matrix. This transform will be applied to all nodes.</param>
        /// <param name="name">Name of the scene</param>
        /// <returns>True if the scene was added successfully, false otherwise</returns>
        public bool AddScene(ICollection<GameObject> gameObjects, Matrix4x4 origin, string name)
        {
            return AddScene(gameObjects, origin.ToDouble(), name);
        }

        /// <summary>
        /// Creates a glTF scene from a collection of GameObjects. The GameObjects will be converted into glTF nodes.
        /// The nodes' positions within the glTF scene will be their GameObjects' world position transformed by the
        /// <paramref name="origin"/> matrix, essentially allowing you to set an arbitrary scene center.
        /// </summary>
        /// <param name="gameObjects">Root level GameObjects (will get added recursively)</param>
        /// <param name="origin">Inverse scene origin matrix. This transform will be applied to all nodes.</param>
        /// <param name="name">Name of the scene</param>
        /// <returns>True if the scene was added successfully, false otherwise</returns>
        public bool AddScene(ICollection<GameObject> gameObjects, double4x4 origin, string name)
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
                m_Writer.AddScene(rootNodes, name);
            }

            return success;
        }

        [Obsolete("SaveToFileAndDispose has been renamed to SaveToFileAndDisposeAsync. (UnityUpgradable) -> SaveToFileAndDisposeAsync(*)", true)]
        public Task<bool> SaveToFileAndDispose(string path, CancellationToken cancellationToken = default)
            => SaveToFileAndDisposeAsync(path, cancellationToken);

        /// <summary>
        /// Exports the collected scenes/content as glTF, writes it to a file
        /// and disposes this object.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="path">glTF destination file path</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the glTF file was created successfully, false otherwise</returns>
        public Task<bool> SaveToFileAndDisposeAsync(string path, CancellationToken cancellationToken = default)
            => SaveToFileAndDisposeAsync(path, false, cancellationToken);

        [Obsolete("SaveToFileAndDispose has been renamed to SaveToFileAndDisposeAsync. (UnityUpgradable) -> SaveToFileAndDisposeAsync(*)", true)]
        public Task<bool> SaveToFileAndDispose(
            string path,
            bool forceSync,
            CancellationToken cancellationToken = default
        )
            => SaveToFileAndDisposeAsync(path, forceSync, cancellationToken);

        /// <summary>
        /// Exports the collected scenes/content as glTF, writes it to a file
        /// and disposes this object.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="path">glTF destination file path</param>
        /// <param name="forceSync">When true, enforces sync execution path. Useful to avoid async limitations in Editor
        /// scripting.</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the glTF file was created successfully, false otherwise</returns>
        public async Task<bool> SaveToFileAndDisposeAsync(
            string path,
            bool forceSync,
            CancellationToken cancellationToken = default
        )
        {
            CertifyNotDisposed();
#if UNITY_EDITOR
            CertifyEditorForceSync(forceSync, nameof(SaveToFileAndDisposeAsync));
#endif
            var success = await m_Writer.SaveToFileAndDisposeAsyncInternal(path, forceSync);
            m_Writer = null;
            return success;
        }

        [Obsolete("SaveToStreamAndDispose has been renamed to SaveToStreamAndDisposeAsync. (UnityUpgradable) -> SaveToStreamAndDisposeAsync(*)", true)]
        public Task<bool> SaveToStreamAndDispose(
            Stream stream,
            CancellationToken cancellationToken = default
            ) => SaveToStreamAndDisposeAsync(stream, cancellationToken);

        /// <summary>
        /// Exports the collected scenes/content as glTF, writes it to a Stream
        /// and disposes this object. Only works for self-contained glTF-Binary.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="stream">glTF destination stream</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the glTF file was written successfully, false otherwise</returns>
        public Task<bool> SaveToStreamAndDisposeAsync(
            Stream stream,
            CancellationToken cancellationToken = default
            ) => SaveToStreamAndDisposeAsync(stream, false, cancellationToken);

        [Obsolete("SaveToStreamAndDispose has been renamed to SaveToStreamAndDisposeAsync. (UnityUpgradable) -> SaveToStreamAndDisposeAsync(*)", true)]
        public Task<bool> SaveToStreamAndDispose(
            Stream stream,
            bool forceSync,
            CancellationToken cancellationToken = default
        )
            => SaveToStreamAndDisposeAsync(stream, forceSync, cancellationToken);

        /// <summary>
        /// Exports the collected scenes/content as glTF, writes it to a Stream
        /// and disposes this object. Only works for self-contained glTF-Binary.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="stream">glTF destination stream</param>
        /// <param name="forceSync">When true, enforces sync execution path. Useful to avoid async limitations in Editor
        /// scripting.</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the glTF file was written successfully, false otherwise</returns>
        public async Task<bool> SaveToStreamAndDisposeAsync(
            Stream stream,
            bool forceSync,
            CancellationToken cancellationToken = default
        )
        {
            CertifyNotDisposed();
#if UNITY_EDITOR
            CertifyEditorForceSync(forceSync, nameof(SaveToStreamAndDisposeAsync));
#endif
            var success = await m_Writer.SaveToStreamAndDisposeAsync(stream, forceSync);
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

#if UNITY_EDITOR
        static void CertifyEditorForceSync(bool forceSync, string methodName)
        {
            if (!forceSync && !Application.isPlaying && !s_SyncWarningRaised)
            {
                Debug.LogWarningFormat(
                    "{0} was called from the Editor in Edit Mode with forceSync: false. Unity does" +
                    " not pump the main-thread SynchronizationContext outside Play Mode, so awaited I/O" +
                    " continuations may never resume and the export can hang. Pass forceSync: true from Editor " +
                    "scripts (menu items, inspectors, post-processors).",
                    methodName);
                s_SyncWarningRaised = true;
            }
        }
#endif

        bool AddGameObject(
            GameObject gameObject,
            double4x4? sceneOrigin,
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
            List<uint> children = null;
            if (childCount > 0)
            {
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
                        children ??= new List<uint>(childCount);
                        children.Add((uint)childNodeId);
                    }
                }
            }

            var transform = gameObject.transform;

            var onIncludedLayer = ((1 << gameObject.layer) & m_Settings.LayerMask) != 0;

            if (onIncludedLayer || children != null)
            {
                double3 translation;
                double4 rotation;
                double3 scale;

                if (sceneOrigin.HasValue)
                {
                    // root level node - calculate transform based on scene origin
                    var localToWorldMatrix = transform.localToWorldMatrix.ToDouble();
                    var trans = math.mul(sceneOrigin.Value, localToWorldMatrix);
                    trans.Decompose(out translation, out rotation, out scale);
                }
                else
                {
                    // nested node - use local transform
                    translation = transform.localPosition.ToDouble();
                    rotation = transform.localRotation.ToDouble();
                    scale = transform.localScale.ToDouble();
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
            else if (gameObject.TryGetComponent(out SkinnedMeshRenderer smr)
                     && (smr.enabled || m_Settings.DisabledComponents))
            {
                mesh = smr.sharedMesh;
                bones = smr.bones;
                smr.GetSharedMaterials(tempMaterials);
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
                List<uint> joints = null;
                if (bones != null)
                {
                    joints = new List<uint>(bones.Length);
                    foreach (var bone in bones)
                    {
                        if (!transformNodeId.TryGetValue(bone, out var boneNodeId))
                        {
#if DEBUG
                            Debug.LogError($"Skip skin on {transform.name}: No node ID for bone transform {bone.name} found!");
                            joints = null;
                            break;
#endif
                        }
                        joints.Add(boneNodeId);
                    }
                }
                m_Writer.AddMeshToNode(nodeId, mesh, materialIds, joints);
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
