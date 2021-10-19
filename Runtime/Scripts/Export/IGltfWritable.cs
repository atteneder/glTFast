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

using UnityEngine;

namespace GLTFast.Export {
    public interface IGltfWritable {

        
        /// <summary>
        /// Registers the use of a glTF extension
        /// </summary>
        /// <param name="extension">Extension's name</param>
        /// <param name="required">True if extension is required and used. False if it's used only</param>
        void RegisterExtensionUsage(Extension extension, bool required = true);
        
        /// <summary>
        /// Adds a Unity Texture to the glTF and returns the resulting image index
        /// </summary>
        /// <param name="uTexture">Unity Texture</param>
        /// <returns>glTF image index</returns>
        int AddImage(Texture uTexture);

        /// <summary>
        /// Creates a glTF texture from with a given image index
        /// </summary>
        /// <param name="imageId">glTF image index returned by <seealso cref="AddImage"/></param>
        /// <returns>glTF texture index</returns>
        int AddTexture(int imageId);
    }
}
