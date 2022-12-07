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

namespace GLTFast.Schema
{

    /// <summary>
    /// TextureInfo extensions
    /// </summary>
    [System.Serializable]
    public class TextureInfoExtension
    {

        /// <inheritdoc cref="Extension.TextureTransform"/>
        // ReSharper disable once InconsistentNaming
        public TextureTransform KHR_texture_transform;

        internal void GltfSerialize(JsonWriter writer)
        {
            if (KHR_texture_transform != null)
            {
                writer.AddObject();
                writer.AddProperty("KHR_texture_transform");
                KHR_texture_transform.GltfSerialize(writer);
                writer.Close();
            }
        }
    }
}
