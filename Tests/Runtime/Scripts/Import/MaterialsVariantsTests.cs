// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Collections.Generic;
using GLTFast.Schema;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Material = UnityEngine.Material;

namespace GLTFast.Tests
{
    class MaterialsVariantsTests
    {
        const int k_SubMeshCount = 3;

        [UnityTest]
        public IEnumerator MaterialsVariantsControlTest()
        {
            var provider = new MaterialProviderMock();
            var renderer = CreateRenderer();
            var renderers = new MeshRenderer[]
            {
                CreateRenderer(),
                CreateRenderer(),
            };
            var slots = new IMaterialsVariantsSlot[k_SubMeshCount];
            for (var subMesh = 0; subMesh < k_SubMeshCount; subMesh++)
            {
                slots[subMesh] = CreatePrimitive(subMesh, provider.MaterialsVariantsCount);
            }
            var slotInstance = new MaterialsVariantsSlotInstances(renderer, slots);
            var multiSlotInstance = new MultiMaterialsVariantsSlotInstances(renderers, slots);
            var slotInstances = new List<IMaterialsVariantsSlotInstance> { slotInstance, multiSlotInstance };
            var ctrl = new MaterialsVariantsControl(provider, slotInstances);
            var go = new GameObject();
            var comp = go.AddComponent<MaterialsVariantsComponent>();
            comp.Control = ctrl;

            for (var variant = 0; variant < provider.MaterialsVariantsCount; variant++)
            {
                yield return AsyncWrapper.WaitForTask(
                    ctrl.ApplyMaterialsVariantAsync(variant)
                    );

                AssertMaterials(renderer, variant);
                foreach (var r in renderers)
                {
                    AssertMaterials(r, variant);
                }
            }

            // Reset to default materials
            yield return AsyncWrapper.WaitForTask(
                comp.Control.ApplyMaterialsVariantAsync(-1)
                );

            AssertDefaultMaterials(renderer);
            foreach (var r in renderers)
            {
                AssertDefaultMaterials(r);
            }
        }

        static void AssertDefaultMaterials(MeshRenderer renderer)
        {
            var materials = renderer.sharedMaterials;
            Assert.AreEqual("Default", materials[0].name);
            for (var subMesh = 1; subMesh < 3; subMesh++)
            {
                Assert.AreEqual(materials[subMesh].name, subMesh.ToString());
            }
        }

        static void AssertMaterials(MeshRenderer renderer, int variant)
        {
            var materials = renderer.sharedMaterials;
            for (var subMesh = 0; subMesh < 3; subMesh++)
            {
                string actual;
                if ((variant + 1) % 3 == 0)
                {
                    actual = subMesh == 0 ? "Default" : subMesh.ToString();
                }
                else
                {
                    actual = ((variant + 3) / 3 * 100 + subMesh).ToString();
                }

                if (actual != materials[subMesh].name)
                {

                    Assert.AreEqual(actual, materials[subMesh].name);
                }
            }
        }

        static MeshRenderer CreateRenderer()
        {
            var go = new GameObject();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new Material[k_SubMeshCount];
            return renderer;
        }

        static MeshPrimitive CreatePrimitive(int seed, int variantsCount)
        {
            var result = new MeshPrimitive
            {
                material = seed == 0 ? -1 : seed,
                extensions = new MeshPrimitiveExtensions
                {
                    KHR_materials_variants = new MaterialsVariantsMeshPrimitiveExtension
                    {
                        mappings = new List<MaterialVariantsMapping>()
                    }
                }
            };

            for (var variant = 0; variant < variantsCount + 2; variant += 3)
            {
                result.extensions.KHR_materials_variants.mappings.Add(
                    new MaterialVariantsMapping { material = seed + (variant + 3) / 3 * 100, variants = new[] { variant, variant + 1 } }
                    );
            }

            return result;
        }

        [Test]
        public void MaterialsVariantsRootExtensionTest()
        {
            var root = new Root
            {
                extensions = new RootExtensions
                {
                    KHR_materials_variants = new MaterialsVariantsRootExtension
                    {
                        variants = new List<MaterialsVariant>
                        {
                            new MaterialsVariant { name = "One" },
                            new MaterialsVariant { name = "Two" },
                            new MaterialsVariant { name = "Spanish Inquisition" },
                        }
                    }
                }
            };

            Assert.AreEqual("One", root.GetMaterialsVariantName(0));
            Assert.AreEqual("Two", root.GetMaterialsVariantName(1));
            Assert.AreEqual("Spanish Inquisition", root.GetMaterialsVariantName(2));
            Assert.IsNull(root.GetMaterialsVariantName(3));
            Assert.IsNull(root.GetMaterialsVariantName(-1));
        }
    }
}
