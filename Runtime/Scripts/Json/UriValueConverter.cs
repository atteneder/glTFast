// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using Unity.Collections;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Reads a glTF URI string token into a <see cref="UriValue"/>.
    /// Data URIs are detected by their <c>data:</c> prefix and decoded from base-64 directly
    /// out of the UTF-8 JSON value into a <see cref="NativeArray{T}"/>, bypassing the otherwise
    /// required allocation of a UTF-16 string for the (potentially very large) encoded payload.
    /// </summary>
    /// <remarks>
    /// To recover the decoded <see cref="NativeArray{T}"/> allocations made by the converter when
    /// deserialization throws after one or more data URIs have already been decoded, wrap the
    /// <c>Deserialize</c> call between <see cref="BeginCollect"/> and <see cref="EndCollect"/>;
    /// on exception drain the returned list and dispose each entry.
    /// </remarks>
    class UriValueConverter : JsonConverter<UriValue>
    {
        [ThreadStatic]
        static List<UriValue> s_Pending;

        /// <summary>
        /// Starts collecting Data-state <see cref="UriValue"/>s produced by the converter on the
        /// current thread. Must be paired with <see cref="EndCollect"/>.
        /// </summary>
        internal static void BeginCollect()
        {
            s_Pending = new List<UriValue>();
        }

        /// <summary>
        /// Stops collecting and returns the list of Data-state <see cref="UriValue"/>s produced
        /// since the matching <see cref="BeginCollect"/> call. Returns null if no collection was
        /// active. On the success path the caller discards the list (ownership has moved to
        /// <see cref="Root"/>); on failure the caller disposes every entry.
        /// </summary>
        internal static List<UriValue> EndCollect()
        {
            var list = s_Pending;
            s_Pending = null;
            return list;
        }

        public override UriValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                return null;
            }

            // Fast path: UTF-8 value is a single contiguous span with no escapes.
            UriValue result;
            if (!reader.HasValueSequence && !reader.ValueIsEscaped)
            {
                result = ReadFromUtf8(reader.ValueSpan);
            }
            else
            {
                // Slow path: copy unescaped bytes via reader.CopyString into pooled UTF-8 scratch.
                var length = reader.HasValueSequence
                    ? checked((int)reader.ValueSequence.Length)
                    : reader.ValueSpan.Length;
                var rented = ArrayPool<byte>.Shared.Rent(length);
                try
                {
                    var written = reader.CopyString(rented);
                    result = ReadFromUtf8(rented.AsSpan(0, written));
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }

            if (result != null && result.IsData)
            {
                s_Pending?.Add(result);
            }
            return result;
        }

        static UriValue ReadFromUtf8(ReadOnlySpan<byte> utf8)
        {
            if (!DataUri.IsDataUri(utf8))
            {
                return new UriValue(Encoding.UTF8.GetString(utf8));
            }

            if (!DataUri.TryGetDataUriDescriptor(
                    utf8, out var mimeType, out var startIndex, out var decodedLength))
            {
                return UriValue.Failed;
            }

            var data = new NativeArray<byte>(decodedLength, Allocator.Persistent);
            var status = Base64.DecodeFromUtf8(
                utf8[startIndex..], data.AsSpan(), out _, out var bytesWritten);
            if (status != OperationStatus.Done || bytesWritten != decodedLength)
            {
                data.Dispose();
                return UriValue.Failed;
            }
            return new UriValue(data, mimeType);
        }

        public override void Write(Utf8JsonWriter writer, UriValue value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            writer.WriteStringValue(value.AsString());
        }
    }
}
