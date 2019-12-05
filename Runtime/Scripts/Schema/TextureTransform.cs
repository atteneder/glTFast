namespace GLTFast.Schema {

    [System.Serializable]
    public class TextureTransform {

        /// <summary>
        /// The offset of the UV coordinate origin as a factor of the texture dimensions.
        /// </summary>
        public float[] offset = {0,0};

        /// <summary>
        /// Rotate the UVs by this many radians counter-clockwise around the origin. This is equivalent to a similar rotation of the image clockwise.
        /// </summary>
        public float rotation = 0;

        /// <summary>
        /// The scale factor applied to the components of the UV coordinates.
        /// </summary>
        public float[] scale = {1,1};

        /// <summary>
        /// Overrides the textureInfo texCoord value if supplied, and if this extension is supported.
        /// </summary>
        public int texCoord = 0;
    }
}
