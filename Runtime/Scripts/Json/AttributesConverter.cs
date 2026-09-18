// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    class AttributesConverter : JsonConverter<Attributes>
    {
        static ReadOnlySpan<byte> k_Position => new[] {
            (byte)'P', (byte)'O', (byte)'S', (byte)'I', (byte)'T', (byte)'I', (byte)'O', (byte)'N'
        };
        static ReadOnlySpan<byte> k_Normal => new[] {
            (byte)'N', (byte)'O', (byte)'R', (byte)'M', (byte)'A', (byte)'L'
        };
        static ReadOnlySpan<byte> k_Tangent => new[] {
            (byte)'T', (byte)'A', (byte)'N', (byte)'G', (byte)'E', (byte)'N', (byte)'T'
        };
        static ReadOnlySpan<byte> k_TexCoord => new[] {
            (byte)'T', (byte)'E', (byte)'X', (byte)'C', (byte)'O', (byte)'O', (byte)'R', (byte)'D', (byte)'_'
        };
        static ReadOnlySpan<byte> k_Color => new[] {
            (byte)'C', (byte)'O', (byte)'L', (byte)'O', (byte)'R', (byte)'_'
        };
        static ReadOnlySpan<byte> k_Joints => new[] {
            (byte)'J', (byte)'O', (byte)'I', (byte)'N', (byte)'T', (byte)'S', (byte)'_'
        };
        static ReadOnlySpan<byte> k_Weights => new[] {
            (byte)'W', (byte)'E', (byte)'I', (byte)'G', (byte)'H', (byte)'T', (byte)'S', (byte)'_'
        };

        public override Attributes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token for Attributes.");
            }

            var result = new Attributes();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return result;
                }
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException($"Expected PropertyName, but got {reader.TokenType}.");
                }

                if (reader.ValueTextEquals(k_Position))
                {
                    reader.Read();
                    result.Position = reader.GetInt32();
                }
                else if (reader.ValueTextEquals(k_Normal))
                {
                    reader.Read();
                    result.Normal = reader.GetInt32();
                }
                else if (reader.ValueTextEquals(k_Tangent))
                {
                    reader.Read();
                    result.Tangent = reader.GetInt32();
                }
                else if (TryGetIndex(ref reader, k_TexCoord, out var index))
                {
                    reader.Read();
                    result.SetTexCoord(index, reader.GetInt32());
                }
                else if (TryGetIndex(ref reader, k_Color, out index))
                {
                    reader.Read();
                    result.SetColor(index, reader.GetInt32());
                }
                else if (TryGetIndex(ref reader, k_Joints, out index))
                {
                    reader.Read();
                    result.SetJoint(index, reader.GetInt32());
                }
                else if (TryGetIndex(ref reader, k_Weights, out index))
                {
                    reader.Read();
                    result.SetWeight(index, reader.GetInt32());
                }
                else
                {
                    var name = reader.GetString();
                    reader.Read();
                    (result.ExtensionData ??= new Dictionary<string, JsonElement>())[name] = JsonElement.ParseValue(ref reader);
                }
            }

            throw new JsonException("Unexpected end of JSON while reading Attributes.");
        }

        public override void Write(Utf8JsonWriter writer, Attributes value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value.Position.HasValue) writer.WriteNumber(k_Position, value.Position.Value);
            if (value.Normal.HasValue) writer.WriteNumber(k_Normal, value.Normal.Value);
            if (value.Tangent.HasValue) writer.WriteNumber(k_Tangent, value.Tangent.Value);
            WriteChannel(writer, k_TexCoord, value.TexCoords);
            WriteChannel(writer, k_Color, value.Colors);
            WriteChannel(writer, k_Joints, value.Joints);
            WriteChannel(writer, k_Weights, value.Weights);
            if (value.ExtensionData != null)
            {
                foreach (var pair in value.ExtensionData)
                {
                    writer.WritePropertyName(pair.Key);
                    pair.Value.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
        }

        static bool TryGetIndex(ref Utf8JsonReader reader, ReadOnlySpan<byte> prefix, out int index)
        {
            if (reader.HasValueSequence)
            {
                var sequence = reader.ValueSequence;
                var length = (int)sequence.Length;
                if (length > 32)
                {
                    index = 0;
                    return false;
                }
                Span<byte> scratch = stackalloc byte[32];
                sequence.CopyTo(scratch);
                return TryParseIndex(scratch[..length], prefix, out index);
            }
            return TryParseIndex(reader.ValueSpan, prefix, out index);
        }

        static bool TryParseIndex(ReadOnlySpan<byte> span, ReadOnlySpan<byte> prefix, out int index)
        {
            if (span.Length <= prefix.Length || !span.StartsWith(prefix))
            {
                index = 0;
                return false;
            }
            var tail = span[prefix.Length..];
            index = 0;
            foreach (var b in tail)
            {
                if (b is < (byte)'0' or > (byte)'9')
                {
                    return false;
                }
                index = index * 10 + (b - (byte)'0');
            }
            return true;
        }

        static void WriteChannel(Utf8JsonWriter writer, ReadOnlySpan<byte> prefix, List<int?> list)
        {
            if (list == null) return;
            Span<byte> buffer = stackalloc byte[prefix.Length + 11]; // int = up to 11 ASCII bytes incl. sign
            prefix.CopyTo(buffer);
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].HasValue)
                {
                    Utf8Formatter.TryFormat(i, buffer[prefix.Length..], out var written);
                    writer.WriteNumber(buffer[..(prefix.Length + written)], list[i].Value);
                }
            }
        }
    }
}
