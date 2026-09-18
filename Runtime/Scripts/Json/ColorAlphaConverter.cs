// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    class ColorAlphaConverter : JsonConverter<ColorAlpha>
    {
        public override ColorAlpha Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected StartArray token for float array.");
            }

            reader.Read();

            var color = new ColorAlpha(0f, 0f, 0f);
            var currentIndex = 0;

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.Number)
                {
                    throw new JsonException($"Expected Number token, but got {reader.TokenType} at index {currentIndex}.");
                }

                var value = (float)FloatParser.GetDouble(reader.ValueSpan);
                switch (currentIndex)
                {
                    case 0:
                        color.R = value;
                        break;
                    case 1:
                        color.G = value;
                        break;
                    case 2:
                        color.B = value;
                        break;
                    case 3:
                        color.A = value;
                        break;
                    default:
                        throw new JsonException($"More than 4 color values in RGBA color array at index {currentIndex}.");
                }

                currentIndex++;
                reader.Read();
            }

            if (currentIndex < 3)
            {
                throw new JsonException("Less than 3 color values in RGBA color array.");
            }

            if (currentIndex < 4)
            {
                color.A = 1f;
            }

            return color;
        }

        public override void Write(Utf8JsonWriter writer, ColorAlpha value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.R);
            writer.WriteNumberValue(value.G);
            writer.WriteNumberValue(value.B);
            writer.WriteNumberValue(value.A);
            writer.WriteEndArray();
        }
    }
}
