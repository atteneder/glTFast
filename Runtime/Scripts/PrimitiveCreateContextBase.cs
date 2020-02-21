

namespace GLTFast {
    
    using Schema;
    
    abstract class PrimitiveCreateContextBase {
        public int primtiveIndex;
        public int[] materials;
        public abstract bool IsCompleted {get;}
        public abstract Primitive? CreatePrimitive();
    }
} 