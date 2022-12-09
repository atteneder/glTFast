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
using UnityEngine;

namespace GLTFast.Schema
{
    /// <summary>
    /// Texture sampler properties for filtering and wrapping modes.
    /// </summary>
    [Serializable]
    public class Sampler : NamedObject
    {

        /// <summary>
        /// Magnification filter mode.
        /// </summary>
        public enum MagFilterMode
        {
            /// <summary>No value</summary>
            None = 0,
            /// <summary>Nearest pixel sampling</summary>
            Nearest = 9728,
            /// <summary>Linear pixel interpolation sampling</summary>
            Linear = 9729,
        }

        /// <summary>
        /// Minification filter mode.
        /// </summary>
        public enum MinFilterMode
        {
            /// <summary>No value</summary>
            None = 0,
            /// <summary>Nearest pixel sampling</summary>
            Nearest = 9728,
            /// <summary>Linear pixel interpolation sampling</summary>
            Linear = 9729,
            /// <summary>Nearest pixel and nearest mipmap sampling</summary>
            NearestMipmapNearest = 9984,
            /// <summary>Linear pixel interpolation and nearest mipmap sampling</summary>
            LinearMipmapNearest = 9985,
            /// <summary>Nearest pixel and linear mipmap interpolation sampling</summary>
            NearestMipmapLinear = 9986,
            /// <summary>Linear pixel interpolation and linear mipmap interpolation sampling</summary>
            LinearMipmapLinear = 9987
        }

        /// <summary>
        /// Texture wrap mode.
        /// </summary>
        public enum WrapMode
        {
            /// <summary>No value</summary>
            None = 0,
            /// <summary>Clamp to edge</summary>
            ClampToEdge = 33071,
            /// <summary>Mirrored repeat</summary>
            MirroredRepeat = 33648,
            /// <summary>Repeat</summary>
            Repeat = 10497
        }

        /// <summary>
        /// Magnification filter.
        /// Valid values correspond to WebGL enums: `9728` (NEAREST) and `9729` (LINEAR).
        /// </summary>
        public MagFilterMode magFilter = MagFilterMode.None;

        /// <summary>
        /// Minification filter. All valid values correspond to WebGL enums.
        /// </summary>
        public MinFilterMode minFilter = MinFilterMode.None;

        /// <summary>
        /// s wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        public WrapMode wrapS = WrapMode.Repeat;

        /// <summary>
        /// t wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        public WrapMode wrapT = WrapMode.Repeat;

        /// <summary>
        /// Unity filter mode, derived from glTF's
        /// <see cref="minFilter"/> and <see cref="magFilter"/>.
        /// </summary>
        public FilterMode FilterMode => ConvertFilterMode(minFilter, magFilter);

        /// <summary>
        /// Unity texture wrap mode (horizontal), derived from glTF's
        /// <see cref="wrapS"/> value.
        /// </summary>
        public TextureWrapMode WrapU => ConvertWrapMode(wrapS);

        /// <summary>
        /// Unity texture wrap mode (vertical), derived from glTF's
        /// <see cref="wrapT"/> value.
        /// </summary>
        public TextureWrapMode WrapV => ConvertWrapMode(wrapT);

        static FilterMode ConvertFilterMode(MinFilterMode minFilterToConvert, MagFilterMode magFilterToConvert)
        {
            switch (minFilterToConvert)
            {
                case MinFilterMode.LinearMipmapLinear:
                    return FilterMode.Trilinear;
                case MinFilterMode.Nearest:
                case MinFilterMode.NearestMipmapNearest:
                case MinFilterMode.NearestMipmapLinear: // incorrect mip-map filtering in this case!
                    return FilterMode.Point;
            }
            switch (magFilterToConvert)
            {
                case MagFilterMode.Nearest:
                    return FilterMode.Point;
                default:
                    return FilterMode.Bilinear;
            }
        }

        static TextureWrapMode ConvertWrapMode(WrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case WrapMode.None:
                case WrapMode.Repeat:
                default:
                    return TextureWrapMode.Repeat;
                case WrapMode.ClampToEdge:
                    return TextureWrapMode.Clamp;
                case WrapMode.MirroredRepeat:
                    return TextureWrapMode.Mirror;
            }
        }

        static WrapMode ConvertWrapMode(TextureWrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case TextureWrapMode.Clamp:
                    return WrapMode.ClampToEdge;
                case TextureWrapMode.Mirror:
                case TextureWrapMode.MirrorOnce:
                    return WrapMode.MirroredRepeat;
                case TextureWrapMode.Repeat:
                default:
                    return WrapMode.Repeat;
            }
        }


        /// <summary>
        /// Parameter-less constructor
        /// </summary>
        public Sampler() { }

        /// <summary>
        /// Constructs a Sampler with filter and wrap modes.
        /// </summary>
        /// <param name="filterMode">Unity texture filter mode</param>
        /// <param name="wrapModeU">Unity texture wrap mode (horizontal)</param>
        /// <param name="wrapModeV">Unity texture wrap mode (vertical)</param>
        public Sampler(FilterMode filterMode, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV)
        {
            switch (filterMode)
            {
                case FilterMode.Point:
                    magFilter = MagFilterMode.Nearest;
                    minFilter = MinFilterMode.Nearest;
                    break;
                case FilterMode.Bilinear:
                    magFilter = MagFilterMode.Linear;
                    minFilter = MinFilterMode.Linear;
                    break;
                case FilterMode.Trilinear:
                    magFilter = MagFilterMode.Linear;
                    minFilter = MinFilterMode.LinearMipmapLinear;
                    break;
            }

            wrapS = ConvertWrapMode(wrapModeU);
            wrapT = ConvertWrapMode(wrapModeV);
        }

        /// <summary>
        /// Applies the Sampler's settings to a Unity texture.
        /// </summary>
        /// <param name="image">Texture to apply the settings to</param>
        /// <param name="defaultMinFilter">Fallback minification filter</param>
        /// <param name="defaultMagFilter">Fallback magnification filter</param>
        public void Apply(Texture2D image,
                          MinFilterMode defaultMinFilter = MinFilterMode.Linear,
                          MagFilterMode defaultMagFilter = MagFilterMode.Linear)
        {
            if (image == null) return;
            image.wrapModeU = WrapU;
            image.wrapModeV = WrapV;

            // Use the default filtering mode for textures that have no such specification in data
            image.filterMode = ConvertFilterMode(
                minFilter == MinFilterMode.None ? defaultMinFilter : minFilter,
                magFilter == MagFilterMode.None ? defaultMagFilter : magFilter
            );
        }

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeRoot(writer);
            // Assuming MagFilterMode.Linear is the project's default, only
            // serialize valid, non-default values
            if (magFilter == MagFilterMode.Nearest)
            {
                writer.AddProperty("magFilter", (int)magFilter);
            }
            // Assuming MinFilterMode.Linear is the project's default, only
            // serialize valid, non-default values
            if (minFilter != MinFilterMode.None && minFilter != MinFilterMode.Linear)
            {
                writer.AddProperty("minFilter", (int)minFilter);
            }
            if (wrapS != WrapMode.None && wrapS != WrapMode.Repeat)
            {
                writer.AddProperty("wrapS", (int)wrapS);
            }
            if (wrapT != WrapMode.None && wrapT != WrapMode.Repeat)
            {
                writer.AddProperty("wrapT", (int)wrapT);
            }
            writer.Close();
        }
    }
}
