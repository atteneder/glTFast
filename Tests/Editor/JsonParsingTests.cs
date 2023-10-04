// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using GLTFast.Schema;
using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests
{
    [Category("JsonParsing")]
    class JsonParsingTests
    {
        [Test]
        public void MaterialExtensions()
        {
            var jsonParser = new GltfJsonUtilityParser();
            var gltf = jsonParser.ParseJson(@"
{
    ""materials"" : [
        {
            ""name"" : ""noExtension""
        },
        {
            ""name"" : ""emptyExtension"",
            ""extensions"": {
                ""dummy"": ""value""
            }
        },
        {
            ""name"" : ""unlit"",
            ""extensions"": {
                ""KHR_materials_unlit"": {}
            }
        },
        {
            ""name"" : ""specularGlossiness"",
            ""extensions"": {
                ""KHR_materials_pbrSpecularGlossiness"": {
                    ""diffuseTexture"": {
                        ""index"": 5
                    }
                }
            }
        },
        {
            ""name"" : ""transmission"",
            ""extensions"": {
                ""KHR_materials_transmission"": {}
            }
        },
        {
            ""name"" : ""clearcoat"",
            ""extensions"": {
                ""KHR_materials_clearcoat"": {}
            }
        },
        {
            ""name"" : ""sheen"",
            ""extensions"": {
                ""KHR_materials_sheen"": {}
            }
        },
        {
            ""name"" : ""all"",
            ""extensions"": {
                ""KHR_materials_unlit"": {},
                ""KHR_materials_pbrSpecularGlossiness"": {},
                ""KHR_materials_transmission"": {},
                ""KHR_materials_clearcoat"": {},
                ""KHR_materials_sheen"": {}
            }
        }
    ]
}
"
            );

            Assert.NotNull(gltf);
            Assert.NotNull(gltf.Materials, "No materials");
            Assert.AreEqual(8, gltf.Materials.Count, "Invalid material quantity");

            var none = gltf.Materials[0];
            Assert.NotNull(none);
            Assert.AreEqual("noExtension", none.name);
            Assert.IsNull(none.Extensions);

            var empty = gltf.Materials[1];
            Assert.NotNull(empty);
            Assert.AreEqual("emptyExtension", empty.name);
            Assert.NotNull(empty.Extensions);
            Assert.IsNull(empty.Extensions.KHR_materials_unlit);
            Assert.IsNull(empty.Extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(empty.Extensions.KHR_materials_clearcoat);
            Assert.IsNull(empty.Extensions.KHR_materials_sheen);
            Assert.IsNull(empty.Extensions.KHR_materials_transmission);

            var unlit = gltf.Materials[2];
            Assert.NotNull(unlit);
            Assert.AreEqual("unlit", unlit.name);
            Assert.NotNull(unlit.Extensions);
            Assert.NotNull(unlit.Extensions.KHR_materials_unlit);
            Assert.IsNull(unlit.Extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(unlit.Extensions.KHR_materials_clearcoat);
            Assert.IsNull(unlit.Extensions.KHR_materials_sheen);
            Assert.IsNull(unlit.Extensions.KHR_materials_transmission);

            var specGloss = gltf.Materials[3];
            Assert.NotNull(specGloss);
            Assert.AreEqual("specularGlossiness", specGloss.name);
            Assert.NotNull(specGloss.Extensions);
            Assert.IsNull(specGloss.Extensions.KHR_materials_unlit);
            Assert.NotNull(specGloss.Extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(specGloss.Extensions.KHR_materials_clearcoat);
            Assert.IsNull(specGloss.Extensions.KHR_materials_sheen);
            Assert.IsNull(specGloss.Extensions.KHR_materials_transmission);

            var transmission = gltf.Materials[4];
            Assert.NotNull(transmission);
            Assert.AreEqual("transmission", transmission.name);
            Assert.NotNull(transmission.Extensions);
            Assert.IsNull(transmission.Extensions.KHR_materials_unlit);
            Assert.IsNull(transmission.Extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(transmission.Extensions.KHR_materials_clearcoat);
            Assert.IsNull(transmission.Extensions.KHR_materials_sheen);
            Assert.NotNull(transmission.Extensions.KHR_materials_transmission);

            var clearcoat = gltf.Materials[5];
            Assert.NotNull(clearcoat);
            Assert.AreEqual("clearcoat", clearcoat.name);
            Assert.NotNull(clearcoat.Extensions);
            Assert.IsNull(clearcoat.Extensions.KHR_materials_unlit);
            Assert.IsNull(clearcoat.Extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.NotNull(clearcoat.Extensions.KHR_materials_clearcoat);
            Assert.IsNull(clearcoat.Extensions.KHR_materials_sheen);
            Assert.IsNull(clearcoat.Extensions.KHR_materials_transmission);

            var sheen = gltf.Materials[6];
            Assert.NotNull(sheen);
            Assert.AreEqual("sheen", sheen.name);
            Assert.NotNull(sheen.Extensions);
            Assert.IsNull(sheen.Extensions.KHR_materials_unlit);
            Assert.IsNull(sheen.Extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(sheen.Extensions.KHR_materials_clearcoat);
            Assert.NotNull(sheen.Extensions.KHR_materials_sheen);
            Assert.IsNull(sheen.Extensions.KHR_materials_transmission);

            var all = gltf.Materials[7];
            Assert.NotNull(all);
            Assert.AreEqual("all", all.name);
            Assert.NotNull(all.Extensions);
            Assert.NotNull(all.Extensions.KHR_materials_unlit);
            Assert.NotNull(all.Extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.NotNull(all.Extensions.KHR_materials_clearcoat);
            Assert.NotNull(all.Extensions.KHR_materials_sheen);
            Assert.NotNull(all.Extensions.KHR_materials_transmission);
        }

        [Test]
        public void SparseAccessors()
        {
            var jsonParser = new GltfJsonUtilityParser();
            var gltf = jsonParser.ParseJson(@"
{
    ""accessors"" : [ {
        ""bufferView"" : 0,
        ""byteOffset"" : 0,
        ""componentType"" : 5123,
        ""count"" : 36,
        ""type"" : ""SCALAR"",
        ""max"" : [ 13 ],
        ""min"" : [ 0 ]
      }, {
        ""bufferView"" : 1,
        ""byteOffset"" : 0,
        ""componentType"" : 5126,
        ""count"" : 14,
        ""type"" : ""VEC3"",
        ""max"" : [ 6.0, 4.0, 0.0 ],
        ""min"" : [ 0.0, 0.0, 0.0 ],
        ""sparse"" : {
          ""count"" : 3,
          ""indices"" : {
            ""bufferView"" : 2,
            ""byteOffset"" : 0,
            ""componentType"" : 5123
          },
          ""values"" : {
            ""bufferView"" : 3,
            ""byteOffset"" : 0
          }
        }
        }, {
        ""bufferView"" : 1,
        ""byteOffset"" : 0,
        ""componentType"" : 5126,
        ""count"" : 14,
        ""type"" : ""VEC3"",
        ""max"" : [ 6.0, 4.0, 0.0 ],
        ""min"" : [ 0.0, 0.0, 0.0 ],
        ""sparse"" : {}
        } ]
}
"
            );

            Assert.NotNull(gltf);
            Assert.NotNull(gltf.Accessors, "No accessors");
            Assert.AreEqual(3, gltf.Accessors.Count, "Invalid accessor quantity");

            var regular = gltf.Accessors[0];
            Assert.NotNull(regular);
            Assert.IsNull(regular.Sparse);

            var sparse = gltf.Accessors[1];
            Assert.NotNull(sparse);
            Assert.AreEqual(14, sparse.count);
            Assert.NotNull(sparse.Sparse);
            Assert.AreEqual(3, sparse.Sparse.count);
            Assert.NotNull(sparse.Sparse.Indices);
            Assert.AreEqual(2, sparse.Sparse.Indices.bufferView);
            Assert.AreEqual(0, sparse.Sparse.Indices.byteOffset);
            Assert.AreEqual(GltfComponentType.UnsignedShort, sparse.Sparse.Indices.componentType);
            Assert.NotNull(sparse.Sparse.Values);
            Assert.AreEqual(3, sparse.Sparse.Values.bufferView);
            Assert.AreEqual(0, sparse.Sparse.Values.byteOffset);

#if GLTFAST_SAFE
            var invalid = gltf.accessors[2];
            Assert.NotNull(invalid);
            Assert.IsNull(invalid.sparse);
#else
            Debug.LogWarning("Invalid Sparse Accessors will break glTFast");
#endif
        }

        [Test]
        public void MeshTargetNames()
        {
            var jsonParser = new GltfJsonUtilityParser();
            var gltf = jsonParser.ParseJson(@"
{
    ""meshes"": [
        {
            ""extras"": {
                ""targetNames"": [
                    ""Key 1"",""Key 2""
                ]
            }
        },
        {
            ""extras"": {
                ""different"": ""content""
            }
        }
    ]
}
"
            );

            Assert.NotNull(gltf);
            Assert.NotNull(gltf.Meshes, "No materials");
            Assert.AreEqual(2, gltf.Meshes.Count, "Invalid materials quantity");

            var mat = gltf.Meshes[0];
            Assert.NotNull(mat);
            Assert.NotNull(mat.Extras);
            Assert.NotNull(mat.Extras.targetNames);
            Assert.NotNull(mat.Extras.targetNames);
            Assert.AreEqual(2, mat.Extras.targetNames.Length, "Invalid targetNames quantity");
            Assert.AreEqual("Key 1", mat.Extras.targetNames[0]);
            Assert.AreEqual("Key 2", mat.Extras.targetNames[1]);

            mat = gltf.Meshes[1];
            Assert.NotNull(mat);
            Assert.NotNull(mat.Extras);
            Assert.IsNull(mat.Extras.targetNames);
        }

        [Test]
        public void MinMagFilter()
        {
            var jsonParser = new GltfJsonUtilityParser();
            var gltf = jsonParser.ParseJson(@"
{
    ""samplers"": [{
        },{
        ""magFilter"": 100,
        ""minFilter"": 100
        },{
        ""magFilter"": 9728,
        ""minFilter"": 9728
        },{
        ""magFilter"": 9729,
        ""minFilter"": 9729
        },{
        ""minFilter"": 9984
        },{
        ""minFilter"": 9985
        },{
        ""minFilter"": 9986
        },{
        ""minFilter"": 9987
        }
    ]
}
"

            );

            Assert.NotNull(gltf);
            Assert.NotNull(gltf.Samplers, "No samplers");
            Assert.AreEqual(8, gltf.Samplers.Count, "Invalid samplers quantity");

            var sampler0 = gltf.Samplers[0];
            Assert.NotNull(sampler0);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler0.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.None, sampler0.minFilter);

            var sampler1 = gltf.Samplers[1];
            Assert.NotNull(sampler1);
            Assert.AreEqual((Sampler.MagFilterMode)100, sampler1.magFilter);
            Assert.AreEqual((Sampler.MinFilterMode)100, sampler1.minFilter);

            var sampler2 = gltf.Samplers[2];
            Assert.NotNull(sampler2);
            Assert.AreEqual(Sampler.MagFilterMode.Nearest, sampler2.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.Nearest, sampler2.minFilter);

            var sampler3 = gltf.Samplers[3];
            Assert.NotNull(sampler3);
            Assert.AreEqual(Sampler.MagFilterMode.Linear, sampler3.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.Linear, sampler3.minFilter);

            var sampler4 = gltf.Samplers[4];
            Assert.NotNull(sampler4);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler4.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.NearestMipmapNearest, sampler4.minFilter);

            var sampler5 = gltf.Samplers[5];
            Assert.NotNull(sampler5);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler5.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.LinearMipmapNearest, sampler5.minFilter);

            var sampler6 = gltf.Samplers[6];
            Assert.NotNull(sampler6);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler6.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.NearestMipmapLinear, sampler6.minFilter);

            var sampler7 = gltf.Samplers[7];
            Assert.NotNull(sampler7);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler7.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.LinearMipmapLinear, sampler7.minFilter);

        }

        [Test]
        public void UnknownNodeExtension()
        {
            var jsonParser = new GltfJsonUtilityParser();
            var gltf = jsonParser.ParseJson(@"
{
    ""nodes"": [
        {
            ""name"": ""Node0""
        },
        {
            ""extensions"": {},
            ""name"": ""Node1""
        },
        {
            ""extensions"": {
                ""MOZ_hubs_components"": {
                    ""morph-audio-feedback"": {
                        ""name"": ""mouthOpen"",
                        ""minValue"": 0.0,
                        ""maxValue"": 1.0
                    }
                }
            },
            ""name"": ""Node2""
        },
        {
            ""extensions"": {
                ""EXT_mesh_gpu_instancing"": {
                    ""attributes"": {
                        ""TRANSLATION"": 42
                    }
                }
            },
            ""name"": ""Node3""
        },
        {
            ""extensions"": {
                ""KHR_lights_punctual"": {
                    ""light"": 42
                }
            },
            ""name"": ""Node4""
        },
        {
            ""extensions"": {
                ""EXT_mesh_gpu_instancing"": {
                    ""attributes"": {
                        ""TRANSLATION"": 13
                    }
                },
                ""KHR_lights_punctual"": {
                    ""light"": 42
                }
            },
            ""name"": ""Node5""
        }
    ]
}
"
            );

            Assert.NotNull(gltf);
            Assert.NotNull(gltf.Nodes, "No nodes");
            Assert.AreEqual(6, gltf.Nodes.Count, "Invalid nodes quantity");

            var node0 = gltf.Nodes[0];
            Assert.NotNull(node0);
            Assert.IsNull(node0.Extensions);

            var node1 = gltf.Nodes[1];
            Assert.NotNull(node1);
            Assert.IsNull(node1.Extensions);

            var node2 = gltf.Nodes[2];
            Assert.NotNull(node2);
            Assert.IsNull(node2.Extensions);

            var node3 = gltf.Nodes[3];
            Assert.NotNull(node3);
            Assert.NotNull(node3.Extensions);
            Assert.NotNull(node3.Extensions.EXT_mesh_gpu_instancing);
            Assert.NotNull(node3.Extensions.EXT_mesh_gpu_instancing.attributes);
            Assert.AreEqual(42, node3.Extensions.EXT_mesh_gpu_instancing.attributes.TRANSLATION);
            Assert.IsNull(node3.Extensions.KHR_lights_punctual);

            var node4 = gltf.Nodes[4];
            Assert.NotNull(node4);
            Assert.NotNull(node4.Extensions);
            Assert.IsNull(node4.Extensions.EXT_mesh_gpu_instancing);
            Assert.NotNull(node4.Extensions.KHR_lights_punctual);
            Assert.AreEqual(42, node4.Extensions.KHR_lights_punctual.light);

            var node5 = gltf.Nodes[5];
            Assert.NotNull(node5);
            Assert.NotNull(node5.Extensions);
            Assert.NotNull(node5.Extensions.EXT_mesh_gpu_instancing);
            Assert.NotNull(node5.Extensions.EXT_mesh_gpu_instancing.attributes);
            Assert.AreEqual(13, node5.Extensions.EXT_mesh_gpu_instancing.attributes.TRANSLATION);
            Assert.NotNull(node5.Extensions.KHR_lights_punctual);
            Assert.AreEqual(42, node5.Extensions.KHR_lights_punctual.light);
        }

        [Test]
        public void ParseGarbage()
        {
            var jsonParser = new GltfJsonUtilityParser();
            var gltf = jsonParser.ParseJson(@"");
            Assert.IsNull(gltf);

            gltf = jsonParser.ParseJson(@"garbage");
            Assert.IsNull(gltf);
        }
    }
}
