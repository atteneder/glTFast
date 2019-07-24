using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {
    using Schema;
    public interface IMaterialGenerator {
		UnityEngine.Material GetDefaultMaterial();
        UnityEngine.Material GenerateMaterial( Material gltfMaterial, Schema.Texture[] textures, UnityEngine.Texture2D[] images, List<UnityEngine.Object> additionalResources );
    }
}
