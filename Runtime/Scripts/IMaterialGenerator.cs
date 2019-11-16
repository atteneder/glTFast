using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {
    using Schema;
    public interface IMaterialGenerator {
		UnityEngine.Material GetPbrMetallicRoughnessMaterial();
        UnityEngine.Material GenerateMaterial(
            Material gltfMaterial,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref UnityEngine.Texture2D[] images
            );
    }
}
