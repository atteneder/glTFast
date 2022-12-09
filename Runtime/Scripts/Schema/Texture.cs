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

using System;

namespace GLTFast.Schema
{

    /// <summary>
    /// A texture is defined by an image and a sampler.
    /// </summary>
    [Serializable]
    public class Texture : NamedObject
    {

        /// <summary>
        /// The index of the sampler used by this texture.
        /// </summary>
        public int sampler = -1;

        /// <summary>
        /// The index of the image used by this texture.
        /// </summary>
        public int source = -1;

        /// <inheritdoc cref="TextureExtension"/>
        public TextureExtension extensions;

        /// <summary>
        /// Retrieves the final image index.
        /// </summary>
        /// <returns>Final image index</returns>
        public int GetImageIndex()
        {
            if (extensions != null)
            {
                if (extensions.KHR_texture_basisu != null && extensions.KHR_texture_basisu.source >= 0)
                {
                    return extensions.KHR_texture_basisu.source;
                }
            }
            return source;
        }

        /// <summary>
        /// True, if the texture is of the KTX format.
        /// </summary>
        public bool IsKtx => extensions?.KHR_texture_basisu != null;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeRoot(writer);
            if (source >= 0)
            {
                writer.AddProperty("source", source);
            }
            if (sampler >= 0)
            {
                writer.AddProperty("sampler", sampler);
            }
            if (extensions != null)
            {
                writer.AddProperty("extensions");
                extensions.GltfSerialize(writer);
            }
            writer.Close();
        }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((Texture)obj);
        }

        bool Equals(Texture other)
        {
            return source == other.source
                && sampler == other.sampler
                && (
                    extensions == null && other.extensions == null
                    || (extensions != null && extensions.Equals(other.extensions))
                );
        }

        /// <summary>
        /// Default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
#if NET_STANDARD
            return HashCode.Combine(source, sampler, extensions.GetHashCode());
#else
            var hash = 17;
            hash = hash * 31 + source;
            hash = hash * 31 + sampler;
            hash = hash * 31 + extensions.GetHashCode();
            return hash;
#endif
        }
    }
}
