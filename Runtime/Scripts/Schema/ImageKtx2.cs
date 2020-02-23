using UnityEngine;

namespace GLTFast.Schema {

    [System.Serializable]
    [System.Obsolete("To be replaced by KHR_texture_basisu")]
    public class ImageKtx2 {
        public uint faceCount;
        // public BufferSlice levels; // Obsolete KHR_texture_cttf way
        // public BufferSlice[] levels;
        public uint pixelHeight;
        public uint pixelWidth;
        public uint supercompressionScheme;
    }
}
