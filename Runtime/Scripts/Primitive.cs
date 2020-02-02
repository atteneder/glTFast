namespace GLTFast {
    
    struct Primitive {
        public UnityEngine.Mesh mesh;
        public int materialIndex;

        public Primitive( UnityEngine.Mesh mesh, int materialIndex ) {
            this.mesh = mesh;
            this.materialIndex = materialIndex;
        }
    }
} 