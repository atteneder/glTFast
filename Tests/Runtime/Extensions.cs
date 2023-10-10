// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

using GLTFast.Schema;
#if NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif
using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests.JsonParsing
{
    [Category("JsonParsing")]
    class Extensions
    {
        const string k_CustomContent = @"
{
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
    ],
    ""subObject"":{
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
    }
}
";

        static readonly string k_CustomExtensionJson =
$@"
{{
    ""extensions"": {{
        ""CUSTOM_my_extension"":{k_CustomContent},
        ""KHR_lights_punctual"":
        {{
            ""lights"":[{{
                ""type"": ""Directional""
            }}]
        }}
    }}
}}";

        [Serializable]
        class MyExtension : SubClass
        {
            public SubClass subObject;
        }

        [Serializable]
        class SubClass
        {
            public float floatProp;
            public int intProp;
            public string stringProp;
            public float[] eulerAngles;
            public int[] intArrayProp;
            public float[] rotation;
            public float[] color;
        }

        [Serializable]
        class NotMatchingExtension
        {
            // ReSharper disable once NotAccessedField.Local
            public string noMatch;
        }

        [Test]
        public void KnownExtensionOnly()
        {
#if !NEWTONSOFT_JSON
            Assert.Ignore("Requires Newtonsoft JSON package to be installed.");
#else
            const string json = @"
            {
                ""extensions"": {
                    ""KHR_lights_punctual"":
                    {
                        ""lights"":[{
                            ""type"": ""Directional""
                        }]
                    }
                }
            }";

            var gltf = JsonConvert.DeserializeObject<Newtonsoft.Schema.Root>(json);
            Assert.NotNull(gltf);
            Assert.NotNull(gltf.extensions);
            Assert.NotNull(gltf.extensions.KHR_lights_punctual);
            Assert.NotNull(gltf.extensions.KHR_lights_punctual.lights);
            Assert.AreEqual(1, gltf.extensions.KHR_lights_punctual.lights.Length);
            Assert.AreEqual(LightPunctual.Type.Directional, gltf.extensions.KHR_lights_punctual.lights[0].GetLightType());
            Assert.IsFalse(gltf.extensions.TryGetValue<MyExtension>("CUSTOM_my_extension", out var ext));
            Assert.IsNull(ext);
#endif
        }

        [Test]
        public void CustomExtensionOnly()
        {
#if !NEWTONSOFT_JSON
            Assert.Ignore("Requires Newtonsoft JSON package to be installed.");
#else
            var json = $@"
            {{
                ""extensions"": {{
                    ""CUSTOM_my_extension"":{k_CustomContent}
                }}
            }}";

            var gltf = JsonConvert.DeserializeObject<Newtonsoft.Schema.Root>(json);
            Assert.NotNull(gltf);
            Assert.NotNull(gltf.extensions);
            Assert.IsNull(gltf.extensions.KHR_lights_punctual);
            Assert.IsTrue(gltf.extensions.TryGetValue<MyExtension>("CUSTOM_my_extension", out var ext));
            CertifyCustomExtensions(gltf.extensions);
#endif
        }

        [Test]
        public void CustomExtensionJsonUtility()
        {
            var jsonParser = new GltfJsonUtilityParser();
            var gltf = jsonParser.ParseJson(k_CustomExtensionJson);
            Assert.NotNull(gltf);
            Assert.NotNull(gltf.Extensions);
            Assert.NotNull(gltf.Extensions.KHR_lights_punctual);
            Assert.NotNull(gltf.Extensions.KHR_lights_punctual.lights);
            Assert.AreEqual(1, gltf.Extensions.KHR_lights_punctual.lights.Length);
            Assert.AreEqual(LightPunctual.Type.Directional, gltf.Extensions.KHR_lights_punctual.lights[0].GetLightType());
        }

        [Test]
        public void CustomExtensionNewtonsoft()
        {
#if !NEWTONSOFT_JSON
            Assert.Ignore("Requires Newtonsoft JSON package to be installed.");
#else
            var gltf = JsonConvert.DeserializeObject<Newtonsoft.Schema.Root>(k_CustomExtensionJson);
            Assert.NotNull(gltf);
            Assert.NotNull(gltf.extensions);
            Assert.NotNull(gltf.extensions.KHR_lights_punctual);
            Assert.NotNull(gltf.extensions.KHR_lights_punctual.lights);
            Assert.AreEqual(1, gltf.extensions.KHR_lights_punctual.lights.Length);
            Assert.AreEqual(LightPunctual.Type.Directional, gltf.extensions.KHR_lights_punctual.lights[0].GetLightType());
            Assert.IsTrue(gltf.extensions.TryGetValue<MyExtension>("CUSTOM_my_extension", out var ext));
            CertifyCustomExtensions(gltf.extensions);
#endif
        }

        [Test]
        public void CustomExtensionEverywhere()
        {
#if !NEWTONSOFT_JSON
            Assert.Ignore("Requires Newtonsoft JSON package to be installed.");
#else
            var json = $@"
{{
    ""accessors"": [{{
        ""customProperty"": 42,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
        ""sparse"":{{
            ""customProperty"": 420,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
            ""indices"": {{
                ""customProperty"": 4200,
                ""extras"": {{""myKey"": ""myValue""}},
                ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
            }},
            ""values"": {{
                ""customProperty"": 4201,
                ""extras"": {{""myKey"": ""myValue""}},
                ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
            }}
        }}
    }}],
    ""animations"": [{{
        ""customProperty"": 43,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
        ""channels"": [{{
            ""customProperty"": 430,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
            ""target"": {{
                ""customProperty"": 4300,
                ""extras"": {{""myKey"": ""myValue""}},
                ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
            }}
        }}],
        ""samplers"": [{{
            ""customProperty"": 431,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
        }}]
    }}],
    ""asset"": {{
        ""customProperty"": 44,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
        ""version"": ""2.0""
    }},
    ""buffers"": [{{
        ""customProperty"": 45,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
    }}],
    ""bufferViews"": [{{
        ""customProperty"": 46,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
    }}],
    ""cameras"": [{{
        ""customProperty"": 47,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
        ""orthographic"": {{
            ""customProperty"": 470,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
        }},
        ""perspective"": {{
            ""customProperty"": 471,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
        }}
    }}],
    ""customProperty"": 48,
    ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
    ""extensionsRequired"": [""CUSTOM_my_extension""],
    ""extensionsUsed"": [""CUSTOM_my_extension""],
    ""extras"": {{""myKey"": ""myValue""}},
    ""images"": [{{
        ""customProperty"": 49,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
    }}],
    ""materials"": [{{
        ""customProperty"": 50,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
        ""pbrMetallicRoughness"": {{
            ""customProperty"": 500,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
            ""baseColorTexture"": {{
                ""customProperty"": 5000,
                ""extras"": {{""myKey"": ""myValue""}},
                ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
            }},
            ""metallicRoughnessTexture"": {{
                ""customProperty"": 5001,
                ""extras"": {{""myKey"": ""myValue""}},
                ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
            }}
        }},
        ""emissiveTexture"": {{
            ""customProperty"": 501,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
        }},
        ""normalTexture"": {{
            ""customProperty"": 502,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
        }},
        ""occlusionTexture"": {{
            ""customProperty"": 503,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}},
        }}
    }}],
    ""meshes"": [{{
        ""primitives"": [{{
            ""customProperty"": 510,
            ""extras"": {{""myKey"": ""myValue""}},
            ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
        }}],
        ""customProperty"": 51,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
    }}],
    ""nodes"": [{{
        ""customProperty"": 52,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
    }}],
    ""samplers"": [{{
        ""customProperty"": 53,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
    }}],
    ""scene"": 0,
    ""scenes"": [{{
        ""customProperty"": 54,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
    }}],
    ""skins"": [{{
        ""customProperty"": 55,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
    }}],
    ""textures"": [{{
        ""customProperty"": 56,
        ""extras"": {{""myKey"": ""myValue""}},
        ""extensions"": {{""CUSTOM_my_extension"": {k_CustomContent}}}
    }}]
}}";

            var gltf = JsonConvert.DeserializeObject<Newtonsoft.Schema.Root>(json);

            CertifyCustomData(gltf, 48);
            CertifyCustomExtensions(gltf.extensions);
            CertifyCustomExtras(gltf.extras);

            CertifyCustomData(gltf.accessors[0], 42);
            CertifyCustomExtensions(gltf.accessors[0].extensions);
            CertifyCustomExtras(gltf.accessors[0].extras);

            CertifyCustomData(gltf.accessors[0].sparse, 420);
            CertifyCustomExtensions(gltf.accessors[0].sparse.extensions);
            CertifyCustomExtras(gltf.accessors[0].sparse.extras);

            CertifyCustomData(gltf.accessors[0].sparse.indices, 4200);
            CertifyCustomExtensions(gltf.accessors[0].sparse.indices.extensions);
            CertifyCustomExtras(gltf.accessors[0].sparse.indices.extras);

            CertifyCustomData(gltf.accessors[0].sparse.values, 4201);
            CertifyCustomExtensions(gltf.accessors[0].sparse.values.extensions);
            CertifyCustomExtras(gltf.accessors[0].sparse.values.extras);

#if UNITY_ANIMATION
            CertifyCustomData(gltf.animations[0], 43);
            CertifyCustomExtensions(gltf.animations[0].extensions);
            CertifyCustomExtras(gltf.animations[0].extras);

            CertifyCustomData(gltf.animations[0].channels[0], 430);
            CertifyCustomExtensions(gltf.animations[0].channels[0].extensions);
            CertifyCustomExtras(gltf.animations[0].channels[0].extras);

            CertifyCustomData(gltf.animations[0].channels[0].target, 4300);
            CertifyCustomExtensions(gltf.animations[0].channels[0].target.extensions);
            CertifyCustomExtras(gltf.animations[0].channels[0].target.extras);

            CertifyCustomData(gltf.animations[0].samplers[0], 431);
            CertifyCustomExtensions(gltf.animations[0].samplers[0].extensions);
            CertifyCustomExtras(gltf.animations[0].samplers[0].extras);
#endif

            CertifyCustomData(gltf.asset, 44);
            CertifyCustomExtensions(gltf.asset.extensions);
            CertifyCustomExtras(gltf.asset.extras);

            CertifyCustomData(gltf.buffers[0], 45);
            CertifyCustomExtensions(gltf.buffers[0].extensions);
            CertifyCustomExtras(gltf.buffers[0].extras);

            CertifyCustomData(gltf.bufferViews[0], 46);
            CertifyCustomExtensions(gltf.bufferViews[0].extensions);
            CertifyCustomExtras(gltf.bufferViews[0].extras);

            CertifyCustomData(gltf.cameras[0], 47);
            CertifyCustomExtensions(gltf.cameras[0].extensions);
            CertifyCustomExtras(gltf.cameras[0].extras);

            CertifyCustomData(gltf.cameras[0].orthographic, 470);
            CertifyCustomExtensions(gltf.cameras[0].orthographic.extensions);
            CertifyCustomExtras(gltf.cameras[0].orthographic.extras);

            CertifyCustomData(gltf.cameras[0].perspective, 471);
            CertifyCustomExtensions(gltf.cameras[0].perspective.extensions);
            CertifyCustomExtras(gltf.cameras[0].perspective.extras);

            CertifyCustomData(gltf.images[0], 49);
            CertifyCustomExtensions(gltf.images[0].extensions);
            CertifyCustomExtras(gltf.images[0].extras);

            CertifyCustomData(gltf.materials[0], 50);
            CertifyCustomExtensions(gltf.materials[0].extensions);
            CertifyCustomExtras(gltf.materials[0].extras);

            CertifyCustomData(gltf.materials[0].pbrMetallicRoughness, 500);
            CertifyCustomExtensions(gltf.materials[0].pbrMetallicRoughness.extensions);
            CertifyCustomExtras(gltf.materials[0].pbrMetallicRoughness.extras);

            CertifyCustomData(gltf.materials[0].pbrMetallicRoughness.baseColorTexture, 5000);
            CertifyCustomExtensions(gltf.materials[0].pbrMetallicRoughness.baseColorTexture.extensions);
            CertifyCustomExtras(gltf.materials[0].pbrMetallicRoughness.baseColorTexture.extras);

            CertifyCustomData(gltf.materials[0].pbrMetallicRoughness.metallicRoughnessTexture, 5001);
            CertifyCustomExtensions(gltf.materials[0].pbrMetallicRoughness.metallicRoughnessTexture.extensions);
            CertifyCustomExtras(gltf.materials[0].pbrMetallicRoughness.metallicRoughnessTexture.extras);

            CertifyCustomData(gltf.materials[0].emissiveTexture, 501);
            CertifyCustomExtensions(gltf.materials[0].emissiveTexture.extensions);
            CertifyCustomExtras(gltf.materials[0].emissiveTexture.extras);

            CertifyCustomData(gltf.materials[0].normalTexture, 502);
            CertifyCustomExtensions(gltf.materials[0].normalTexture.extensions);
            CertifyCustomExtras(gltf.materials[0].normalTexture.extras);

            CertifyCustomData(gltf.materials[0].occlusionTexture, 503);
            CertifyCustomExtensions(gltf.materials[0].occlusionTexture.extensions);
            CertifyCustomExtras(gltf.materials[0].occlusionTexture.extras);

            CertifyCustomData(gltf.meshes[0], 51);
            CertifyCustomExtensions(gltf.meshes[0].extensions);
            CertifyCustomExtras(gltf.meshes[0].extras);

            CertifyCustomData(gltf.meshes[0].primitives[0], 510);
            CertifyCustomExtensions(gltf.meshes[0].primitives[0].extensions);
            CertifyCustomExtras(gltf.meshes[0].primitives[0].extras);

            CertifyCustomData(gltf.nodes[0], 52);
            CertifyCustomExtensions(gltf.nodes[0].extensions);
            CertifyCustomExtras(gltf.nodes[0].extras);

            CertifyCustomData(gltf.samplers[0], 53);
            CertifyCustomExtensions(gltf.samplers[0].extensions);
            CertifyCustomExtras(gltf.samplers[0].extras);

            CertifyCustomData(gltf.scenes[0], 54);
            CertifyCustomExtensions(gltf.scenes[0].extensions);
            CertifyCustomExtras(gltf.scenes[0].extras);

            CertifyCustomData(gltf.skins[0], 55);
            CertifyCustomExtensions(gltf.skins[0].extensions);
            CertifyCustomExtras(gltf.skins[0].extras);

            CertifyCustomData(gltf.textures[0], 56);
            CertifyCustomExtensions(gltf.textures[0].extensions);
            CertifyCustomExtras(gltf.textures[0].extras);
#endif
        }

#if NEWTONSOFT_JSON
        static void CertifyCustomData(GLTFast.Newtonsoft.Schema.IJsonObject gltf, int expected)
        {
            Assert.NotNull(gltf);
            Assert.IsTrue(gltf.TryGetValue("customProperty", out int prop));
            Assert.AreEqual(expected, prop);
        }

        static void CertifyCustomExtensions(GLTFast.Newtonsoft.Schema.IJsonObject extensions)
        {
            Assert.NotNull(extensions);
            Assert.IsFalse(extensions.TryGetValue<MyExtension>("NO_MATCH", out _));
            Assert.IsTrue(extensions.TryGetValue<MyExtension>("CUSTOM_my_extension", out var ext));
            CertifySubClass(ext);
            CertifySubClass(ext.subObject);
            return;

            void CertifySubClass(SubClass sub)
            {
                Assert.NotNull(sub);
                Assert.AreEqual(3.4700000286102295f, sub.floatProp, "JSON value mismatch");
                Assert.AreEqual(42, sub.intProp, "JSON value mismatch");
                Assert.AreEqual("Yadiya", sub.stringProp, "JSON value mismatch");
                Assert.AreEqual(3, sub.eulerAngles.Length);
                Assert.AreEqual(1.0f, sub.eulerAngles[0]);
                Assert.AreEqual(1.0f, sub.eulerAngles[1]);
                Assert.AreEqual(1.0f, sub.eulerAngles[2]);
                Assert.AreEqual(3, sub.intArrayProp.Length);
                Assert.AreEqual(1, sub.intArrayProp[0]);
                Assert.AreEqual(1, sub.intArrayProp[1]);
                Assert.AreEqual(1, sub.intArrayProp[2]);
                Assert.AreEqual(4, sub.rotation.Length);
                Assert.AreEqual(0.8199999928474426f, sub.rotation[0]);
                Assert.AreEqual(0.8199999928474426f, sub.rotation[1]);
                Assert.AreEqual(0.8199999928474426f, sub.rotation[2]);
                Assert.AreEqual(0.8199999928474426f, sub.rotation[3]);
                Assert.AreEqual(4, sub.color.Length);
                Assert.AreEqual(1.0f, sub.color[0]);
                Assert.AreEqual(1.0f, sub.color[1]);
                Assert.AreEqual(1.0f, sub.color[2]);
                Assert.AreEqual(1.0f, sub.color[3]);
            }
        }

        static void CertifyCustomExtras(GLTFast.Newtonsoft.Schema.IJsonObject extras)
        {
            Assert.NotNull(extras);
            Assert.IsFalse(extras.TryGetValue("NoMatch", out int _));
            Assert.IsTrue(extras.TryGetValue("myKey", out string value));
            Assert.AreEqual("myValue", value);

            // incorrect destination type int (actually is a string)
            Assert.Throws<FormatException>(
                () => extras.TryGetValue("myKey", out int intValue));
        }
#endif

        [Test]
        public void CustomExtensionNowhere()
        {
#if NEWTONSOFT_JSON
            const string json = @"
{
    ""accessors"": [{}],
    ""animations"": [{}],
    ""asset"": {
        ""version"": ""2.0""},
    ""buffers"": [{}],
    ""bufferViews"": [{}],
    ""cameras"": [{}],
    ""images"": [{}],
    ""materials"": [{}],
    ""meshes"": [{}],
    ""nodes"": [{}],
    ""samplers"": [{}],
    ""scenes"": [{}],
    ""skins"": [{}],
    ""textures"": [{}]
}
            ";

            var gltf = JsonConvert.DeserializeObject<Newtonsoft.Schema.Root>(json);

            Assert.NotNull(gltf);
            Assert.IsNull(gltf.extras);
            Assert.IsNull(gltf.extensions);

            Assert.IsNull(gltf.accessors[0].extras);
            Assert.IsNull(gltf.accessors[0].extensions);

#if UNITY_ANIMATION
            Assert.IsNull(gltf.animations[0].extras);
            Assert.IsNull(gltf.animations[0].extensions);
#endif

            Assert.IsNull(gltf.asset.extras);
            Assert.IsNull(gltf.asset.extensions);

            Assert.IsNull(gltf.buffers[0].extras);
            Assert.IsNull(gltf.buffers[0].extensions);

            Assert.IsNull(gltf.bufferViews[0].extras);
            Assert.IsNull(gltf.bufferViews[0].extensions);

            Assert.IsNull(gltf.cameras[0].extras);
            Assert.IsNull(gltf.cameras[0].extensions);

            Assert.IsNull(gltf.images[0].extras);
            Assert.IsNull(gltf.images[0].extensions);

            Assert.IsNull(gltf.materials[0].extras);
            Assert.IsNull(gltf.materials[0].extensions);

            Assert.IsNull(gltf.meshes[0].extras);
            Assert.IsNull(gltf.meshes[0].extensions);

            Assert.IsNull(gltf.nodes[0].extras);
            Assert.IsNull(gltf.nodes[0].Extensions);

            Assert.IsNull(gltf.samplers[0].extras);
            Assert.IsNull(gltf.samplers[0].extensions);

            Assert.IsNull(gltf.scenes[0].extras);
            Assert.IsNull(gltf.scenes[0].extensions);

            Assert.IsNull(gltf.skins[0].extras);
            Assert.IsNull(gltf.skins[0].extensions);

            Assert.IsNull(gltf.textures[0].extras);
            Assert.IsNull(gltf.textures[0].Extensions);
#else
            Assert.Ignore("Requires Newtonsoft JSON package to be installed.");
#endif
        }
    }
}
