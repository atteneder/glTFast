using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GLTFast
{
    public class GlbAsset : GltfAsset
    {
        protected override void LoadContentPrimary(DownloadHandler dlh) {
            byte[] results = dlh.data;
            gLTFastInstance.LoadGlb(results,url);
        }
	}
}