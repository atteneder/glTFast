// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests
{
    using Addons;

    static class ImportAddonTests
    {
        [Test]
        public static void GetImportAddonInstance()
        {
            var gltf = new GltfImport();
            var addonA = gltf.GetImportAddonInstance<AddonInstanceA>();
            Assert.IsNull(addonA);

            ImportAddonRegistry.RegisterImportAddon(new AddonB());
            gltf = new GltfImport();
            addonA = gltf.GetImportAddonInstance<AddonInstanceA>();
            Assert.IsNull(addonA);
            var addonB = gltf.GetImportAddonInstance<AddonInstanceB>();
            Assert.IsNotNull(addonB);
        }

        class AddonA : ImportAddon<AddonInstanceA> { }
        class AddonB : ImportAddon<AddonInstanceB> { }

        class AddonInstanceA : ImportAddonInstance
        {
            public override bool SupportsGltfExtension(string extensionName)
            {
                return false;
            }

            public override void Inject(GltfImportBase gltfImport)
            {
                gltfImport.AddImportAddonInstance(this);
            }

            public override void Inject(IInstantiator instantiator) { }

            public override void Dispose() { }
        }

        class AddonInstanceB : ImportAddonInstance
        {
            public override bool SupportsGltfExtension(string extensionName)
            {
                return false;
            }

            public override void Inject(GltfImportBase gltfImport)
            {
                gltfImport.AddImportAddonInstance(this);
            }

            public override void Inject(IInstantiator instantiator) { }

            public override void Dispose() { }
        }
    }
}
