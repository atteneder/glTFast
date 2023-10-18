// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
        public string Url
        {
            get => url;
            set => url = value;
        }

        /// <summary>
        /// Automatically load at start
        /// </summary>
        public bool LoadOnStartup
        {
            get => loadOnStartup;
            set => loadOnStartup = value;
        }

        /// <summary>
        /// Scene to load (-1 loads glTFs default scene)
        /// </summary>
        protected int SceneId => sceneId;

        /// <summary>
        /// If true, the first animation clip starts playing right after instantiation.
        /// </summary>
        public bool PlayAutomatically => playAutomatically;

        /// <summary>
        /// If true, url is treated as relative StreamingAssets path
        /// </summary>
        public bool StreamingAsset
        {
            get => streamingAsset;
            set => streamingAsset = value;
        }

        /// <inheritdoc cref="GLTFast.InstantiationSettings"/>
        public InstantiationSettings InstantiationSettings
        {
            get => instantiationSettings;
            set => instantiationSettings = value;
        }

        [SerializeField]
        [Tooltip("URL to load the glTF from. Loading local file paths works by prefixing them with \"file://\"")]
        string url;

        [SerializeField]
        [Tooltip("Automatically load at start.")]
        bool loadOnStartup = true;

        [SerializeField]
        [Tooltip("Override scene to load (-1 loads glTFs default scene)")]
        int sceneId = -1;

        [SerializeField]
        [Tooltip("If true, the first animation clip starts playing right after instantiation")]
        bool playAutomatically = true;

        [SerializeField]
        [Tooltip("If checked, url is treated as relative StreamingAssets path.")]
        bool streamingAsset;

        [SerializeField]
        InstantiationSettings instantiationSettings;

        /// <summary>
        /// Latest scene's instance.
        /// </summary>
        public GameObjectSceneInstance SceneInstance { get; protected set; }

        /// <summary>
        /// Final URL, considering all options (like <see cref="streamingAsset"/>)
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string FullUrl => streamingAsset
            ? Path.Combine(Application.streamingAssetsPath, url)
            : url;

        /// <summary>
        /// Called at initialization phase
        /// </summary>
        protected virtual async void Start()
        {
            if (loadOnStartup && !string.IsNullOrEmpty(url))
            {
                // Automatic load on startup
                await Load(FullUrl);
            }
        }

        /// <inheritdoc />
        public override async Task<bool> Load(
            string gltfUrl,
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
            )
        {
            logger = logger ?? new ConsoleLogger();
            var success = await base.Load(gltfUrl, downloadProvider, deferAgent, materialGenerator, logger);
            if (success)
            {
                if (deferAgent != null) await deferAgent.BreakPoint();
                // Auto-Instantiate
                if (sceneId >= 0)
                {
                    success = await InstantiateScene(sceneId, logger);
                }
                else
                {
                    success = await Instantiate(logger);
                }
            }
            return success;
        }

        /// <inheritdoc />
        protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger)
        {
            return new GameObjectInstantiator(Importer, transform, logger, instantiationSettings);
        }

        /// <inheritdoc />
        protected override void PostInstantiation(IInstantiator instantiator, bool success)
        {
            SceneInstance = (instantiator as GameObjectInstantiator)?.SceneInstance;
#if UNITY_ANIMATION
            if (SceneInstance != null) {
                if (playAutomatically) {
                    var legacyAnimation = SceneInstance.LegacyAnimation;
                    if (legacyAnimation != null) {
                        SceneInstance.LegacyAnimation.Play();
                    }
                }
            }
#endif
            base.PostInstantiation(instantiator, success);
        }

        /// <inheritdoc />
        public override void ClearScenes()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            SceneInstance = null;
        }
    }
}
