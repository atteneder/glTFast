namespace GLTFast.Schema {

    [System.Serializable]
    public class Node : RootChild {

        /// <summary>
        /// The indices of this node's children.
        /// </summary>
        public uint[] children;

        /// <summary>
        /// The index of the mesh in this node.
        /// </summary>
        public int mesh = -1;

        /// <summary>
        /// A floating-point 4x4 transformation matrix stored in column-major order.
        /// </summary>
        public float[] matrix;

        /// <summary>
        /// The node's unit quaternion rotation in the order (x, y, z, w),
        /// where w is the scalar.
        /// </summary>
        public float[] rotation;

        /// <summary>
        /// The node's non-uniform scale.
        /// </summary>
        public float[] scale;

        /// <summary>
        /// The node's translation.
        /// </summary>
        public float[] translation;

        /// <summary>
        /// The weights of the instantiated Morph Target.
        /// Number of elements must match number of Morph Targets of used mesh.
        /// </summary>
        //public double[] weights;
    }
}