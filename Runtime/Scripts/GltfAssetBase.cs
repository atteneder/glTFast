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

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace GLTFast
{
    using Loading;

    public class GltfAssetBase : MonoBehaviour
    {
        protected GltfImport gLTFastInstance;
        
        /// <summary>
        /// Indicates wheter the glTF was loaded (no matter if successfully or not)
        /// </summary>
        /// <value>True when loading routine ended, false otherwise.</value>
        public bool isDone => gLTFastInstance!=null && gLTFastInstance.LoadingDone;
        
        public int? currentSceneId { get; protected set; }
        
        /// <summary>
        /// Method for manual loading with custom <see cref="IDownloadProvider"/> and <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="url">URL of the glTF file.</param>
        /// <param name="downloadProvider">Download Provider for custom loading (e.g. caching or HTTP authorization)</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        public virtual async Task<bool> Load( string url, IDownloadProvider downloadProvider=null, IDeferAgent deferAgent=null, IMaterialGenerator materialGenerator=null ) {
            gLTFastInstance = new GltfImport(downloadProvider,deferAgent, materialGenerator);
            return await gLTFastInstance.Load(url);
        }

        /// <summary>
        /// Creates an instance of the main scene
        /// </summary>
        /// <returns>True if instantiation was successful.</returns>
        public bool Instantiate() {
            if (gLTFastInstance == null) return false;
            var success = gLTFastInstance.InstantiateMainScene(transform);
            currentSceneId = success ? gLTFastInstance.defaultSceneIndex : (int?)null;
            return success;
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <returns>True if instantiation was successful.</returns>
        public bool InstantiateScene(int sceneIndex) {
            if (gLTFastInstance == null) return false;
            var success = gLTFastInstance.InstantiateScene(transform,sceneIndex);
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
        }

        /// <summary>
        /// Returns an imported glTF material.
        /// Note: Asset has to have finished loading before!
        /// </summary>
        /// <param name="index">Index of material in glTF file.</param>
        /// <returns>glTF material if it was loaded successfully and index is correct, null otherwise.</returns>
        public UnityEngine.Material GetMaterial( int index = 0 ) {
            return gLTFastInstance?.GetMaterial(index);
        }

        /// <summary>
        /// Number of scenes loaded
        /// </summary>
        public int sceneCount => gLTFastInstance?.sceneCount ?? 0;

        /// <summary>
        /// Array of scenes' names (entries can be null, if not specified)
        /// </summary>
        public string[] sceneNames {
            get {
                if (gLTFastInstance != null && gLTFastInstance.sceneCount > 0) {
                    var names = new string[gLTFastInstance.sceneCount];
                    for (int i = 0; i < names.Length; i++) {
                        names[i] = gLTFastInstance.GetSceneName(i);
                    }
                    return names;
                }
                return null;
            }
        }

        protected virtual void OnDestroy()
        {
            if(gLTFastInstance!=null) {
                gLTFastInstance.Dispose();
                gLTFastInstance=null;
            }
        }
    }
}
