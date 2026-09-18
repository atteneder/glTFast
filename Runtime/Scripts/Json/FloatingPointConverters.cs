// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using Unity.Mathematics;

namespace Unity.Cloud.Gltfast.Objects
{
    class Double3Converter : JsonConverter<double3>
    {
        public override double3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new double3();
            DoubleFixedSizeReader.Begin(ref reader);
            result.x = DoubleFixedSizeReader.Next(ref reader);
            result.y = DoubleFixedSizeReader.Next(ref reader);
            result.z = DoubleFixedSizeReader.Next(ref reader);
            DoubleFixedSizeReader.End(ref reader, 3);
            return result;
        }

        public override void Write(Utf8JsonWriter writer, double3 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteNumberValue(value.z);
            writer.WriteEndArray();
        }
    }

    class Double4Converter : JsonConverter<double4>
    {
        public override double4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new double4();
            DoubleFixedSizeReader.Begin(ref reader);
            result.x = DoubleFixedSizeReader.Next(ref reader);
            result.y = DoubleFixedSizeReader.Next(ref reader);
            result.z = DoubleFixedSizeReader.Next(ref reader);
            result.w = DoubleFixedSizeReader.Next(ref reader);
            DoubleFixedSizeReader.End(ref reader, 4);
            return result;
        }

        public override void Write(Utf8JsonWriter writer, double4 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteNumberValue(value.z);
            writer.WriteNumberValue(value.w);
            writer.WriteEndArray();
        }
    }

    class Double4x4Converter : JsonConverter<double4x4>
    {
        public override double4x4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // glTF stores the matrix as 16 doubles in column-major order, matching double4x4's column layout.
            double4 c0 = default, c1 = default, c2 = default, c3 = default;
            DoubleFixedSizeReader.Begin(ref reader);
            c0.x = DoubleFixedSizeReader.Next(ref reader);
            c0.y = DoubleFixedSizeReader.Next(ref reader);
            c0.z = DoubleFixedSizeReader.Next(ref reader);
            c0.w = DoubleFixedSizeReader.Next(ref reader);
            c1.x = DoubleFixedSizeReader.Next(ref reader);
            c1.y = DoubleFixedSizeReader.Next(ref reader);
            c1.z = DoubleFixedSizeReader.Next(ref reader);
            c1.w = DoubleFixedSizeReader.Next(ref reader);
            c2.x = DoubleFixedSizeReader.Next(ref reader);
            c2.y = DoubleFixedSizeReader.Next(ref reader);
            c2.z = DoubleFixedSizeReader.Next(ref reader);
            c2.w = DoubleFixedSizeReader.Next(ref reader);
            c3.x = DoubleFixedSizeReader.Next(ref reader);
            c3.y = DoubleFixedSizeReader.Next(ref reader);
            c3.z = DoubleFixedSizeReader.Next(ref reader);
            c3.w = DoubleFixedSizeReader.Next(ref reader);
            DoubleFixedSizeReader.End(ref reader, 16);
            return new double4x4(c0, c1, c2, c3);
        }

        public override void Write(Utf8JsonWriter writer, double4x4 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            WriteColumn(writer, value.c0);
            WriteColumn(writer, value.c1);
            WriteColumn(writer, value.c2);
            WriteColumn(writer, value.c3);
            writer.WriteEndArray();
        }

        static void WriteColumn(Utf8JsonWriter writer, double4 column)
        {
            writer.WriteNumberValue(column.x);
            writer.WriteNumberValue(column.y);
            writer.WriteNumberValue(column.z);
            writer.WriteNumberValue(column.w);
        }
    }

    class Float2Converter : JsonConverter<float2>
    {
        public override float2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new float2();
            DoubleFixedSizeReader.Begin(ref reader);
            result.x = (float)DoubleFixedSizeReader.Next(ref reader);
            result.y = (float)DoubleFixedSizeReader.Next(ref reader);
            DoubleFixedSizeReader.End(ref reader, 2);
            return result;
        }

        public override void Write(Utf8JsonWriter writer, float2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteEndArray();
        }
    }

    static class DoubleFixedSizeReader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Begin(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected StartArray token for double array.");
            }
            reader.Read();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Next(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Expected Number token, but got {reader.TokenType}.");
            }
            var value = FloatParser.GetDouble(reader.ValueSpan);
            reader.Read();
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void End(ref Utf8JsonReader reader, int length)
        {
            if (reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException($"Expected array length {length} exceeded.");
            }
        }
    }
}
