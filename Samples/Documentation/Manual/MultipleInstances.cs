// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using GLTFast;

#if NEWTONSOFT_JSON
namespace Samples.Documentation.Manual
{
#region MultipleInstances
    using System;
    using UnityEngine;
    using GltfImport = GLTFast.Newtonsoft.GltfImport;

    public class MultipleInstances : MonoBehaviour
    {
        // Path to the gltf asset to be imported
        public string Uri;

        [Range(1,10)]
        public int quantity = 3;

        async void Start()
        {
            try
            {
                var gltfImport = new GltfImport();
                await gltfImport.Load(Uri);

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

                        await materialsVariantsControl.ApplyMaterialsVariantAsync(i%gltfImport.MaterialsVariantsCount);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
#endregion
}
#endif
