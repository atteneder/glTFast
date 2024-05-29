// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;
using System.Text;
using GLTFast.Schema;
using NUnit.Framework;
using UnityEngine;
using Mesh = GLTFast.Schema.Mesh;

namespace GLTFast.Tests.Export
{
    class JsonSerialization
    {
        [Test]
        public void MaterialsVariantsExtension()
        {
            var gltf = new Root
            {
                extensions = new RootExtensions
                {
                    KHR_materials_variants = new MaterialsVariantsRootExtension
                    {
                        variants = new List<MaterialsVariant>
                        {
                            new MaterialsVariant{ name = "red" },
                            new MaterialsVariant{ name = "green" },
                            new MaterialsVariant{ name = "blue" },
                        }
                    }
                },
                meshes = new[]
                {
                    new Mesh
                    {
                        primitives = new []
                        {
                            new MeshPrimitive
                            {
                                extensions = new MeshPrimitiveExtensions
                                {
                                    KHR_materials_variants = new MaterialsVariantsMeshPrimitiveExtension
                                    {
                                        mappings = new List<MaterialVariantsMapping>
                                        {
                                            new MaterialVariantsMapping {material = 0, variants = new [] { 0 }},
                                            new MaterialVariantsMapping {material = 1, variants = new [] { 1 }},
                                            new MaterialVariantsMapping {material = 2, variants = new [] { 2 }},
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            gltf.GltfSerialize(writer);
            writer.Close();
            var jsonString = Encoding.Default.GetString((stream.ToArray()));
            Assert.AreEqual(
                @"{""meshes"":[{""primitives"":[{""extensions"":{""KHR_materials_variants"":{""mappings"":[{""material"":0,""variants"":[0]},{""material"":1,""variants"":[1]},{""material"":2,""variants"":[2]}]}}}]}],""extensions"":{""KHR_materials_variants"":{""variants"":[""name"":""red"",""name"":""green"",""name"":""blue""]}}}",
                jsonString
                );
        }
    }
}
