using UnityEngine;

namespace GLTFast.FakeSchema {

    /// <summary>
    /// The material appearance of a primitive.
    /// </summary>
    [System.Serializable]
    public class Material : RootChild {
        public MaterialExtension extensions = null;
    }
}