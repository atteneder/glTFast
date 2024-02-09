// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using GLTFast.Schema;
using Unity.Collections;
using UnityEngine;
using Material = UnityEngine.Material;

namespace GLTFast
{

    /// <inheritdoc />
    /// <typeparam name="TRoot">glTF root (de-serialized glTF JSON) class type</typeparam>
    public interface IGltfReadable<out TRoot> : IGltfReadable
        where TRoot : RootBase
    {
        /// <summary>
        /// Get source root (de-serialized glTF JSON).
        /// This is intended for read-only access. Changes might corrupt data
        /// and break subsequent scene instantiation.
        /// </summary>
        /// <returns>De-serialized glTF root object</returns>
        TRoot GetSourceRoot();
    }

    /// <summary>
    /// Provides read-only access to a glTF (schema and imported Unity resources)
    /// </summary>
    public interface IGltfReadable
    {

        /// <summary>
        /// Number of materials
        /// </summary>
        int MaterialCount { get; }

        /// <summary>
        /// Number of images
        /// </summary>
        int ImageCount { get; }

        /// <summary>
        /// Number of textures
        /// </summary>
        int TextureCount { get; }

        /// <summary>
        /// Get a Unity Material by its glTF material index
        /// </summary>
        /// <param name="index">glTF material index</param>
        /// <returns>Corresponding Unity Material</returns>
        Material GetMaterial(int index = 0);

        /// <summary>
        /// Returns a fallback material to be used when no material was
        /// assigned (provided by the <see cref="Materials.IMaterialGenerator"/>)
        /// </summary>
        /// <returns>Default material</returns>
        Material GetDefaultMaterial();

        /// <summary>
        /// Get texture by glTF image index
        /// </summary>
        /// <param name="index">glTF image index</param>
        /// <returns>Loaded Unity texture</returns>
        Texture2D GetImage(int index = 0);

        /// <summary>
        /// Get texture by glTF texture index
        /// </summary>
        /// <param name="index">glTF texture index</param>
        /// <returns>Loaded Unity texture</returns>
        Texture2D GetTexture(int index = 0);

        /// <summary>
        /// Evaluates if the texture's vertical orientation conforms to Unity's default.
        /// If it's not aligned (=true; =flipped), the texture has to be applied mirrored vertically.
        /// </summary>
        /// <param name="index">glTF texture index</param>
        /// <returns>True if the vertical orientation is flipped, false otherwise</returns>
        bool IsTextureYFlipped(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) camera
        /// </summary>
        /// <param name="index">glTF camera index</param>
        /// <returns>De-serialized glTF camera</returns>
        CameraBase GetSourceCamera(uint index);

        /// <summary>
        /// Get source (de-serialized glTF) material
        /// </summary>
        /// <param name="index">glTF material index</param>
        /// <returns>De-serialized glTF material</returns>
        MaterialBase GetSourceMaterial(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) node
        /// </summary>
        /// <param name="index">glTF node index</param>
        /// <returns>De-serialized glTF node</returns>
        NodeBase GetSourceNode(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) scene
        /// </summary>
        /// <param name="index">glTF scene index</param>
        /// <returns>De-serialized glTF scene</returns>
        Scene GetSourceScene(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) texture
        /// </summary>
        /// <param name="index">glTF texture index</param>
        /// <returns>De-serialized glTF texture</returns>
        TextureBase GetSourceTexture(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) image
        /// </summary>
        /// <param name="index">glTF image index</param>
        /// <returns>De-serialized glTF image</returns>
        Image GetSourceImage(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) light
        /// </summary>
        /// <param name="index">glTF light index</param>
        /// <returns>De-serialized glTF light</returns>
        LightPunctual GetSourceLightPunctual(uint index);

        /// <summary>
        /// Returns an array of inverse bone matrices representing a skin's
        /// bind pose suitable for use with UnityEngine.Mesh.bindposes by glTF
        /// skin index.
        /// </summary>
        /// <param name="skinId">glTF skin index</param>
        /// <returns>Corresponding bind poses</returns>
        Matrix4x4[] GetBindPoses(int skinId);

        /// <summary>
        /// Creates a generic byte-array view into an accessor.
        /// Only available during loading phase as underlying buffers are disposed right afterwards.
        /// </summary>
        /// <param name="accessorIndex">glTF accessor index</param>
        /// <returns>Valid byte-slice view into accessor's data if parameter was correct and buffers are available.
        /// Zero-length slice otherwise.</returns>
        NativeSlice<byte> GetAccessor(int accessorIndex);
    }
}
