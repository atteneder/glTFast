// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    using Loading;
    using Logging;
    using Materials;

    /// <summary>
    /// Base component for code-less loading of glTF files
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public abstract class GltfAssetBase : MonoBehaviour
    {
        [SerializeField]
        ImportSettings importSettings;

        /// <inheritdoc cref="Unity.Cloud.Gltfast.ImportSettings"/>
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

        [Obsolete("Load has been renamed to LoadAsync. (UnityUpgradable) -> LoadAsync(*)", true)]
        public Task<bool> Load(
            string gltfUrl,
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
            )
            => LoadAsync(gltfUrl, downloadProvider, deferAgent, materialGenerator, logger);

        /// <summary>
        /// Method for manual loading with custom <see cref="IDownloadProvider"/> and <see cref="IDeferAgent"/>.
        /// </summary>
        /// <param name="gltfUrl">URL of the glTF file.</param>
        /// <param name="downloadProvider">Download Provider for custom loading (e.g. caching or HTTP authorization)</param>
        /// <param name="deferAgent">Defer Agent takes care of interrupting the
        /// loading procedure in order to keep the frame rate responsive.</param>
        /// <param name="materialGenerator">Used to convert glTF materials to <see cref="Material"/> instances</param>
        /// <param name="logger">Custom logger for reporting messages. Default behavior is inherited from the <see cref="Unity.Cloud.Gltfast.GltfImport(Unity.Cloud.Gltfast.Loading.IDownloadProvider, Unity.Cloud.Gltfast.IDeferAgent, Unity.Cloud.Gltfast.Materials.IMaterialGenerator, Unity.Cloud.Gltfast.Logging.ICodeLogger)"/> constructor that this method forwards to.</param>
        /// <returns>Async Task that loads the glTF's contents</returns>
        public virtual async Task<bool> LoadAsync(
            string gltfUrl,
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
            )
        {
            Importer = new GltfImport(downloadProvider, deferAgent, materialGenerator, logger);
            return await Importer.LoadAsync(gltfUrl, importSettings);
        }

        [Obsolete("Instantiate has been renamed to InstantiateAsync. (UnityUpgradable) -> InstantiateAsync(*)", true)]
        public Task<bool> Instantiate(ICodeLogger logger = null)
        {
            return InstantiateAsync(logger);
        }

        /// <summary>
        /// Creates an instance of the main scene
        /// </summary>
        /// <param name="logger">Custom logger for reporting messages. Defaults to the shared <see cref="Unity.Cloud.Gltfast.Logging.ConsoleLogger.Instance"/> (writes to Unity's Console) when <c>null</c> is passed. Pass <see cref="Unity.Cloud.Gltfast.Logging.NullLogger.Instance"/> (or <c>new NullLogger()</c>) to suppress all output.</param>
        /// <returns>True if instantiation was successful.</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public async Task<bool> InstantiateAsync(ICodeLogger logger = null)
        {
            if (Importer == null) return false;
            logger ??= ConsoleLogger.Instance;
            var instantiator = GetDefaultInstantiator(logger);
            var success = await Importer.InstantiateMainSceneAsync(instantiator);
            PostInstantiation(instantiator, success);
            return success;
        }

        [Obsolete("InstantiateScene has been renamed to InstantiateSceneAsync. (UnityUpgradable) -> InstantiateSceneAsync(*)", true)]
        public Task<bool> InstantiateScene(int sceneIndex, ICodeLogger logger = null)
            => InstantiateSceneAsync(sceneIndex, logger);

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="logger">Custom logger for reporting messages. Defaults to the shared <see cref="Unity.Cloud.Gltfast.Logging.ConsoleLogger.Instance"/> (writes to Unity's Console) when <c>null</c> is passed. Pass <see cref="Unity.Cloud.Gltfast.Logging.NullLogger.Instance"/> (or <c>new NullLogger()</c>) to suppress all output.</param>
        /// <returns>True if instantiation was successful.</returns>
        public virtual async Task<bool> InstantiateSceneAsync(int sceneIndex, ICodeLogger logger = null)
        {
            if (Importer == null) return false;
            logger ??= ConsoleLogger.Instance;
            var instantiator = GetDefaultInstantiator(logger);
            var success = await Importer.InstantiateSceneAsync(instantiator, sceneIndex);
            PostInstantiation(instantiator, success);
            return success;
        }

        [Obsolete("InstantiateScene has been renamed to InstantiateSceneAsync. (UnityUpgradable) -> InstantiateSceneAsync(*)", true)]
        protected Task<bool> InstantiateScene(int sceneIndex, GameObjectInstantiator instantiator)
            => InstantiateSceneAsync(sceneIndex, instantiator);

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="instantiator">Receives scene construction calls</param>
        /// <returns>True if instantiation was successful.</returns>
        protected async Task<bool> InstantiateSceneAsync(int sceneIndex, GameObjectInstantiator instantiator)
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
