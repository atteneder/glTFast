// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using Unity.Cloud.Gltfast.Text.Json.Serialization.Metadata;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// De-/serializes <c>extras</c>, which the glTF specification allows to be any JSON value.
    /// A JSON object is read into <typeparamref name="T"/> as usual; any other value is retained
    /// verbatim in <see cref="ExtrasContainer.RawValueElement"/>.
    /// </summary>
    /// <typeparam name="T">The declared <c>extras</c> type.</typeparam>
    /// <remarks>
    /// Apply to the <c>extras</c> property, never to <typeparamref name="T"/> itself. A type-level
    /// attribute would not reach derived types (attribute lookup does not inherit), and it would make
    /// <see cref="MeshExtrasConverter"/>'s delegation resolve back to itself.
    /// </remarks>
    abstract class ExtrasConverterBase<T> : JsonConverter<T>
        where T : ExtrasContainer, new()
    {
        public sealed override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return ReadObject(ref reader, options);
            }

            // ParseValue copies the value into a document of its own, so this does not keep the glTF
            // JSON buffer alive, and it leaves the reader on the value's last token, as required.
            return new T { RawValueElement = JsonElement.ParseValue(ref reader) };
        }

        public sealed override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value.RawValueElement.ValueKind != JsonValueKind.Undefined)
            {
                value.RawValueElement.WriteTo(writer);
                return;
            }
            WriteObject(writer, value, options);
        }

        protected abstract T ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options);

        protected abstract void WriteObject(Utf8JsonWriter writer, T value, JsonSerializerOptions options);
    }

    /// <summary>
    /// <c>extras</c> converter for containers without declared members.
    /// </summary>
    /// <remarks>
    /// Reads the JSON object directly instead of delegating to the generated converter, which would
    /// re-enter <see cref="JsonSerializer"/> once per <c>extras</c> and cost about a fifth of the
    /// parsing time on documents that carry <c>extras</c> throughout. That is only equivalent as long
    /// as <see cref="ExtrasContainer"/> declares nothing to (de-)serialize, which the
    /// <c>ExtrasContainerHasNoSerializedMembers</c> test asserts.
    /// </remarks>
    sealed class ExtrasConverter : ExtrasConverterBase<ExtrasContainer>
    {
        protected override ExtrasContainer ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Dictionary<string, JsonElement> data = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new ExtrasContainer { ExtensionData = data };
                }

                var key = reader.GetString();
                reader.Read();
                (data ??= new Dictionary<string, JsonElement>())[key] = JsonElement.ParseValue(ref reader);
            }
            throw new JsonException("Incomplete \"extras\" object.");
        }

        protected override void WriteObject(
            Utf8JsonWriter writer,
            ExtrasContainer value,
            JsonSerializerOptions options
            )
        {
            writer.WriteStartObject();
            if (value.ExtensionData != null)
            {
                foreach (var property in value.ExtensionData)
                {
                    writer.WritePropertyName(property.Key);
                    property.Value.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// <c>extras</c> converter for <see cref="MeshExtras"/>, which has a declared member.
    /// </summary>
    /// <remarks>
    /// Delegates the object case to the generated converter, so <see cref="MeshExtras.TargetNames"/>
    /// and any member added later are handled without duplicating generated logic. Meshes are far less
    /// numerous than nodes, so the delegation overhead does not matter here.
    /// </remarks>
    sealed class MeshExtrasConverter : ExtrasConverterBase<MeshExtras>
    {
        // A single converter instance is shared by every thread deserializing through the same
        // context, so this stays stateless. JsonSerializerOptions caches the JsonTypeInfo itself and
        // is thread-safe, making a local cache both redundant and racy.
        static JsonTypeInfo<MeshExtras> GetTypeInfo(JsonSerializerOptions options)
            => (JsonTypeInfo<MeshExtras>)options.GetTypeInfo(typeof(MeshExtras));

        protected override MeshExtras ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(ref reader, GetTypeInfo(options));
        }

        protected override void WriteObject(
            Utf8JsonWriter writer,
            MeshExtras value,
            JsonSerializerOptions options
            )
        {
            JsonSerializer.Serialize(writer, value, GetTypeInfo(options));
        }
    }
}
