using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GLTFast
{
    public class GlbAsset : GltfAsset
    {
        protected override IEnumerator LoadContent( DownloadHandler dlh ) {
            byte[] results = dlh.data;
            gLTFastInstance = new GLTFast();
            var success = gLTFastInstance.LoadGlb(results,transform);
            if(onLoadComplete!=null) {
                onLoadComplete(success);
            }
            yield break;
        }
	}
}