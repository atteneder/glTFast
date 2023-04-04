using UnityEngine;

namespace GLTFast
{
    public class NonReusableTexture : IDisposableTexture
    {
        public NonReusableTexture(Texture2D texture)
        {
            Texture = texture;
        }

        public void Dispose()
        {
            GltfImport.SafeDestroy(Texture);
        }

        public Texture2D Texture { get; }
    }
    
    internal static class DisposableTextureExtensions
    {
        internal static IDisposableTexture ToDisposableTexture(this Texture2D texture2D) =>
            new NonReusableTexture(texture2D);
    }
}