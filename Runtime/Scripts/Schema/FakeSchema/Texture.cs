namespace GLTFast.FakeSchema
{
    [System.Serializable]
    class Texture : NamedObject
    {
        public TextureExtension extensions;
    }

    [System.Serializable]
    class TextureExtension
    {
        // ReSharper disable InconsistentNaming
        public string KHR_texture_basisu;
#if WEBP
        public string EXT_texture_webp;
#endif
        // ReSharper enable InconsistentNaming
    }
}
