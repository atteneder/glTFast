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
    /// Reference to a texture.
    /// </summary>
    [System.Serializable]
    public class TextureInfo
    {

        /// <summary>
        /// The index of the texture.
        /// </summary>
        public int index = -1;

        /// <summary>
        /// This integer value is used to construct a string in the format
        /// TEXCOORD_&lt;set index&gt; which is a reference to a key in
        /// mesh.primitives.attributes (e.g. A value of 0 corresponds to TEXCOORD_0).
        /// </summary>
        public int texCoord;

        /// <inheritdoc cref="TextureInfoExtension"/>
        public TextureInfoExtension extensions;

        internal void GltfSerializeTextureInfo(JsonWriter writer)
        {
            if (index >= 0)
            {
                writer.AddProperty("index", index);
            }
            if (texCoord > 0)
            {
                writer.AddProperty("texCoord", texCoord);
            }

            if (extensions != null)
            {
                writer.AddProperty("extensions");
                extensions.GltfSerialize(writer);
            }
        }

        internal virtual void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeTextureInfo(writer);
            writer.Close();
        }
    }
}
