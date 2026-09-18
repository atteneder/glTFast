// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using GLTFast.Addons;
using GLTFast.Logging;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GLTFast.Documentation.Examples
{
    class TextureAddOnExample : MonoBehaviour
    {
        // Path to the gltf asset to be imported
        public string uri;
        public bool isLogicalPath;

        [Header("Add-Ons")]
        public bool webP = true;
        public bool png = true;

        async void Start()
        {
            await LoadGltf();
        }

        public async Task LoadGltf()
        {
            try
            {
                // // Global registration of the addons, they will be used for all imports
                // ImportAddonRegistry.RegisterImportAddon(new WebpTextureAddon());
                // ImportAddonRegistry.RegisterImportAddon(new PngTextureAddon());

#if NEWTONSOFT_JSON
                var gltfImport = new Newtonsoft.GltfImport(logger: new ConsoleLogger());
#else
                var gltfImport = new GltfImport(logger: new ConsoleLogger());
#endif

                // Local registration of the addons, they will be used only for this import
                if (webP)
                {
                    gltfImport.AddImportAddonInstance(new WebpTextureAddonInstance());
                }

                if (png)
                {
                    gltfImport.AddImportAddonInstance(new PngTextureAddonInstance());
                }

                var settings = new ImportSettings { GenerateMipMaps = true };
                string path;
                if (isLogicalPath)
                {
#if !UNITY_EDITOR
                    Debug.LogError("Loading glTFs from logical paths is only supported in the editor.");
                    return;
#else
                    path = FileUtil.GetPhysicalPath(uri);
#endif
                }
                else
                {
                    path = uri;
                }
                await gltfImport.Load(path, settings);
                await gltfImport.InstantiateMainSceneAsync(transform);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

    }

    abstract class ImageLoaderAddonInstance : ImportAddonInstance
    {
        public override void Dispose() { }

        public override bool SupportsGltfExtension(string extensionName)
        {
            return false;
        }

        public override void Inject(IInstantiator instantiator) { }
    }
}
