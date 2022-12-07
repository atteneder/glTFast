using System;

namespace GLTFast
{

    [Serializable]
    struct GltfAssetDependency
    {

        public enum Type
        {
            Unknown,
            Texture,
            Buffer
        }

        public Type type;
        public string originalUri;
        public string assetPath;

        public GltfAssetDependency(string originalUri, Type type = Type.Unknown)
        {
            this.originalUri = originalUri;
            this.type = type;

            assetPath = null;
        }
    }
}
