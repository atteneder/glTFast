using UnityEngine;

namespace GLTFast.Schema {

    [System.Serializable]
    public class ImageKtx2 {
        public uint faceCount;
        public BufferSlice[] levels;
        public uint pixelHeight;
        public uint pixelWidth;
        public uint supercompressionScheme;
    }
}
