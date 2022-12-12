using System.Collections.Generic;
using GLTFast.Loading;

namespace GLTFast.Editor
{
    public interface IEditorDownloadProvider : IDownloadProvider
    {
        public List<GltfAssetDependency> assetDependencies { get; set; }
    }
}