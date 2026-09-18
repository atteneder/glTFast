// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace Unity.Cloud.Gltfast.Documentation.Examples
{
    #region MultipleInstances
    using System;
    using System.Threading.Tasks;
    using Unity.Cloud.Gltfast.Logging;
    using UnityEngine;

    class MultipleInstances : MonoBehaviour
    {
        // Path to the gltf asset to be imported
        public string uri;

        [Range(1, 10)]
        public int quantity = 3;

        async void Start()
        {
            try
            {
                await LoadGltf();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async Task LoadGltf()
        {
            var gltfImport = new GltfImport();
            await gltfImport.LoadAsync(uri);

            for (var i = 0; i < quantity; i++)
            {
                var go = new GameObject($"glTF-{i}")
                {
                    transform =
                    {
                        localPosition = new Vector3(0, 0, i * .13f)
                    }
                };
                var instantiator = new GameObjectInstantiator(gltfImport, go.transform);
                await gltfImport.InstantiateMainSceneAsync(instantiator);
                var scene = instantiator.SceneInstance;
                var materialsVariantsControl = scene.MaterialsVariantsControl;

                if (materialsVariantsControl != null)
                {
                    var materialsVariantsComponent = go.AddComponent<MaterialsVariantsComponent>();
                    materialsVariantsComponent.Control = materialsVariantsControl;

                    await materialsVariantsControl.ApplyMaterialsVariantAsync(i % gltfImport.MaterialsVariantsCount);
                }
            }
        }
    }
    #endregion
}
