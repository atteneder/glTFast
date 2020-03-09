#if KTX_UNITY

namespace GLTFast.Schema {

    [System.Serializable]
    public class ImageExtension {
        [System.Obsolete("To be replaced by KHR_texture_basisu")]
        public ImageKtx2 KHR_image_ktx2;
    }
}
#endif // KTX_UNITY
