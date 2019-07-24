using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {
	public struct GlbBinChunk
	{
		public int start;
		public uint length;
		
		public GlbBinChunk(int start, uint length)
		{
			this.start = start;
			this.length = length;
		}
	}
}
