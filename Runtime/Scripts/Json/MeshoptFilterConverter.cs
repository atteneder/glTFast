// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if MESHOPT_IS_RECENT

using System;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    class MeshoptFilterConverter : JsonConverter<MeshoptFilter>
    {
        static ReadOnlySpan<byte> None => new[] { (byte)'N', (byte)'O', (byte)'N', (byte)'E' };

        static ReadOnlySpan<byte> Octahedral => new[]
        {
            (byte)'O', (byte)'C', (byte)'T', (byte)'A', (byte)'H',
            (byte)'E', (byte)'D', (byte)'R', (byte)'A', (byte)'L'
        };

        static ReadOnlySpan<byte> Quaternion => new[]
        {
            (byte)'Q', (byte)'U', (byte)'A', (byte)'T', (byte)'E',
            (byte)'R', (byte)'N', (byte)'I', (byte)'O', (byte)'N'
        };

        static ReadOnlySpan<byte> Exponential => new[]
        {
            (byte)'E', (byte)'X', (byte)'P', (byte)'O', (byte)'N',
            (byte)'E', (byte)'N', (byte)'T', (byte)'I', (byte)'A', (byte)'L'
        };

        public override MeshoptFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.ValueTextEquals(Octahedral))
                {
                    return MeshoptFilter.Octahedral;
                }
                if (reader.ValueTextEquals(Quaternion))
                {
                    return MeshoptFilter.Quaternion;
                }
                if (reader.ValueTextEquals(Exponential))
                {
                    return MeshoptFilter.Exponential;
                }
            }
            return MeshoptFilter.None;
        }

        public override void Write(Utf8JsonWriter writer, MeshoptFilter value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case MeshoptFilter.None:
                    writer.WriteStringValue(None);
                    break;
                case MeshoptFilter.Octahedral:
                    writer.WriteStringValue(Octahedral);
                    break;
                case MeshoptFilter.Quaternion:
                    writer.WriteStringValue(Quaternion);
                    break;
                case MeshoptFilter.Exponential:
                    writer.WriteStringValue(Exponential);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
#endif // MESHOPT_IS_RECENT
