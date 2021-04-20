// Copyright 2020-2021 Andreas Atteneder
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

using System;
using System.Threading.Tasks;
using GLTFast.Loading;
using UnityEngine;

namespace GLTFast
{
    public class GltfBoundsAsset : GltfAsset {

        [Tooltip("Create a box collider encapsulating the glTF asset")]
        public bool createBoxCollider = true;

        [NonSerialized]
        public Bounds bounds;

        public override async Task<bool> Load( string url, IDownloadProvider downloadProvider=null, IDeferAgent deferAgent=null, IMaterialGenerator materialGenerator=null ) {
            var success = await base.Load(url, downloadProvider, deferAgent, materialGenerator);
            if(success) {
                var insta = new GameObjectBoundsInstantiator(gLTFastInstance,transform);
                // Auto-Instantiate
                gLTFastInstance.InstantiateGltf(insta);

                var sceneBounds = insta.CalculateBounds();
                if (sceneBounds.HasValue) {
                    bounds = sceneBounds.Value;
                    if (createBoxCollider) {
#if UNITY_PHYSICS
                        var boxCollider = gameObject.AddComponent<BoxCollider>();
                        boxCollider.center = bounds.center;
                        boxCollider.size = bounds.size;
#else
                        Debug.LogError("GltfBoundsAsset requires the built-in Physics package to be enabled (in the Package Manager)");
#endif
                    }
                }
            }
            return success;
        }
    }
}
