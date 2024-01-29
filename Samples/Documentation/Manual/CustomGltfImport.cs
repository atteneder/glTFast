#if NEWTONSOFT_JSON
namespace Samples.Documentation.Manual
{
#region CustomGltfImport
    using GLTFast;
    using GLTFast.Addons;
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using GltfImport = GLTFast.Newtonsoft.GltfImport;

    public class CustomGltfImport : MonoBehaviour
    {
        // Path to the gltf asset to be imported
        public string Uri;

        async Task Start()
        {
            try
            {
                ImportAddonRegistry.RegisterImportAddon(new MyAddon());
                var gltfImport = new GltfImport();
                await gltfImport.Load(Uri);
                await gltfImport.InstantiateMainSceneAsync(transform);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public class MyAddon : ImportAddon<MyAddonInstance> { }
        public class MyAddonInstance : ImportAddonInstance
        {
            GltfImport m_GltfImport;

            public override void Dispose() { }

            public override void Inject(GltfImportBase gltfImport)
            {
                var newtonsoftGltfImport = gltfImport as GltfImport;
                if (newtonsoftGltfImport == null)
                    return;

                m_GltfImport = newtonsoftGltfImport;
                newtonsoftGltfImport.AddImportAddonInstance(this);
            }

            public override void Inject(IInstantiator instantiator)
            {
                var goInstantiator = instantiator as GameObjectInstantiator;
                if (goInstantiator == null)
                    return;
                var _ = new MyInstantiatorAddon(m_GltfImport, goInstantiator);
            }

            public override bool SupportsGltfExtension(string extensionName)
            {
                return false;
            }
        }
    }

    public class MyInstantiatorAddon
    {
        GltfImport m_GltfImport;
        GameObjectInstantiator m_Instantiator;

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
            var gltf = m_GltfImport.GetSourceRoot();

            var node = gltf.Nodes[(int)nodeIndex] as GLTFast.Newtonsoft.Schema.Node;
            var extras = node.extras;

            if (extras == null)
                return;

            // Access values in the extras property
            if (extras.TryGetValue("some-extra-key", out string extraValue))
            {
                var component = gameObject.AddComponent<ExtraData>();
                component.someExtraKey = extraValue;
            }
        }
    }
#endregion

}
#endif
