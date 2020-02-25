

namespace GLTFast {
    
    using Schema;
    
    abstract class PrimitiveCreateContextBase {
        public int primtiveIndex;
        public int[] materials;
        public bool needsNormals;
        public bool needsTangents;
        public abstract bool IsCompleted {get;}
        public abstract Primitive? CreatePrimitive();
    }
} 