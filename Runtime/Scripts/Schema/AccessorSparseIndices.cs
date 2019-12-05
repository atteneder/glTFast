namespace GLTFast.Schema {

    [System.Serializable]
    public class AccessorSparseIndices {
        /// <summary>
        /// The index of the bufferView with sparse indices.
        /// Referenced bufferView can't have ARRAY_BUFFER or ELEMENT_ARRAY_BUFFER target.
        /// </summary>
        public uint BufferView;

        /// <summary>
        /// The offset relative to the start of the bufferView in bytes. Must be aligned.
        /// <minimum>0</minimum>
        /// </summary>
        public int ByteOffset;

        /// <summary>
        /// The indices data type. Valid values correspond to WebGL enums:
        /// `5121` (UNSIGNED_BYTE)
        /// `5123` (UNSIGNED_SHORT)
        /// `5125` (UNSIGNED_INT)
        /// </summary>
        public GLTFComponentType ComponentType;
    }
}
