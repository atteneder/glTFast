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

using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast
{
    using Logging;
    using Loading;
    using Materials;

    /// <summary>
    /// Base component for code-less loading of glTF files
    /// </summary>
    public abstract class GltfAssetBase : MonoBehaviour
    {
        /// <inheritdoc cref="ImportSettings"/>
        public ImportSettings importSettings;
        
        /// <summary>
        /// Instance used for loading the glTF's content
        /// </summary>
        public GltfImport importer { get; protected set; }
        
        /// <summary>
        /// Indicates wheter the glTF was loaded (no matter if successfully or not)
        /// </summary>
        /// <value>True when loading routine ended, false otherwise.</value>
        public bool isDone => importer!=null && importer.LoadingDone;
        
        /// <summary>
        /// Scene ID of the recently instantiated scene. Null if there was no
        /// scene instantiated (successfully).
        /// </summary>
        public int? currentSceneId { get; protected set; }
        
        /// <summary>
        /// Method for manual loading with custom <see cref="IDownloadProvider"/> and <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="url">URL of the glTF file.</param>
        /// <param name="downloadProvider">Download Provider for custom loading (e.g. caching or HTTP authorization)</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        /// <param name="materialGenerator">Used to convert glTF materials to <see cref="Material"/> instances</param>
        /// <param name="logger">Used for message reporting</param>
        /// <returns>Async Task that loads the glTF's contents</returns>
        public virtual async Task<bool> Load(
            string url,
            IDownloadProvider downloadProvider=null,
            IDeferAgent deferAgent=null,
            IMaterialGenerator materialGenerator=null,
            ICodeLogger logger = null
            )
        {
            importer = new GltfImport(downloadProvider,deferAgent, materialGenerator, logger);
            return await importer.Load(url,importSettings);
        }

        /// <summary>
        /// Creates an instance of the main scene
        /// </summary>
        /// <param name="logger">Used for message reporting</param>
        /// <returns>True if instantiation was successful.</returns>
        public async Task<bool> Instantiate(ICodeLogger logger = null) {
            if (importer == null) return false;
            var instantiator = GetDefaultInstantiator(logger);
            var success = await importer.InstantiateMainSceneAsync(instantiator);
            PostInstantiation(instantiator, success);
            return success;
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="logger">Used for message reporting</param>
        /// <returns>True if instantiation was successful.</returns>
        public virtual async Task<bool> InstantiateScene(int sceneIndex, ICodeLogger logger = null) {
            if (importer == null) return false;
            var instantiator = GetDefaultInstantiator(logger);
            var success = await importer.InstantiateSceneAsync(instantiator,sceneIndex);
            PostInstantiation(instantiator, success);
            return success;
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="instantiator">Receives scene construction calls</param>
        /// <returns>True if instantiation was successful.</returns>
        protected async Task<bool> InstantiateScene(int sceneIndex, GameObjectInstantiator instantiator) {
            if (importer == null) return false;
            var success = await importer.InstantiateSceneAsync(instantiator,sceneIndex);
            PostInstantiation(instantiator, success);
            return success;
        }

        /// <summary>
        /// Removes previously instantiated scene(s)
        /// </summary>
        public abstract void ClearScenes();

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

        /// <summary>
        /// Returns an instance of the default instantiator
        /// </summary>
        /// <param name="logger">Custom logger to use with the instantiator</param>
        /// <returns>Default instantiator instance</returns>
        protected abstract IInstantiator GetDefaultInstantiator(ICodeLogger logger);
        
        /// <summary>
        /// Callback that is called after instantiation
        /// </summary>
        /// <param name="instantiator">instantiator that was used</param>
        /// <param name="success">True if instantiation was successful, false otherwise</param>
        protected virtual void PostInstantiation(IInstantiator instantiator, bool success) {
            currentSceneId = success ? importer.defaultSceneIndex : (int?)null;
        }

        /// <summary>
        /// Releases previously allocated resources.
        /// </summary>
        public void Dispose() {
            if(importer!=null) {
                importer.Dispose();
                importer=null;
            }
        }

        /// <summary>
        /// Called before GameObject is destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            Dispose();
        }
    }
}
