#if DEBUG

using System.Collections.Generic;

namespace GLTFast
{
    class VertexAttributeConfig
    {
        public HashSet<int> meshIndices;

        public VertexAttributeConfig() {
            meshIndices = new HashSet<int>();
        }
    }
}
#endif
