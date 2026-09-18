// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    class ExtensionListConverter : JsonConverter<List<EnumOrRawValue<Extension>>>
    {
        static readonly ExtensionValueConverter k_ElementConverter = new();

        public override List<EnumOrRawValue<Extension>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected StartArray token.");
            }

            var list = new List<EnumOrRawValue<Extension>>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                list.Add(k_ElementConverter.Read(ref reader, typeof(EnumOrRawValue<Extension>), options));
            }
            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<EnumOrRawValue<Extension>> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var element in value)
            {
                k_ElementConverter.Write(writer, element, options);
            }
            writer.WriteEndArray();
        }
    }
}
