// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
        [SerializeField]
        ImportSettings importSettings;

        /// <inheritdoc cref="GLTFast.ImportSettings"/>
        public ImportSettings ImportSettings
        {
            get => importSettings;
            set => importSettings = value;
        }

        /// <summary>
        /// Instance used for loading the glTF's content
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public GltfImport Importer { get; protected set; }

        /// <summary>
        /// Indicates whether the glTF was loaded (no matter if successfully or not)
        /// </summary>
        /// <value>True when loading routine ended, false otherwise.</value>
        public bool IsDone => Importer != null && Importer.LoadingDone;

        /// <summary>
        /// Scene ID of the recently instantiated scene. Null if there was no
        /// scene instantiated (successfully).
        /// </summary>
        public int? CurrentSceneId { get; protected set; }

        /// <summary>
        /// Method for manual loading with custom <see cref="IDownloadProvider"/> and <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="gltfUrl">URL of the glTF file.</param>
        /// <param name="downloadProvider">Download Provider for custom loading (e.g. caching or HTTP authorization)</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        /// <param name="materialGenerator">Used to convert glTF materials to <see cref="Material"/> instances</param>
        /// <param name="logger">Used for message reporting</param>
        /// <returns>Async Task that loads the glTF's contents</returns>
        public virtual async Task<bool> Load(
            string gltfUrl,
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
            )
        {
            Importer = new GltfImport(downloadProvider, deferAgent, materialGenerator, logger);
            return await Importer.Load(gltfUrl, importSettings);
        }

        /// <summary>
        /// Creates an instance of the main scene
        /// </summary>
        /// <param name="logger">Used for message reporting</param>
        /// <returns>True if instantiation was successful.</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public async Task<bool> Instantiate(ICodeLogger logger = null)
        {
            if (Importer == null) return false;
            var instantiator = GetDefaultInstantiator(logger);
            var success = await Importer.InstantiateMainSceneAsync(instantiator);
            PostInstantiation(instantiator, success);
            return success;
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="logger">Used for message reporting</param>
        /// <returns>True if instantiation was successful.</returns>
        public virtual async Task<bool> InstantiateScene(int sceneIndex, ICodeLogger logger = null)
        {
            if (Importer == null) return false;
            var instantiator = GetDefaultInstantiator(logger);
            var success = await Importer.InstantiateSceneAsync(instantiator, sceneIndex);
            PostInstantiation(instantiator, success);
            return success;
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="instantiator">Receives scene construction calls</param>
        /// <returns>True if instantiation was successful.</returns>
        protected async Task<bool> InstantiateScene(int sceneIndex, GameObjectInstantiator instantiator)
        {
            if (Importer == null) return false;
            var success = await Importer.InstantiateSceneAsync(instantiator, sceneIndex);
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
        public Material GetMaterial(int index = 0)
        {
            return Importer?.GetMaterial(index);
        }

        /// <summary>
        /// Number of scenes loaded
        /// </summary>
        public int SceneCount => Importer?.SceneCount ?? 0;

        /// <summary>
        /// Array of scenes' names (entries can be null, if not specified)
        /// </summary>
        public string[] SceneNames
        {
            get
            {
                if (Importer != null && Importer.SceneCount > 0)
                {
                    var names = new string[Importer.SceneCount];
                    for (int i = 0; i < names.Length; i++)
                    {
                        names[i] = Importer.GetSceneName(i);
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
        protected virtual void PostInstantiation(IInstantiator instantiator, bool success)
        {
            CurrentSceneId = success ? Importer.DefaultSceneIndex : null;
        }

        /// <summary>
        /// Releases previously allocated resources.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Dispose()
        {
            if (Importer != null)
            {
                Importer.Dispose();
                Importer = null;
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
