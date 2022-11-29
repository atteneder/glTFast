﻿// Copyright 2020-2022 Andreas Atteneder
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
    using Logging;
    using Materials;

    /// <summary>
    /// Base component for code-less loading of glTF files
    /// Extends <seealso cref="GltfAsset"/> with bounding box calculation
    /// </summary>
    public class GltfBoundsAsset : GltfAsset {

        /// <summary>
        /// If true, a box collider encapsulating the glTF scene is created
        /// (only if the built-in Physics module is enabled).
        /// </summary>
        [Tooltip("If true, a box collider encapsulating the glTF asset is created")]
        public bool createBoxCollider = true;

        /// <summary>
        /// Bounding box of the instantiated glTF scene
        /// </summary>
        [NonSerialized]
        public Bounds bounds;

        
        /// <inheritdoc />
        public override async Task<bool> Load(
            string url,
            IDownloadProvider downloadProvider=null,
            IDeferAgent deferAgent=null,
            IMaterialGenerator materialGenerator=null,
            ICodeLogger logger = null
            )
        {
            importer = new GltfImport(downloadProvider,deferAgent, materialGenerator);
            var success = await importer.Load(url);
            if(success) {
                var insta = (GameObjectBoundsInstantiator) GetDefaultInstantiator(logger);
                // Auto-Instantiate
                if (sceneId>=0) {
                    success = await importer.InstantiateSceneAsync(insta, sceneId);
                    currentSceneId = success ? sceneId : (int?)null;
                } else {
                    success = await importer.InstantiateMainSceneAsync(insta);
                    currentSceneId = importer.defaultSceneIndex;
                }

                sceneInstance = insta.sceneInstance;

                if(success) {
                    SetBounds(insta);
                }
            }
            return success;
        }

        /// <inheritdoc />
        public override async Task<bool> InstantiateScene(int sceneIndex, ICodeLogger logger = null) {
            var instantiator = (GameObjectBoundsInstantiator)GetDefaultInstantiator(logger);
            var success = await base.InstantiateScene(sceneIndex, instantiator);
            currentSceneId = success ? sceneIndex : (int?)null;
            sceneInstance = instantiator.sceneInstance;
            if (success) {
                SetBounds(instantiator);
            }
            return success;
        }

        /// <inheritdoc />
        protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger) {
            return new GameObjectBoundsInstantiator(importer, transform, logger);
        }
        
        void SetBounds(GameObjectBoundsInstantiator insta) {
            var sceneBounds = insta.sceneInstance!=null ? insta.CalculateBounds() : null;
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
    }
}
