namespace GLTFast
{
    public class GlbAsset : GltfAsset
    {
        protected override bool isGltfBinary {
            get { return true; }
        }
	}
}