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

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace GLTFast
{
    using Loading;

    public class GltfAssetBase : MonoBehaviour
    {
        protected GltfImport importer;
        
        /// <summary>
        /// Indicates wheter the glTF was loaded (no matter if successfully or not)
        /// </summary>
        /// <value>True when loading routine ended, false otherwise.</value>
        public bool isDone => importer!=null && importer.LoadingDone;
        
        public int? currentSceneId { get; protected set; }
        
        /// <summary>
        /// Latest scene's instance.  
        /// </summary>
        public GameObjectInstantiator.SceneInstance sceneInstance { get; protected set; }
        
        /// <summary>
        /// Method for manual loading with custom <see cref="IDownloadProvider"/> and <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="url">URL of the glTF file.</param>
        /// <param name="downloadProvider">Download Provider for custom loading (e.g. caching or HTTP authorization)</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        /// <param name="materialGenerator">Used to convert glTF materials to <see cref="Material"/> instances</param>
        /// <param name="logger">Used for message reporting</param>
        public virtual async Task<bool> Load(
            string url,
            IDownloadProvider downloadProvider=null,
            IDeferAgent deferAgent=null,
            IMaterialGenerator materialGenerator=null,
            ICodeLogger logger = null
            )
        {
            importer = new GltfImport(downloadProvider,deferAgent, materialGenerator, logger);
            return await importer.Load(url);
        }

        /// <summary>
        /// Creates an instance of the main scene
        /// </summary>
        /// <param name="logger">Used for message reporting</param>
        /// <returns>True if instantiation was successful.</returns>
        public bool Instantiate(ICodeLogger logger = null) {
            if (importer == null) return false;
            var instantiator = GetDefaultInstantiator(logger);
            var success = importer.InstantiateMainScene(instantiator);
            sceneInstance = instantiator.sceneInstance;
            currentSceneId = success ? importer.defaultSceneIndex : (int?)null;
            return success;
        }

        /// <summary>
        /// Creates an instance of the main scene
        /// </summary>
        /// <param name="opt">Parent GameObject.Tranform for new instance</param>
        /// <returns>True if instantiation was successful.</returns>
        public bool Instantiate(Transform opt)
        {
            if (importer == null) return false;
            var success = importer.InstantiateMainScene(opt);
            sceneInstance = instantiator.sceneInstance;
            currentSceneId = success ? importer.defaultSceneIndex : (int?)null;
            return success;
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="logger">Used for message reporting</param>
        /// <returns>True if instantiation was successful.</returns>
        public virtual bool InstantiateScene(int sceneIndex, ICodeLogger logger = null) {
            if (importer == null) return false;
            var instantiator = GetDefaultInstantiator(logger);
            var success = importer.InstantiateScene(instantiator,sceneIndex);
            sceneInstance = instantiator.sceneInstance;
            currentSceneId = success ? sceneIndex : (int?)null;
            return success;
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="instantiator">Receives scene construction calls</param>
        /// <returns>True if instantiation was successful.</returns>
        protected bool InstantiateScene(int sceneIndex, GameObjectInstantiator instantiator) {
            if (importer == null) return false;
            var success = importer.InstantiateScene(instantiator,sceneIndex);
            sceneInstance = instantiator.sceneInstance;
            currentSceneId = success ? sceneIndex : (int?)null;
            return success;
        }

        /// <summary>
        /// Removes previously instantiated scene(s)
        /// </summary>
        public void ClearScenes() {
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
            sceneInstance = null;
        }

        /// <summary>
        /// Returns an imported glTF material.
        /// Note: Asset has to have finished loading before!
        /// </summary>
        /// <param name="index">Index of material in glTF file.</param>
        /// <returns>glTF material if it was loaded successfully and index is correct, null otherwise.</returns>
        public UnityEngine.Material GetMaterial( int index = 0 ) {
            return importer?.GetMaterial(index);
        }

        /// <summary>
        /// Returns an imported glTF's animation clips.
        /// Note: Asset has to have finished loading before!
        /// </summary>
        /// <returns>glTF animation array if it was loaded successfully, null otherwise.</returns>
        public UnityEngine.AnimationClip[] GetAnimationClips() {
            return importer?.GetAnimationClips();
        }

        /// <summary>
        /// Number of scenes loaded
        /// </summary>
        public int sceneCount => importer?.sceneCount ?? 0;

        /// <summary>
        /// Array of scenes' names (entries can be null, if not specified)
        /// </summary>
        public string[] sceneNames {
            get {
                if (importer != null && importer.sceneCount > 0) {
                    var names = new string[importer.sceneCount];
                    for (int i = 0; i < names.Length; i++) {
                        names[i] = importer.GetSceneName(i);
                    }
                    return names;
                }
                return null;
            }
        }

        protected virtual GameObjectInstantiator GetDefaultInstantiator(ICodeLogger logger) {
            return new GameObjectInstantiator(importer, transform, logger);
        }

        protected virtual void OnDestroy()
        {
            if(importer!=null) {
                importer.Dispose();
                importer=null;
            }
        }
    }
}
