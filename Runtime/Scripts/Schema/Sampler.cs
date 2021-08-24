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

namespace GLTFast.Schema
{
    [System.Serializable]
    public class Sampler : RootChild
    {

        /// <summary>
        /// Magnification filter mode.
        /// </summary>
        public enum MagFilterMode
        {
            None = 0,
            Nearest = 9728,
            Linear = 9729,
        }
    
        /// <summary>
        /// Minification filter mode.
        /// </summary>
        public enum MinFilterMode
        {
            None = 0,
            Nearest = 9728,
            Linear = 9729,
            NearestMipmapNearest = 9984,
            LinearMipmapNearest = 9985,
            NearestMipmapLinear = 9986,
            LinearMipmapLinear = 9987
        }
    
        /// <summary>
        /// Texture wrap mode.
        /// </summary>
        public enum WrapMode
        {
            None = 0,
            ClampToEdge = 33071,
            MirroredRepeat = 33648,
            Repeat = 10497
        }

        /// <summary>
        /// Magnification filter.
        /// Valid values correspond to WebGL enums: `9728` (NEAREST) and `9729` (LINEAR).
        /// </summary>
        public MagFilterMode magFilter = MagFilterMode.Linear;

        /// <summary>
        /// Minification filter. All valid values correspond to WebGL enums.
        /// </summary>
        public MinFilterMode minFilter = MinFilterMode.NearestMipmapLinear;

        /// <summary>
        /// s wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        public WrapMode wrapS = WrapMode.Repeat;

        /// <summary>
        /// t wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        public WrapMode wrapT = WrapMode.Repeat;

        public TextureWrapMode wrapU {
            get {
                return ConvertWrapMode((WrapMode)wrapS);
            }
        }

        public TextureWrapMode wrapV {
            get {
                return ConvertWrapMode((WrapMode)wrapT);
            }
        }

        public FilterMode filterMode {
            get {
                switch(minFilter) {
                case MinFilterMode.LinearMipmapLinear:
                    return FilterMode.Trilinear;
                case MinFilterMode.Nearest:
                case MinFilterMode.NearestMipmapNearest:
                case MinFilterMode.NearestMipmapLinear: // incorrect mip-map filtering in this case!
                    return FilterMode.Point;
                }
                switch(magFilter) {
                case MagFilterMode.Nearest:
                    return FilterMode.Point;
                default:
                    return FilterMode.Bilinear;
                }
            }
        }

        static TextureWrapMode ConvertWrapMode(WrapMode wrapMode) {
            switch(wrapMode) {
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

        public void Apply(Texture2D image) {
            if (image == null) return;
            image.wrapModeU = wrapU;
            image.wrapModeV = wrapV;
            image.filterMode = filterMode;
        }
    }
}