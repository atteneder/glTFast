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

    /// <inheritdoc cref="Extension.TextureTransform"/>
    [System.Serializable]
    public class TextureTransform
    {

        /// <summary>
        /// The offset of the UV coordinate origin as a factor of the texture dimensions.
        /// </summary>
        public float[] offset = { 0, 0 };

        /// <summary>
        /// Rotate the UVs by this many radians counter-clockwise around the origin. This is equivalent to a similar rotation of the image clockwise.
        /// </summary>
        public float rotation;

        /// <summary>
        /// The scale factor applied to the components of the UV coordinates.
        /// </summary>
        public float[] scale = { 1, 1 };

        /// <summary>
        /// Overrides the textureInfo texCoord value if supplied, and if this extension is supported.
        /// </summary>
        public int texCoord = -1;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (offset != null)
            {
                writer.AddArrayProperty("offset", offset);
            }
            if (scale != null)
            {
                writer.AddArrayProperty("scale", scale);
            }
            if (rotation != 0)
            {
                writer.AddProperty("rotation", rotation);
            }
            if (texCoord >= 0)
            {
                writer.AddProperty("texCoord", texCoord);
            }
            writer.Close();
        }
    }
}
