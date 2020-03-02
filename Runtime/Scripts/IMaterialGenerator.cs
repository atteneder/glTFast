using System.Collections.Generic;

namespace GLTFast {
    using Schema;
    public interface IMaterialGenerator {

        UnityEngine.Material GetPbrMetallicRoughnessMaterial(bool doubleSided=false);
        UnityEngine.Material GenerateMaterial(
            Material gltfMaterial,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,UnityEngine.Texture2D>[] imageVariants
            );
    }
}
