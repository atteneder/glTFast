namespace GLTFast.Schema {
    
    [System.Serializable]
    public class Root
    {
        /// <summary>
        /// Names of glTF extensions used somewhere in this asset.
        /// </summary>
        public string[] extensionsUsed;

        /// <summary>
        /// Names of glTF extensions required to properly load this asset.
        /// </summary>
        public string[] extensionsRequired;

        /// <summary>
        /// An array of accessors. An accessor is a typed view into a bufferView.
        /// </summary>
        public Accessor[] accessors;

        /// <summary>
        /// An array of keyframe animations.
        /// </summary>
        //public List<Animation> Animations;

        /// <summary>
        /// Metadata about the glTF asset.
        /// </summary>
        public Asset asset;

        /// <summary>
        /// An array of buffers. A buffer points to binary geometry, animation, or skins.
        /// </summary>
        public Buffer[] buffers;

        /// <summary>
        /// An array of bufferViews.
        /// A bufferView is a view into a buffer generally representing a subset of the buffer.
        /// </summary>
        public BufferView[] bufferViews;

        /// <summary>
        /// An array of cameras. A camera defines a projection matrix.
        /// </summary>
        //public List<Camera> Cameras;

        /// <summary>
        /// An array of images. An image defines data used to create a texture.
        /// </summary>
        public Image[] images;

        /// <summary>
        /// An array of materials. A material defines the appearance of a primitive.
        /// </summary>
        public Material[] materials;

        /// <summary>
        /// An array of meshes. A mesh is a set of primitives to be rendered.
        /// </summary>
        public Mesh[] meshes;

        /// <summary>
        /// An array of nodes.
        /// </summary>
        public Node[] nodes;

        /// <summary>
        /// An array of samplers. A sampler contains properties for texture filtering and wrapping modes.
        /// </summary>
        public Sampler[] samplers;

        /// <summary>
        /// The index of the default scene.
        /// </summary>
        //public SceneId Scene;

        /// <summary>
        /// An array of scenes.
        /// </summary>
        public Scene[] scenes;

        /// <summary>
        /// An array of skins. A skin is defined by joints and matrices.
        /// </summary>
        //public List<Skin> Skins;

        /// <summary>
        /// An array of textures.
        /// </summary>
        public Texture[] textures;

		public bool IsAccessorInterleaved( int accessorIndex ) {
			var accessor = accessors[accessorIndex];
			var bufferView = bufferViews[accessor.bufferView];
			if (bufferView.byteStride < 0) return false;
			int elementSize = Accessor.GetAccessorAttriuteTypeLength(accessor.typeEnum) * Accessor.GetAccessorComponentTypeLength(accessor.componentType);
			return bufferView.byteStride > elementSize;
		}
    }
}