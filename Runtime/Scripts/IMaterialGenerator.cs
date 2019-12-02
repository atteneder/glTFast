using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {
    using Schema;
    public interface IMaterialGenerator {
		UnityEngine.Material GetPbrMetallicRoughnessMaterial(bool doubleSided=false);
        UnityEngine.Material GenerateMaterial( Material gltfMaterial, Schema.Texture[] textures, UnityEngine.Texture2D[] images );
    }
}
