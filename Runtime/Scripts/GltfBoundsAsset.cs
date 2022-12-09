// Copyright 2020-2022 Andreas Atteneder
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
    public class GltfBoundsAsset : GltfAsset
    {
        /// <summary>
        /// If true, a box collider encapsulating the glTF scene is created
        /// (only if the built-in Physics module is enabled).
        /// </summary>
        public bool CreateBoxCollider
        {
            get => createBoxCollider;
            set => createBoxCollider = value;
        }

        /// <summary>
        /// Bounding box of the instantiated glTF scene
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public Bounds Bounds { get; private set; }

        [SerializeField]
        [Tooltip("If true, a box collider encapsulating the glTF asset is created")]
        bool createBoxCollider = true;

        /// <inheritdoc />
        public override async Task<bool> Load(
            string gltfUrl,
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
            )
        {
            Importer = new GltfImport(downloadProvider, deferAgent, materialGenerator, logger);
            var success = await Importer.Load(gltfUrl);
            if (success)
            {
                var instantiator = (GameObjectBoundsInstantiator)GetDefaultInstantiator(logger);
                // Auto-Instantiate
                if (SceneId >= 0)
                {
                    success = await Importer.InstantiateSceneAsync(instantiator, SceneId);
                    CurrentSceneId = success ? SceneId : (int?)null;
                }
                else
                {
                    success = await Importer.InstantiateMainSceneAsync(instantiator);
                    CurrentSceneId = Importer.DefaultSceneIndex;
                }

                SceneInstance = instantiator.SceneInstance;

                if (success)
                {
                    SetBounds(instantiator);
                }
            }
            return success;
        }

        /// <inheritdoc />
        public override async Task<bool> InstantiateScene(int sceneIndex, ICodeLogger logger = null)
        {
            var instantiator = (GameObjectBoundsInstantiator)GetDefaultInstantiator(logger);
            var success = await base.InstantiateScene(sceneIndex, instantiator);
            CurrentSceneId = success ? sceneIndex : (int?)null;
            SceneInstance = instantiator.SceneInstance;
            if (success)
            {
                SetBounds(instantiator);
            }
            return success;
        }

        /// <inheritdoc />
        protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger)
        {
            return new GameObjectBoundsInstantiator(Importer, transform, logger);
        }

        void SetBounds(GameObjectBoundsInstantiator instantiator)
        {
            var sceneBounds = instantiator.SceneInstance != null ? instantiator.CalculateBounds() : null;
            if (sceneBounds.HasValue)
            {
                Bounds = sceneBounds.Value;
                if (createBoxCollider)
                {
#if UNITY_PHYSICS
                    var boxCollider = gameObject.AddComponent<BoxCollider>();
                    boxCollider.center = Bounds.center;
                    boxCollider.size = Bounds.size;
#else
                    Debug.LogError("GltfBoundsAsset requires the built-in Physics package to be enabled (in the Package Manager)");
#endif
                }
            }
        }
    }
}
