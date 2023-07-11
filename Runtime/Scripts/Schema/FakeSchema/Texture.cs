namespace GLTFast.FakeSchema
{
    [System.Serializable]
    class Texture : NamedObject
    {
        public TextureExtension extensions;
    }

    [System.Serializable]
    class TextureExtension : NamedObject
    {
        // ReSharper disable InconsistentNaming
        public string KHR_texture_basisu;
        public string EXT_texture_webp;
        // ReSharper enable InconsistentNaming
    }
}
