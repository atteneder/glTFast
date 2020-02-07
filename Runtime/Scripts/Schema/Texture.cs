namespace GLTFast.Schema {

    [System.Serializable]
    public class Texture {
        /// <summary>
        /// The index of the sampler used by this texture.
        /// </summary>
        public int sampler = -1;

        /// <summary>
        /// The index of the image used by this texture.
        /// </summary>
        public int source = -1;

        public TextureExtension extensions;

        public int GetImageIndex() {
            if(extensions!=null && extensions.KHR_texture_basisu!=null) {
                return extensions.KHR_texture_basisu.source;
            }
            return source;
        }

        public bool isKtx {
            get {
                return extensions!=null && extensions.KHR_texture_basisu!=null;
            }
        }
    }
}
