

namespace GLTFast {
    
    using Schema;
    
    abstract class PrimitiveCreateContextBase {
        public int primtiveIndex;
        public MeshPrimitive primitive;
        public abstract bool IsCompleted {get;}
        public abstract Primitive? CreatePrimitive();
    }
} 