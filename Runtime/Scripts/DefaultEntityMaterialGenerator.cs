using GLTFast.Schema;
using System.Collections;
using System.Collections.Generic;
using Unity.Rendering;
using UnityEngine;

namespace GLTFast
{
    public class DefaultEntityMaterialGenerator : IMaterialGenerator
    {
        UnityEngine.Material material;

        public UnityEngine.Material GetPbrMetallicRoughnessMaterial(bool doubleSided = false)
        {
            return null;
        }

        public UnityEngine.Material GenerateMaterial(Schema.Material gltfMaterial, ref Schema.Texture[] textures, ref Image[] schemaImages, ref Dictionary<int, Texture2D>[] imageVariants)
        {
            return null;
        }
    }
}