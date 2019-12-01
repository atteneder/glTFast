namespace GLTFast.Schema{ 
    [System.Serializable]
    public class NormalTextureInfo : TextureInfo {

        /// <summary>
        /// The scalar multiplier applied to each normal vector of the texture.
        /// This value is ignored if normalTexture is not specified.
        /// This value is linear.
        /// </summary>
        public float scale = 1.0f;
    }
}