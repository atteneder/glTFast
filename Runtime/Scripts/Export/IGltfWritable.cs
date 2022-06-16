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

using System.IO;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Export {
    
    /// <summary>
    /// Is able to receive asset resources and export them to glTF
    /// </summary>
    public interface IGltfWritable {

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
        void AddMeshToNode(int nodeId, Mesh uMesh, int[] materialIds);

        /// <summary>
        /// Adds a Unity material 
        /// </summary>
        /// <param name="uMaterial">Unity material</param>
        /// <param name="materialId">glTF material index</param>
        /// <param name="materialExport">Material converter</param>
        /// <returns>True if converting and adding material was successful, false otherwise</returns>
        bool AddMaterial(UnityEngine.Material uMaterial, out int materialId, IMaterialExport materialExport);
        
        /// <summary>
        /// Adds an ImageExport to the glTF and returns the resulting image index
        /// </summary>
        /// <param name="imageExport">Image to be exported</param>
        /// <returns>glTF image index</returns>
        int AddImage(ImageExportBase imageExport);

        /// <summary>
        /// Creates a glTF texture from with a given image index
        /// </summary>
        /// <param name="imageId">glTF image index returned by <seealso cref="AddImage"/></param>
        /// <param name="samplerId">glTF sampler index returned by <seealso cref="AddSampler"/></param>
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
