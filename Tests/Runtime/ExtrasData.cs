// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using NUnit.Framework;
using UnityEngine;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace GLTFast.Tests.JsonParsing
{
    [Category("JsonParsing")]
    class ExtrasData
    {
        const string k_ExtrasDataJson = @"
{
    ""asset"" : {
        ""copyright"" : ""(c) 2022 Andreas Atteneder, CC BY 4.0."",
        ""generator"" : ""Khronos glTF Blender I/O v3.3.27"",
        ""version"" : ""2.0""
    },
    ""scene"" : 0,
    ""scenes"" : [
        {
            ""name"" : ""Scene"",
            ""nodes"" : [
                0
            ]
        }
    ],
    ""nodes"" : [
        {
            ""extras"" : {
                ""floatProp"" : 3.4700000286102295,
                ""intProp"" : 42,
                ""stringProp"" : ""Yadiya"",
                ""eulerAngles"" : [
                    1.0,
                    1.0,
                    1.0
                ],
                ""intArrayProp"" : [
                    1,
                    1,
                    1
                ],
                ""rotation"" : [
                    0.8199999928474426,
                    0.8199999928474426,
                    0.8199999928474426,
                    0.8199999928474426
                ],
                ""color"" : [
                    1.0,
                    1.0,
                    1.0,
                    1.0
                ]
            },
            ""mesh"" : 0,
            ""name"" : ""Cube""
        }
    ],
    ""materials"" : [
        {
            ""doubleSided"" : true,
            ""name"" : ""Material"",
            ""pbrMetallicRoughness"" : {
                ""baseColorFactor"" : [
                    0.800000011920929,
                    0.800000011920929,
                    0.800000011920929,
                    1
                ],
                ""metallicFactor"" : 0,
                ""roughnessFactor"" : 0.5
            }
        }
    ],
    ""meshes"" : [
        {
            ""name"" : ""Cube"",
            ""primitives"" : [
                {
                    ""attributes"" : {
                        ""POSITION"" : 0,
                        ""NORMAL"" : 1
                    },
                    ""indices"" : 2,
                    ""material"" : 0
                }
            ]
        }
    ],
    ""accessors"" : [
        {
            ""bufferView"" : 0,
            ""componentType"" : 5126,
            ""count"" : 3,
            ""max"" : [
                0,
                2,
                1
            ],
            ""min"" : [
                0,
                0,
                -1
            ],
            ""type"" : ""VEC3""
        },
        {
            ""bufferView"" : 1,
            ""componentType"" : 5126,
            ""count"" : 3,
            ""type"" : ""VEC3""
        },
        {
            ""bufferView"" : 2,
            ""componentType"" : 5123,
            ""count"" : 3,
            ""type"" : ""SCALAR""
        }
    ],
    ""bufferViews"" : [
        {
            ""buffer"" : 0,
            ""byteLength"" : 36,
            ""byteOffset"" : 0,
            ""target"" : 34962
        },
        {
            ""buffer"" : 0,
            ""byteLength"" : 36,
            ""byteOffset"" : 36,
            ""target"" : 34962
        },
        {
            ""buffer"" : 0,
            ""byteLength"" : 6,
            ""byteOffset"" : 72,
            ""target"" : 34963
        }
    ],
    ""buffers"" : [
        {
            ""byteLength"" : 80,
            ""uri"" : ""ExtrasData.bin""
        }
    ]
}
";

        [Test]
        public void ExtrasDataTest()
        {
#if !NEWTONSOFT_JSON
            Assert.Ignore("Requires Newtonsoft JSON package to be installed.");
#else
            var gltf = JsonConvert.DeserializeObject<Newtonsoft.Schema.Root>(k_ExtrasDataJson);

            Assert.NotNull(gltf);
            Assert.NotNull(gltf.nodes);
            Assert.GreaterOrEqual(gltf.nodes.Length, 1);
            Assert.NotNull(gltf.nodes[0]);

            AssertResultNewtonsoftJson(gltf);
#endif
        }

#if NEWTONSOFT_JSON
        static void AssertResultNewtonsoftJson(GLTFast.Newtonsoft.Schema.Root gltf)
        {
            Assert.NotNull(gltf);
            var e = gltf.nodes[0].extras;
            Assert.IsNotNull(e);

            Assert.IsTrue(e.TryGetValue("floatProp", out float floatProp));
            Assert.NotNull(floatProp);
            Assert.AreEqual(3.4700000286102295f, floatProp, "JSON value mismatch");

            Assert.IsTrue(e.TryGetValue("intProp", out int intProp));
            Assert.NotNull(intProp);
            Assert.AreEqual(42, intProp, "JSON value mismatch");

            Assert.IsTrue(e.TryGetValue("stringProp", out string stringProp));
            Assert.NotNull(stringProp);
            Assert.AreEqual("Yadiya", stringProp, "JSON value mismatch");

            Assert.IsTrue(e.TryGetValue("eulerAngles", out float[] eulerValues));
            Assert.AreEqual(3, eulerValues.Length);
            Assert.AreEqual(1.0f, eulerValues[0]);
            Assert.AreEqual(1.0f, eulerValues[1]);
            Assert.AreEqual(1.0f, eulerValues[2]);

            Assert.IsTrue(e.TryGetValue("intArrayProp", out int[] intValues));
            Assert.AreEqual(3, intValues.Length);
            Assert.AreEqual(1, intValues[0]);
            Assert.AreEqual(1, intValues[1]);
            Assert.AreEqual(1, intValues[2]);

            Assert.IsTrue(e.TryGetValue("rotation", out float[] rotationValues));
            Assert.AreEqual(4, rotationValues.Length);
            Assert.AreEqual(0.8199999928474426f, rotationValues[0]);
            Assert.AreEqual(0.8199999928474426f, rotationValues[1]);
            Assert.AreEqual(0.8199999928474426f, rotationValues[2]);
            Assert.AreEqual(0.8199999928474426f, rotationValues[3]);

            Assert.IsTrue(e.TryGetValue("color", out float[] colorValues));
            Assert.AreEqual(4, colorValues.Length);
            Assert.AreEqual(1.0f, colorValues[0]);
            Assert.AreEqual(1.0f, colorValues[1]);
            Assert.AreEqual(1.0f, colorValues[2]);
            Assert.AreEqual(1.0f, colorValues[3]);
        }
#endif
    }
}
