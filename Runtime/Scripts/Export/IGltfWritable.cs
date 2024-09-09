// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Export
{

    /// <summary>
    /// Is able to receive asset resources and export them to glTF
    /// </summary>
    public interface IGltfWritable
    {

        /// <summary>
        /// Adds a node to the glTF
        /// </summary>
        /// <param name="translation">Local translation of the node (in Unity-space)</param>
        /// <param name="rotation">Local rotation of the node (in Unity-space)</param>
        /// <param name="scale">Local scale of the node (in Unity-space)</param>
        /// <param name="children">Array of node indices that are parented to
        /// this newly created node</param>
        /// <param name="name">Name of the node</param>
        /// <returns>glTF node index</returns>
        uint AddNode(
            float3? translation = null,
            quaternion? rotation = null,
            float3? scale = null,
            uint[] children = null,
            string name = null
        );

        /// <summary>
        /// Assigns a mesh to a previously added node
        /// </summary>
        /// <param name="nodeId">Index of the node to add the mesh to</param>
        /// <param name="uMesh">Unity mesh to be assigned and exported</param>
        /// <param name="materialIds">glTF materials IDs to be assigned
        /// (multiple in case of sub-meshes)</param>
        [Obsolete("Use overload with skinning parameter.")]
        void AddMeshToNode(int nodeId, Mesh uMesh, int[] materialIds);

        /// <summary>
        /// Assigns a mesh to a previously added node
        /// </summary>
        /// <param name="nodeId">Index of the node to add the mesh to</param>
        /// <param name="uMesh">Unity mesh to be assigned and exported</param>
        /// <param name="materialIds">glTF materials IDs to be assigned
        /// (multiple in case of sub-meshes)</param>
        /// <param name="skinning">Skinning has been applied (e.g. <see cref="SkinnedMeshRenderer"/>).</param>
        [Obsolete("Use overload with joints parameter.")]
        void AddMeshToNode(int nodeId, Mesh uMesh, int[] materialIds, bool skinning);

        /// <summary>
        /// Assigns a mesh to a previously added node
        /// </summary>
        /// <param name="nodeId">Index of the node to add the mesh to</param>
        /// <param name="uMesh">Unity mesh to be assigned and exported</param>
        /// <param name="materialIds">glTF materials IDs to be assigned
        /// (multiple in case of sub-meshes)</param>
        /// <param name="joints">Node indices representing the joints of a skin.</param>
        void AddMeshToNode(int nodeId, Mesh uMesh, int[] materialIds, uint[] joints);

        /// <summary>
        /// Assigns a camera to a previously added node
        /// </summary>
        /// <param name="nodeId">Index of the node to add the mesh to</param>
        /// <param name="cameraId">glTF camera ID to be assigned</param>
        void AddCameraToNode(int nodeId, int cameraId);

        /// <summary>
        /// Assigns a light to a previously added node
        /// </summary>
        /// <param name="nodeId">Index of the node to add the mesh to</param>
        /// <param name="lightId">glTF light ID to be assigned</param>
        void AddLightToNode(int nodeId, int lightId);

        /// <summary>
        /// Adds a Unity material
        /// </summary>
        /// <param name="uMaterial">Unity material</param>
        /// <param name="materialId">glTF material index</param>
        /// <param name="materialExport">Material converter</param>
        /// <returns>True if converting and adding material was successful, false otherwise</returns>
        bool AddMaterial(Material uMaterial, out int materialId, IMaterialExport materialExport);

        /// <summary>
        /// Adds an ImageExport to the glTF and returns the resulting image index
        /// </summary>
        /// <param name="imageExport">Image to be exported</param>
        /// <returns>glTF image index</returns>
        int AddImage(ImageExportBase imageExport);

        /// <summary>
        /// Creates a glTF texture from with a given image index
        /// </summary>
        /// <param name="imageId">glTF image index returned by <see cref="AddImage"/></param>
        /// <param name="samplerId">glTF sampler index returned by <see cref="AddSampler"/></param>
        /// <returns>glTF texture index</returns>
        int AddTexture(int imageId, int samplerId);

        /// <summary>
        /// Creates a glTF sampler based on Unity filter and wrap settings
        /// </summary>
        /// <param name="filterMode">Texture filter mode</param>
        /// <param name="wrapModeU">Texture wrap mode in U direction</param>
        /// <param name="wrapModeV">Texture wrap mode in V direction</param>
        /// <returns>glTF sampler index or -1 if no sampler is required</returns>
        int AddSampler(FilterMode filterMode, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV);

        /// <summary>
        /// Creates a glTF camera based on a Unity camera
        /// </summary>
        /// <param name="uCamera">Unity camera</param>
        /// <param name="cameraId">glTF camera index</param>
        /// <returns>True if camera was successfully created, false otherwise</returns>
        bool AddCamera(Camera uCamera, out int cameraId);

        /// <summary>
        /// Creates a glTF light based on a Unity light
        /// Uses the KHR_lights_punctual extension.
        /// </summary>
        /// <param name="uLight">Unity light</param>
        /// <param name="lightId">glTF light index</param>
        /// <returns>True if light was successfully created, false otherwise</returns>
        bool AddLight(Light uLight, out int lightId);

        /// <summary>
        /// Adds a scene to the glTF
        /// </summary>
        /// <param name="nodes">Root level nodes</param>
        /// <param name="name">Name of the scene</param>
        /// <returns>glTF scene index</returns>
        uint AddScene(uint[] nodes, string name = null);

        /// <summary>
        /// Registers the use of a glTF extension
        /// </summary>
        /// <param name="extension">Extension's name</param>
        /// <param name="required">True if extension is required and used. False if it's used only</param>
        void RegisterExtensionUsage(Extension extension, bool required = true);

        /// <summary>
        /// Exports the collected scenes/content as glTF, writes it to a file
        /// and disposes this object.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="path">glTF destination file path</param>
        /// <returns>True if the glTF file was created successfully, false otherwise</returns>
        Task<bool> SaveToFileAndDispose(string path);

        /// <summary>
        /// Exports the collected scenes/content as glTF, writes it to a Stream
        /// and disposes this object. Only works for self-contained glTF-Binary.
        /// After the export this instance cannot be re-used!
        /// </summary>
        /// <param name="stream">glTF destination stream</param>
        /// <returns>True if the glTF file was created successfully, false otherwise</returns>
        Task<bool> SaveToStreamAndDispose(Stream stream);
    }
}
