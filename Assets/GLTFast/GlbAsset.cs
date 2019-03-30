using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GLTFast
{
    public class GlbAsset : GltfAsset
    {
        protected override bool LoadContentPrimary(DownloadHandler dlh) {
            byte[] results = dlh.data;
            return gLTFastInstance.LoadGlb(results,url);
        }
	}
}