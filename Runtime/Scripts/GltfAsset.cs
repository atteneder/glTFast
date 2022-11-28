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

using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast
{
    using Loading;
    using Logging;
    using Materials;
    
    /// <summary>
    /// Base component for code-less loading of glTF files
    /// </summary>
    public class GltfAsset : GltfAssetBase
    {
        /// <summary>
        /// URL to load the glTF from
        /// Loading local file paths works by prefixing them with "file://"
        /// </summary>
        [Tooltip("URL to load the glTF from. Loading local file paths works by prefixing them with \"file://\"")]
        public string url;
        
        /// <summary>
        /// Automatically load at start
        /// </summary>
        [Tooltip("Automatically load at start.")]
        public bool loadOnStartup = true;
        
        /// <summary>
        /// Scene to load (-1 loads glTFs default scene)
        /// </summary>
        [Tooltip("Override scene to load (-1 loads glTFs default scene)")]
        public int sceneId = -1;
        
        /// <summary>
        /// If true, the first animation clip starts playing right after instantiation. 
        /// </summary>
        [Tooltip("If true, the first animation clip starts playing right after instantiation")]
        public bool playAutomatically = true;
        
        /// <summary>
        /// If true, url is treated as relative StreamingAssets path
        /// </summary>
        [Tooltip("If checked, url is treated as relative StreamingAssets path.")]
        public bool streamingAsset = false;

        /// <inheritdoc cref="InstantiationSettings"/>
        public InstantiationSettings instantiationSettings;
        
        /// <summary>
        /// Latest scene's instance.  
        /// </summary>
        public GameObjectInstantiator.SceneInstance sceneInstance { get; protected set; }
        
        /// <summary>
        /// Final URL, considering all options (like <seealso cref="streamingAsset"/>)
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string FullUrl => streamingAsset
            ? Path.Combine(Application.streamingAssetsPath, url)
            : url;

        /// <summary>
        /// Called at initialization phase
        /// </summary>
        protected virtual async void Start() {
            if(loadOnStartup && !string.IsNullOrEmpty(url)) {
                // Automatic load on startup
                await Load(FullUrl);
            }
        }

        /// <inheritdoc />
        public override async Task<bool> Load(
            string url,
            IDownloadProvider downloadProvider=null,
            IDeferAgent deferAgent=null,
            IMaterialGenerator materialGenerator=null,
            ICodeLogger logger = null
            )
        {
            logger = logger ?? new ConsoleLogger();
            var success = await base.Load(url, downloadProvider, deferAgent, materialGenerator, logger);
            if(success) {
                if (deferAgent != null) await deferAgent.BreakPoint();
                // Auto-Instantiate
                if (sceneId>=0) {
                    await InstantiateScene(sceneId,logger);
                } else {
                    await Instantiate(logger);
                }
            }
            return success;
        }
        
        /// <inheritdoc />
        protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger) {
            return new GameObjectInstantiator(importer, transform, logger, instantiationSettings);
        }
        
        /// <inheritdoc />
        protected override void PostInstantiation(IInstantiator instantiator, bool success) {
            sceneInstance = (instantiator as GameObjectInstantiator).sceneInstance;
#if UNITY_ANIMATION
            if (playAutomatically) {
                var legacyAnimation = sceneInstance?.legacyAnimation;
                if (legacyAnimation != null) {
                    sceneInstance.legacyAnimation.Play();
                }
            }
#endif
            base.PostInstantiation(instantiator, success);
        }
        
        /// <inheritdoc />
        public override void ClearScenes() {
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
            sceneInstance = null;
        }
    }
}
