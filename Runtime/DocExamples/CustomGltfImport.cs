// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0


namespace Unity.Cloud.Gltfast.Documentation.Examples
{
    #region CustomGltfImport
    using System;
    using System.Threading.Tasks;
    using Unity.Cloud.Gltfast;
    using Unity.Cloud.Gltfast.Addons;
    using Unity.Cloud.Gltfast.Objects;
    using UnityEngine;

    class CustomGltfImport : MonoBehaviour
    {
        // Path to the gltf asset to be imported
        public string uri;

        async void Start()
        {
            await LoadGltf();
        }

        public async Task LoadGltf()
        {
            try
            {
                ImportAddonRegistry.RegisterImportAddon(new MyAddon());
                var gltfImport = new GltfImport();
                await gltfImport.LoadAsync(uri);
                await gltfImport.InstantiateMainSceneAsync(transform);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        class MyAddon : ImportAddon<MyAddonInstance> { }

        class MyAddonInstance : ImportAddonInstance
        {
            GltfImport m_GltfImport;

            public override void Dispose() { }

            public override void Inject(GltfImport gltfImport)
            {
                var import = gltfImport as GltfImport;
                if (import == null)
                    return;

                m_GltfImport = import;
                import.AddImportAddonInstance(this);
            }

            public override void Inject(IInstantiator instantiator)
            {
                var goInstantiator = instantiator as GameObjectInstantiator;
                if (goInstantiator == null)
                    return;
                _ = new MyInstantiatorAddon(m_GltfImport, goInstantiator);
            }

            public override bool SupportsGltfExtension(string extensionName)
            {
                return false;
            }
        }
    }

    class MyInstantiatorAddon
    {
        readonly GltfImport m_GltfImport;
        readonly GameObjectInstantiator m_Instantiator;

        public MyInstantiatorAddon(GltfImport gltfImport, GameObjectInstantiator instantiator)
        {
            m_GltfImport = gltfImport;
            m_Instantiator = instantiator;
            m_Instantiator.NodeCreated += OnNodeCreated;
            m_Instantiator.EndSceneCompleted += () =>
            {
                m_Instantiator.NodeCreated -= OnNodeCreated;
            };
        }

        void OnNodeCreated(uint nodeIndex, GameObject gameObject)
        {
            // De-serialize glTF JSON
            var gltf = m_GltfImport.Root;

            var node = gltf.Nodes[(int)nodeIndex];
            var extras = node?.Extras;

            if (extras is { Kind: ValueKind.Object }
                && extras.TryGetValue("some-extra-key", out string extraValue))
            {
                var component = gameObject.AddComponent<ExtraData>();
                component.someExtraKey = extraValue;
            }
        }
    }
    #endregion
}
