// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Objects;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Camera = Unity.Cloud.Gltfast.Objects.Camera;
using Material = UnityEngine.Material;
using Mesh = Unity.Cloud.Gltfast.Objects.Mesh;
using Texture = Unity.Cloud.Gltfast.Objects.Texture;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Provides read-only access to a glTF (glTF objects and imported Unity resources)
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public interface IGltfReadable : IMaterialProvider
    {
        /// <summary>
        /// De-serialized glTF JSON object.
        /// This is intended for read-only access. Changes might corrupt data
        /// and break subsequent scene instantiation.
        /// </summary>
        Root Root { get; }

        /// <summary>
        /// Number of materials
        /// </summary>
        int MaterialCount { get; }

        /// <summary>
        /// Number of images
        /// </summary>
        /// <seealso cref="TextureCount"/>
        /// <seealso cref="GetTexture"/>
        [Obsolete("Use TextureCount and GetTexture instead. This property will be removed in future releases.")]
        int ImageCount { get; }

        /// <summary>
        /// Number of textures
        /// </summary>
        /// <seealso cref="GetTexture"/>
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
        /// Get imported glTF image by index.
        /// <b>Warning:</b> Only works temporarily during loading phase!
        /// It's recommended to work with <see cref="GetTexture"/> instead.
        /// </summary>
        /// <param name="index">glTF image index</param>
        /// <returns>Loaded Unity texture</returns>
        /// <seealso cref="GetTexture"/>
        [Obsolete("Use GetTexture instead. This method will be removed in future releases.")]
        Texture2D GetImage(int index = 0);

        /// <summary>
        /// Get imported glTF texture by index.
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
        Camera GetSourceCamera(uint index);

        /// <summary>
        /// Get source (de-serialized glTF) material
        /// </summary>
        /// <param name="index">glTF material index</param>
        /// <returns>De-serialized glTF material</returns>
        Unity.Cloud.Gltfast.Objects.Material GetSourceMaterial(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) mesh.
        /// </summary>
        /// <param name="meshIndex">glTF mesh index.</param>
        /// <returns>De-serialized glTF mesh.</returns>
        Mesh GetSourceMesh(int meshIndex);

        /// <summary>
        /// Get source (de-serialized glTF) mesh primitive
        /// </summary>
        /// <param name="meshIndex">glTF mesh index.</param>
        /// <param name="primitiveIndex">glTF primitive index within mesh.</param>
        /// <returns>De-serialized glTF mesh primitive</returns>
        MeshPrimitive GetSourceMeshPrimitive(int meshIndex, int primitiveIndex);

        /// <summary>
        /// Get source (de-serialized glTF) node
        /// </summary>
        /// <param name="index">glTF node index</param>
        /// <returns>De-serialized glTF node</returns>
        Node GetSourceNode(int index = 0);

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
        Texture GetSourceTexture(int index = 0);

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
    }
}
