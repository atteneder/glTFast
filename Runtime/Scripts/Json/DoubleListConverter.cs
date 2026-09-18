// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    class DoubleListConverter : JsonConverter<List<double>>
    {
        public override List<double> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected StartArray token for double array.");
            }

            reader.Read();

            var result = new List<double>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.Number)
                {
                    throw new JsonException($"Expected Number token, but got {reader.TokenType} at index {result.Count}.");
                }

                result.Add(FloatParser.GetDouble(reader.ValueSpan));
                reader.Read();
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, List<double> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
        }
    }
}
