// Copyright 2020-2021 Andreas Atteneder
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

namespace GLTFast {
    using Schema;
    public interface IMaterialGenerator {

        /// <summary>
        /// Get fallback material that is assigned to nodes without a material.
        /// </summary>
        /// <returns>fallback material</returns>
        UnityEngine.Material GetDefaultMaterial();

        /// <summary>
        /// Converts a glTF material into a Unity <see cref="Material"/>.
        /// <see cref="gltfMaterial"/> might reference textures, which can be queried from <see cref="gltf"/>
        /// </summary>
        /// <param name="gltfMaterial">Source glTF material</param>
        /// <param name="gltf">Interface to a loaded glTF's resources (e.g. textures)</param>
        /// <returns></returns>
        UnityEngine.Material GenerateMaterial(Material gltfMaterial, IGltfReadable gltf);

        /// <summary>
        /// Is called prior to <seealso cref="GenerateMaterial"/>. The logger should be used
        /// to inform users about incidents of arbitrary severity (error,warning or info)
        /// during material generation.
        /// </summary>
        /// <param name="logger"></param>
        void SetLogger(ICodeLogger logger);
    }
}
