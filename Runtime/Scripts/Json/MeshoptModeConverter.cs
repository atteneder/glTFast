// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if MESHOPT_IS_RECENT

using System;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    class MeshoptModeConverter : JsonConverter<MeshoptMode>
    {
        static ReadOnlySpan<byte> k_ModeAttributes => new[] {
            (byte)'A', (byte)'T', (byte)'T', (byte)'R', (byte)'I',
            (byte)'B', (byte)'U', (byte)'T', (byte)'E', (byte)'S'
        };

        static ReadOnlySpan<byte> k_ModeTriangles => new[] {
            (byte)'T', (byte)'R', (byte)'I', (byte)'A', (byte)'N',
            (byte)'G', (byte)'L', (byte)'E', (byte)'S'
        };

        static ReadOnlySpan<byte> k_ModeIndices => new[] {
            (byte)'I', (byte)'N', (byte)'D', (byte)'I', (byte)'C',
            (byte)'E', (byte)'S'
        };

        public override MeshoptMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.ValueTextEquals(k_ModeAttributes))
                {
                    return MeshoptMode.Attributes;
                }
                if (reader.ValueTextEquals(k_ModeTriangles))
                {
                    return MeshoptMode.Triangles;
                }
                if (reader.ValueTextEquals(k_ModeIndices))
                {
                    return MeshoptMode.Indices;
                }
            }
            return MeshoptMode.Undefined;
        }

        public override void Write(Utf8JsonWriter writer, MeshoptMode value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case MeshoptMode.Attributes:
                    writer.WriteStringValue(k_ModeAttributes);
                    break;
                case MeshoptMode.Triangles:
                    writer.WriteStringValue(k_ModeTriangles);
                    break;
                case MeshoptMode.Indices:
                    writer.WriteStringValue(k_ModeIndices);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
#endif // MESHOPT_IS_RECENT
