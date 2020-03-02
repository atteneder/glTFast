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
        public int magFilter = (int) MagFilterMode.Linear;

        /// <summary>
        /// Minification filter. All valid values correspond to WebGL enums.
        /// </summary>
        public int minFilter = (int) MinFilterMode.NearestMipmapLinear;

        /// <summary>
        /// s wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        public int wrapS = (int) WrapMode.Repeat;

        /// <summary>
        /// t wrapping mode.  All valid values correspond to WebGL enums.
        /// </summary>
        public int wrapT = (int) WrapMode.Repeat;

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
    }
}