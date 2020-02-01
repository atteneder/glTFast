using UnityEngine.Networking;

namespace GLTFast
{
    public class GlbAsset : GltfAsset
    {
        protected override void LoadContentPrimary(GLTFast gLTFastInstance, DownloadHandler dlh, string url) {
            byte[] results = dlh.data;
            gLTFastInstance.LoadGlb(results,url);
        }
	}
}