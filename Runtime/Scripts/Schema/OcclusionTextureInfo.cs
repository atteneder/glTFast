namespace GLTFast.Schema{ 
    [System.Serializable]
    public class OcclusionTextureInfo : TextureInfo {

        /// <summary>
        /// A scalar multiplier controlling the amount of occlusion applied.
        /// A value of 0.0 means no occlusion.
        /// A value of 1.0 means full occlusion.
        /// This value is ignored if the corresponding texture is not specified.
        /// This value is linear.
        /// <minimum>0.0</minimum>
        /// <maximum>1.0</maximum>
        /// </summary>
        public float strength = 1.0f;
    }
}