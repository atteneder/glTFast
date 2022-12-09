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

using UnityEngine;

namespace GLTFast
{

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
        /// Get source root (de-serialized glTF JSON).
        /// This is intended for read-only access. Changes might corrupt data
        /// and break subsequent scene instantiation.
        /// </summary>
        /// <returns>De-serialized glTF root object</returns>
        Schema.Root GetSourceRoot();

        /// <summary>
        /// Get source (de-serialized glTF) camera
        /// </summary>
        /// <param name="index">glTF camera index</param>
        /// <returns>De-serialized glTF camera</returns>
        Schema.Camera GetSourceCamera(uint index);

        /// <summary>
        /// Get source (de-serialized glTF) material
        /// </summary>
        /// <param name="index">glTF material index</param>
        /// <returns>De-serialized glTF material</returns>
        Schema.Material GetSourceMaterial(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) node
        /// </summary>
        /// <param name="index">glTF node index</param>
        /// <returns>De-serialized glTF node</returns>
        Schema.Node GetSourceNode(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) scene
        /// </summary>
        /// <param name="index">glTF scene index</param>
        /// <returns>De-serialized glTF scene</returns>
        Schema.Scene GetSourceScene(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) texture
        /// </summary>
        /// <param name="index">glTF texture index</param>
        /// <returns>De-serialized glTF texture</returns>
        Schema.Texture GetSourceTexture(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) image
        /// </summary>
        /// <param name="index">glTF image index</param>
        /// <returns>De-serialized glTF image</returns>
        Schema.Image GetSourceImage(int index = 0);

        /// <summary>
        /// Get source (de-serialized glTF) light
        /// </summary>
        /// <param name="index">glTF light index</param>
        /// <returns>De-serialized glTF light</returns>
        Schema.LightPunctual GetSourceLightPunctual(uint index);

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
