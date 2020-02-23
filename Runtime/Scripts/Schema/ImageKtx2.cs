using UnityEngine;

namespace GLTFast.Schema {

    [System.Serializable]
    public class ImageKtx2 {
        public uint faceCount;
        // public BufferSlice levels; // Obsolete KHR_texture_cttf way
        // public BufferSlice[] levels;
        public uint pixelHeight;
        public uint pixelWidth;
        public uint supercompressionScheme;
    }
}
