namespace GLTFast.Schema {

    [System.Serializable]
    public class TextureExtension {

        [System.Obsolete("to be replaced by KHR_texture_basisu")]
        public TextureFormat KHR_texture_cttf = null;
        public TextureFormat KHR_texture_basisu = null;
    }

    [System.Serializable]
    public class TextureFormat {
        public int source = -1;
    }
}
