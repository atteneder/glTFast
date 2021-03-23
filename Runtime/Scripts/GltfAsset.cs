﻿// Copyright 2020-2021 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.IO;
using System.Threading.Tasks;
using GLTFast.Loading;
using UnityEngine;

namespace GLTFast
{
    public class GltfAsset : GltfAssetBase
    {
        [Tooltip("URL to load the glTF from.")]
        public string url;
        
        [Tooltip("Automatically load at start.")]
        public bool loadOnStartup = true;
        
        [Tooltip("If checked, url is treated as relative StreamingAssets path.")]
        public bool streamingAsset = false;

        public string FullUrl => streamingAsset
            ? Path.Combine(Application.streamingAssetsPath, url)
            : url;

        protected virtual async void Start() {
            if(loadOnStartup && !string.IsNullOrEmpty(url)) {
                // Automatic load on startup
                await Load(FullUrl);
            }
        }

        public override async Task<bool> Load( string url, IDownloadProvider downloadProvider=null, IDeferAgent deferAgent=null, IMaterialGenerator materialGenerator=null, bool disposeData=true ) {
            var success = await base.Load(url, downloadProvider, deferAgent, materialGenerator, disposeData);
            if(success) {
                if (deferAgent != null) await deferAgent.BreakPoint();
                // Auto-Instantiate
                gLTFastInstance.InstantiateGltf(transform);
            }
            return success;
        }
    }
}
