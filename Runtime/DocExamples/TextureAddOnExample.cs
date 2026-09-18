// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Unity.Cloud.Gltfast.Addons;
using Unity.Cloud.Gltfast.Logging;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Unity.Cloud.Gltfast.Documentation.Examples
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
                var gltfImport = new GltfImport();

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
                await gltfImport.LoadAsync(path, settings);
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
