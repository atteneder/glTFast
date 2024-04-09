// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using GLTFast.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace GLTFast.Tests.JsonParsing
{
    class JsonWriterTests
    {
        const string k_ValueWithAllSpecialChars = "this\fone\\is\not even\remotely\"\tight\"";
        [Test]
        public void Start()
        {
            var json = CreateJsonTest(writer =>
            {
                writer.AddProperty("objectKey");
                writer.AddObject();
                writer.AddProperty("stringKey", "validValue");
                writer.AddProperty("intKey", 42);
                writer.AddProperty("floatKey", 42.42f);
                writer.AddProperty("booleanKey", true);
                writer.Close();
                writer.AddArrayProperty("arrayKey", new int[] { 1, 2, 3 });
            });

            Assert.AreEqual(
                @"{""objectKey"":{""stringKey"":""validValue"",""intKey"":42,""floatKey"":42.42,""booleanKey"":true},""arrayKey"":[1,2,3]}",
                json
                );
        }

        [Test]
        public void InvalidStringKey()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                CreateJsonTest(writer =>
                {
                    writer.AddProperty("string\nKey", "validValue");
                })
            );
        }

        [Test]
        public void InvalidIntegerKey()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                CreateJsonTest(writer =>
                {
                    writer.AddProperty("int\\Key", 42);
                })
            );
        }

        [Test]
        public void InvalidFloatKey()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                CreateJsonTest(writer =>
                {
                    writer.AddProperty("float\tKey", 42.42f);
                })
            );
        }

        [Test]
        public void InvalidBoolString()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                CreateJsonTest(writer =>
                {
                    writer.AddProperty("bool\rKey", false);
                })
            );
        }

        [Test]
        public void InvalidObjectKey()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                CreateJsonTest(writer =>
                {
                    writer.AddProperty("object\fKey");
                })
            );
        }

        [Test]
        public void InvalidArrayKey()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                CreateJsonTest(writer =>
                {
                    writer.AddArrayProperty("array\"Key", new[] { 1, 2, 3 });
                })
            );
        }

        [Test]
        public void InvalidStringValue()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                CreateJsonTest(writer =>
                {
                    writer.AddProperty("key", "string\nvalue");
                })
            );
        }

        [Test]
        public void InvalidArrayValue()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
                CreateJsonTest(writer =>
                {
                    writer.AddArrayProperty("array", new[] { "this one is valid", "this one\nis not" });
                })
            );
        }

        [Test]
        public void EscapedStringValue()
        {
            var json = CreateJsonTest(writer =>
            {
                writer.AddPropertySafe("key", k_ValueWithAllSpecialChars);
            });

            Assert.AreEqual(
                @"{""key"":""this\fone\\is\not even\remotely\""\tight\""""}",
                json
            );
        }

        [Test]
        public void EscapedArrayValue()
        {
            var json = CreateJsonTest(writer =>
            {
                writer.AddArrayPropertySafe("array", new[] { "this one is valid", k_ValueWithAllSpecialChars });
            });

            Assert.AreEqual(
                @"{""array"":[""this one is valid"",""this\fone\\is\not even\remotely\""\tight\""""]}",
                json
            );
        }

        static string CreateJsonTest(Action<JsonWriter> func)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var jsonWriter = new JsonWriter(writer);

            func(jsonWriter);

            jsonWriter.Close();
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(stream);
            var result = reader.ReadToEnd();
            writer.Close();

            var jObject = JObject.Parse(result);
            Assert.NotNull(jObject);

            return result;
        }
    }
}
