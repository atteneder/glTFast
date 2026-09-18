// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using Unity.Collections;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Holds the value of a glTF URI field.
    /// For a regular URI (relative path or remote URL) the original string is preserved.
    /// For a base-64 <c>data:</c> URI the payload is decoded into a <see cref="NativeArray{T}"/>
    /// during JSON parsing, avoiding the otherwise required UTF-16 string allocation of the
    /// (potentially very large) encoded payload.
    /// </summary>
    /// <remarks>
    /// Instances that own decoded data must be disposed. <see cref="Buffer"/>, <see cref="Image"/>
    /// and <see cref="Root"/> forward <see cref="IDisposable.Dispose"/> calls automatically.
    /// </remarks>
    [JsonConverter(typeof(UriValueConverter))]
    public sealed class UriValue : IDisposable
    {
        enum UriState : byte
        {
            String,
            Data,
            Failed,
            Disposed,
        }

        UriState m_State;
        string m_String;
        NativeArray<byte> m_Data;

        /// <summary>
        /// Creates a wrapper for a regular URI string (relative path or remote URL).
        /// </summary>
        /// <param name="uri">URI value.</param>
        public UriValue(string uri)
        {
            m_State = UriState.String;
            m_String = uri;
        }

        internal UriValue(NativeArray<byte> data, string mimeType)
        {
            m_State = UriState.Data;
            m_Data = data;
            MimeType = mimeType;
        }

        UriValue(UriState state)
        {
            m_State = state;
        }

        internal static UriValue Failed => new(UriState.Failed);

        /// <summary>
        /// True if this URI is a regular string (not an inlined data URI).
        /// </summary>
        public bool IsString => m_State == UriState.String;

        /// <summary>
        /// True if this URI wraps successfully decoded data URI bytes that have not yet
        /// been taken over by the loader.
        /// </summary>
        public bool IsData => m_State == UriState.Data;

        /// <summary>
        /// True if the URI was recognized as a data URI but decoding the payload failed.
        /// </summary>
        public bool IsFailed => m_State == UriState.Failed;

        /// <summary>
        /// MIME type extracted from a data URI's media type segment.
        /// Returns null for non-data URIs and for failed decodes.
        /// </summary>
        public string MimeType { get; private set; }

        /// <summary>
        /// Returns the URI string for non-data URIs.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this wrapper does not hold a string (i.e. data, transferred or failed state).
        /// There is no implicit fallback to base-64-encode the data: callers requiring a string
        /// representation for a data URI must reconstruct it themselves.
        /// </exception>
        /// <returns>URI string for non-data URIs</returns>
        public string AsString()
        {
            if (m_State != UriState.String)
            {
                throw new InvalidOperationException(
                    $"UriValue.AsString is only valid for string URIs (current state: {m_State}).");
            }
            return m_String;
        }

        /// <summary>
        /// Returns the decoded data URI payload as a non-owning view. The wrapper retains
        /// ownership of the underlying <see cref="NativeArray{T}"/>; callers must not dispose
        /// the returned array. Safe to call any number of times.
        /// </summary>
        /// <param name="data">Receives the decoded bytes on success.</param>
        /// <returns>True if decoded data is available, false otherwise.</returns>
        public bool TryGetData(out NativeArray<byte> data)
        {
            if (m_State != UriState.Data || !m_Data.IsCreated)
            {
                data = default;
                return false;
            }
            data = m_Data;
            return true;
        }

        /// <summary>
        /// Disposes the decoded data URI payload, if owned.
        /// Idempotent: safe to call multiple times and in any state.
        /// </summary>
        public void Dispose()
        {
            if (m_State == UriState.Data && m_Data.IsCreated)
            {
                m_Data.Dispose();
            }
            m_Data = default;
            m_String = null;
            MimeType = null;
            m_State = UriState.Disposed;
        }
    }
}
