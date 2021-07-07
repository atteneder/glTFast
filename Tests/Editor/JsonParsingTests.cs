// Copyright 2020-2021 Andreas Atteneder
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
    public class JsonParsingTests
    {
        [Test]
        public void MaterialExtensions() {
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
            Assert.NotNull(gltf.materials,"No materials");
            Assert.AreEqual(8, gltf.materials.Length, "Invalid material quantity");

            var none = gltf.materials[0];
            Assert.NotNull(none);
            Assert.AreEqual("noExtension",none.name);
            Assert.IsNull(none.extensions);
            
            var empty = gltf.materials[1];
            Assert.NotNull(empty);
            Assert.AreEqual("emptyExtension",empty.name);
            Assert.NotNull(empty.extensions);
            Assert.IsNull(empty.extensions.KHR_materials_unlit);
            Assert.IsNull(empty.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(empty.extensions.KHR_materials_clearcoat);
            Assert.IsNull(empty.extensions.KHR_materials_sheen);
            Assert.IsNull(empty.extensions.KHR_materials_transmission);
            
            var unlit = gltf.materials[2];
            Assert.NotNull(unlit);
            Assert.AreEqual("unlit",unlit.name);
            Assert.NotNull(unlit.extensions);
            Assert.NotNull(unlit.extensions.KHR_materials_unlit);
            Assert.IsNull(unlit.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(unlit.extensions.KHR_materials_clearcoat);
            Assert.IsNull(unlit.extensions.KHR_materials_sheen);
            Assert.IsNull(unlit.extensions.KHR_materials_transmission);
            
            var specGloss = gltf.materials[3];
            Assert.NotNull(specGloss);
            Assert.AreEqual("specularGlossiness",specGloss.name);
            Assert.NotNull(specGloss.extensions);
            Assert.IsNull(specGloss.extensions.KHR_materials_unlit);
            Assert.NotNull(specGloss.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(specGloss.extensions.KHR_materials_clearcoat);
            Assert.IsNull(specGloss.extensions.KHR_materials_sheen);
            Assert.IsNull(specGloss.extensions.KHR_materials_transmission);
            
            var transmission = gltf.materials[4];
            Assert.NotNull(transmission);
            Assert.AreEqual("transmission",transmission.name);
            Assert.NotNull(transmission.extensions);
            Assert.IsNull(transmission.extensions.KHR_materials_unlit);
            Assert.IsNull(transmission.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(transmission.extensions.KHR_materials_clearcoat);
            Assert.IsNull(transmission.extensions.KHR_materials_sheen);
            Assert.NotNull(transmission.extensions.KHR_materials_transmission);
            
            var clearcoat = gltf.materials[5];
            Assert.NotNull(clearcoat);
            Assert.AreEqual("clearcoat",clearcoat.name);
            Assert.NotNull(clearcoat.extensions);
            Assert.IsNull(clearcoat.extensions.KHR_materials_unlit);
            Assert.IsNull(clearcoat.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.NotNull(clearcoat.extensions.KHR_materials_clearcoat);
            Assert.IsNull(clearcoat.extensions.KHR_materials_sheen);
            Assert.IsNull(clearcoat.extensions.KHR_materials_transmission);

            var sheen = gltf.materials[6];
            Assert.NotNull(sheen);
            Assert.AreEqual("sheen",sheen.name);
            Assert.NotNull(sheen.extensions);
            Assert.IsNull(sheen.extensions.KHR_materials_unlit);
            Assert.IsNull(sheen.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.IsNull(sheen.extensions.KHR_materials_clearcoat);
            Assert.NotNull(sheen.extensions.KHR_materials_sheen);
            Assert.IsNull(sheen.extensions.KHR_materials_transmission);
            
            var all = gltf.materials[7];
            Assert.NotNull(all);
            Assert.AreEqual("all",all.name);
            Assert.NotNull(all.extensions);
            Assert.NotNull(all.extensions.KHR_materials_unlit);
            Assert.NotNull(all.extensions.KHR_materials_pbrSpecularGlossiness);
            Assert.NotNull(all.extensions.KHR_materials_clearcoat);
            Assert.NotNull(all.extensions.KHR_materials_sheen);
            Assert.NotNull(all.extensions.KHR_materials_transmission);
        }
        
        [Test]
        public void SparseAccessors() {
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
            Assert.NotNull(gltf.accessors,"No accessors");
            Assert.AreEqual(3, gltf.accessors.Length, "Invalid accessor quantity");

            var regular = gltf.accessors[0];
            Assert.NotNull(regular);
            Assert.IsNull(regular.sparse);
            
            var sparse = gltf.accessors[1];
            Assert.NotNull(sparse);
            Assert.AreEqual(14,sparse.count);
            Assert.NotNull(sparse.sparse);
            Assert.AreEqual(3,sparse.sparse.count);
            Assert.NotNull(sparse.sparse.indices);
            Assert.AreEqual(2,sparse.sparse.indices.bufferView);
            Assert.AreEqual(0,sparse.sparse.indices.byteOffset);
            Assert.AreEqual(GLTFComponentType.UnsignedShort,sparse.sparse.indices.componentType);
            Assert.NotNull(sparse.sparse.values);
            Assert.AreEqual(3,sparse.sparse.values.bufferView);
            Assert.AreEqual(0,sparse.sparse.values.byteOffset);
            
#if GLTFAST_SAFE
            var invalid = gltf.accessors[2];
            Assert.NotNull(invalid);
            Assert.IsNull(invalid.sparse);
#else
            Debug.LogWarning("Invalid Sparse Accessors will break glTFast");
#endif
        }
    }
}
