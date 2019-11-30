namespace GLTFast.Schema {

    [System.Serializable]
    public class TextureInfo {

        /// <summary>
        /// The index of the texture.
        /// </summary>
        public int index = -1;

        /// <summary>
        /// This integer value is used to construct a string in the format
        /// TEXCOORD_<set index> which is a reference to a key in
        /// mesh.primitives.attributes (e.g. A value of 0 corresponds to TEXCOORD_0).
        /// </summary>
        public int texCoord = 0;

        public TextureInfoExtension extensions;
    }
}
