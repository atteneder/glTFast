// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using GLTFast.Schema;
using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests
{
    class JsonParsingTests
    {
        [Test]
        public void MaterialExtensions()
        {
            var gltf = JsonParser.ParseJson(@"
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
            Assert.NotNull(gltf.materials, "No materials");
            Assert.AreEqual(8, gltf.materials.Length, "Invalid material quantity");

            var none = gltf.materials[0];
            Assert.NotNull(none);
            Assert.AreEqual("noExtension", none.name);
            Assert.IsNull(none.extensions);

            var empty = gltf.materials[1];
            Assert.NotNull(empty);
            Assert.AreEqual("emptyExtension", empty.name);
            Assert.NotNull(empty.extensions);
            Assert.IsNull(empty.extensions.KHR_materials_unlit);
            Assert.IsNull(empty.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(empty.extensions.KHR_materials_clearcoat);
            Assert.IsNull(empty.extensions.KHR_materials_sheen);
            Assert.IsNull(empty.extensions.KHR_materials_transmission);

            var unlit = gltf.materials[2];
            Assert.NotNull(unlit);
            Assert.AreEqual("unlit", unlit.name);
            Assert.NotNull(unlit.extensions);
            Assert.NotNull(unlit.extensions.KHR_materials_unlit);
            Assert.IsNull(unlit.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(unlit.extensions.KHR_materials_clearcoat);
            Assert.IsNull(unlit.extensions.KHR_materials_sheen);
            Assert.IsNull(unlit.extensions.KHR_materials_transmission);

            var specGloss = gltf.materials[3];
            Assert.NotNull(specGloss);
            Assert.AreEqual("specularGlossiness", specGloss.name);
            Assert.NotNull(specGloss.extensions);
            Assert.IsNull(specGloss.extensions.KHR_materials_unlit);
            Assert.NotNull(specGloss.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(specGloss.extensions.KHR_materials_clearcoat);
            Assert.IsNull(specGloss.extensions.KHR_materials_sheen);
            Assert.IsNull(specGloss.extensions.KHR_materials_transmission);

            var transmission = gltf.materials[4];
            Assert.NotNull(transmission);
            Assert.AreEqual("transmission", transmission.name);
            Assert.NotNull(transmission.extensions);
            Assert.IsNull(transmission.extensions.KHR_materials_unlit);
            Assert.IsNull(transmission.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(transmission.extensions.KHR_materials_clearcoat);
            Assert.IsNull(transmission.extensions.KHR_materials_sheen);
            Assert.NotNull(transmission.extensions.KHR_materials_transmission);

            var clearcoat = gltf.materials[5];
            Assert.NotNull(clearcoat);
            Assert.AreEqual("clearcoat", clearcoat.name);
            Assert.NotNull(clearcoat.extensions);
            Assert.IsNull(clearcoat.extensions.KHR_materials_unlit);
            Assert.IsNull(clearcoat.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.NotNull(clearcoat.extensions.KHR_materials_clearcoat);
            Assert.IsNull(clearcoat.extensions.KHR_materials_sheen);
            Assert.IsNull(clearcoat.extensions.KHR_materials_transmission);

            var sheen = gltf.materials[6];
            Assert.NotNull(sheen);
            Assert.AreEqual("sheen", sheen.name);
            Assert.NotNull(sheen.extensions);
            Assert.IsNull(sheen.extensions.KHR_materials_unlit);
            Assert.IsNull(sheen.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(sheen.extensions.KHR_materials_clearcoat);
            Assert.NotNull(sheen.extensions.KHR_materials_sheen);
            Assert.IsNull(sheen.extensions.KHR_materials_transmission);

            var all = gltf.materials[7];
            Assert.NotNull(all);
            Assert.AreEqual("all", all.name);
            Assert.NotNull(all.extensions);
            Assert.NotNull(all.extensions.KHR_materials_unlit);
            Assert.NotNull(all.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.NotNull(all.extensions.KHR_materials_clearcoat);
            Assert.NotNull(all.extensions.KHR_materials_sheen);
            Assert.NotNull(all.extensions.KHR_materials_transmission);
        }

        [Test]
        public void SparseAccessors()
        {
            var gltf = JsonParser.ParseJson(@"
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
            Assert.NotNull(gltf.accessors, "No accessors");
            Assert.AreEqual(3, gltf.accessors.Length, "Invalid accessor quantity");

            var regular = gltf.accessors[0];
            Assert.NotNull(regular);
            Assert.IsNull(regular.sparse);

            var sparse = gltf.accessors[1];
            Assert.NotNull(sparse);
            Assert.AreEqual(14, sparse.count);
            Assert.NotNull(sparse.sparse);
            Assert.AreEqual(3, sparse.sparse.count);
            Assert.NotNull(sparse.sparse.indices);
            Assert.AreEqual(2, sparse.sparse.indices.bufferView);
            Assert.AreEqual(0, sparse.sparse.indices.byteOffset);
            Assert.AreEqual(GltfComponentType.UnsignedShort, sparse.sparse.indices.componentType);
            Assert.NotNull(sparse.sparse.values);
            Assert.AreEqual(3, sparse.sparse.values.bufferView);
            Assert.AreEqual(0, sparse.sparse.values.byteOffset);

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
            var gltf = JsonParser.ParseJson(@"
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
            Assert.NotNull(gltf.meshes, "No materials");
            Assert.AreEqual(2, gltf.meshes.Length, "Invalid materials quantity");

            var mat = gltf.meshes[0];
            Assert.NotNull(mat);
            Assert.NotNull(mat.extras);
            Assert.NotNull(mat.extras.targetNames);
            Assert.NotNull(mat.extras.targetNames);
            Assert.AreEqual(2, mat.extras.targetNames.Length, "Invalid targetNames quantity");
            Assert.AreEqual("Key 1", mat.extras.targetNames[0]);
            Assert.AreEqual("Key 2", mat.extras.targetNames[1]);

            mat = gltf.meshes[1];
            Assert.NotNull(mat);
            Assert.NotNull(mat.extras);
            Assert.IsNull(mat.extras.targetNames);
        }

        [Test]
        public void MinMagFilter()
        {
            var gltf = JsonParser.ParseJson(@"
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
            Assert.NotNull(gltf.samplers, "No samplers");
            Assert.AreEqual(8, gltf.samplers.Length, "Invalid samplers quantity");

            var sampler0 = gltf.samplers[0];
            Assert.NotNull(sampler0);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler0.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.None, sampler0.minFilter);

            var sampler1 = gltf.samplers[1];
            Assert.NotNull(sampler1);
            Assert.AreEqual((Sampler.MagFilterMode)100, sampler1.magFilter);
            Assert.AreEqual((Sampler.MinFilterMode)100, sampler1.minFilter);

            var sampler2 = gltf.samplers[2];
            Assert.NotNull(sampler2);
            Assert.AreEqual(Sampler.MagFilterMode.Nearest, sampler2.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.Nearest, sampler2.minFilter);

            var sampler3 = gltf.samplers[3];
            Assert.NotNull(sampler3);
            Assert.AreEqual(Sampler.MagFilterMode.Linear, sampler3.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.Linear, sampler3.minFilter);

            var sampler4 = gltf.samplers[4];
            Assert.NotNull(sampler4);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler4.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.NearestMipmapNearest, sampler4.minFilter);

            var sampler5 = gltf.samplers[5];
            Assert.NotNull(sampler5);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler5.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.LinearMipmapNearest, sampler5.minFilter);

            var sampler6 = gltf.samplers[6];
            Assert.NotNull(sampler6);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler6.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.NearestMipmapLinear, sampler6.minFilter);

            var sampler7 = gltf.samplers[7];
            Assert.NotNull(sampler7);
            Assert.AreEqual(Sampler.MagFilterMode.None, sampler7.magFilter);
            Assert.AreEqual(Sampler.MinFilterMode.LinearMipmapLinear, sampler7.minFilter);

        }

        [Test]
        public void UnknownNodeExtension()
        {
            var gltf = JsonParser.ParseJson(@"
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
            Assert.NotNull(gltf.nodes, "No nodes");
            Assert.AreEqual(6, gltf.nodes.Length, "Invalid nodes quantity");

            var node0 = gltf.nodes[0];
            Assert.NotNull(node0);
            Assert.IsNull(node0.extensions);

            var node1 = gltf.nodes[1];
            Assert.NotNull(node1);
            Assert.IsNull(node1.extensions);

            var node2 = gltf.nodes[2];
            Assert.NotNull(node2);
            Assert.IsNull(node2.extensions);

            var node3 = gltf.nodes[3];
            Assert.NotNull(node3);
            Assert.NotNull(node3.extensions);
            Assert.NotNull(node3.extensions.EXT_mesh_gpu_instancing);
            Assert.NotNull(node3.extensions.EXT_mesh_gpu_instancing.attributes);
            Assert.AreEqual(42, node3.extensions.EXT_mesh_gpu_instancing.attributes.TRANSLATION);
            Assert.IsNull(node3.extensions.KHR_lights_punctual);

            var node4 = gltf.nodes[4];
            Assert.NotNull(node4);
            Assert.NotNull(node4.extensions);
            Assert.IsNull(node4.extensions.EXT_mesh_gpu_instancing);
            Assert.NotNull(node4.extensions.KHR_lights_punctual);
            Assert.AreEqual(42, node4.extensions.KHR_lights_punctual.light);

            var node5 = gltf.nodes[5];
            Assert.NotNull(node5);
            Assert.NotNull(node5.extensions);
            Assert.NotNull(node5.extensions.EXT_mesh_gpu_instancing);
            Assert.NotNull(node5.extensions.EXT_mesh_gpu_instancing.attributes);
            Assert.AreEqual(13, node5.extensions.EXT_mesh_gpu_instancing.attributes.TRANSLATION);
            Assert.NotNull(node5.extensions.KHR_lights_punctual);
            Assert.AreEqual(42, node5.extensions.KHR_lights_punctual.light);
        }

        [Test]
        public void ParseGarbage()
        {
            var gltf = JsonParser.ParseJson(@"");
            Assert.IsNull(gltf);

            gltf = JsonParser.ParseJson(@"garbage");
            Assert.IsNull(gltf);
        }
    }
}
