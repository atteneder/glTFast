// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace GLTFast.Editor
{

    using Loading;

    class EditorDownloadProvider : IDownloadProvider
    {

        public List<GltfAssetDependency> assetDependencies = new List<GltfAssetDependency>();

#pragma warning disable 1998
        public async Task<IDownload> Request(Uri url)
        {
            var dependency = new GltfAssetDependency
            {
                originalUri = url.OriginalString
            };
            assetDependencies.Add(dependency);
            var req = new SyncFileLoader(url);
            return req;
        }

        public async Task<ITextureDownload> RequestTexture(Uri url, bool nonReadable)
        {
            var dependency = new GltfAssetDependency
            {
                originalUri = url.OriginalString,
                type = GltfAssetDependency.Type.Texture
            };
            assetDependencies.Add(dependency);
            var req = new SyncTextureLoader(url);
            return req;
        }
#pragma warning restore 1998
    }

    class SyncFileLoader : IDownload, INativeDownload
    {
        NativeArray<byte> m_FileBytes;
        bool m_Success;

        protected SyncFileLoader() { }

        public SyncFileLoader(Uri url)
        {
            if (NativeFileReader.TryReadAllBytes(url.OriginalString, out m_FileBytes, out var error))
            {
                NativeData = m_FileBytes.AsReadOnly();
                m_Success = true;
            }
            else
            {
                Error = error;
            }
        }

        public object Current => null;
        public bool MoveNext() { return false; }
        public void Reset() { }

        public virtual bool Success => m_Success;

        public string Error { get; protected set; }
        public byte[] Data
        {
            get
            {
                Debug.LogError("Managed byte array `Data` is not used anymore by glTFast and should not be used " +
                    "as it creates a copy. It is maintained to satisfy the IDownload contract.");
                return m_FileBytes.ToArray();
            }
        }

        public NativeArray<byte>.ReadOnly NativeData { get; private set; }

        public string Text => m_Success ? System.Text.Encoding.UTF8.GetString(m_FileBytes.AsReadOnlySpan()) : null;

        public bool? IsBinary
        {
            get
            {
                if (Success)
                {
                    return GltfGlobals.IsGltfBinary(NativeData);
                }
                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_FileBytes.IsCreated)
                    m_FileBytes.Dispose();
                m_FileBytes = default;
                m_Success = false;
                NativeData = default;
            }
        }
    }

    sealed class SyncTextureLoader : SyncFileLoader, ITextureDownload
    {

        public Texture2D Texture { get; private set; }

        public override bool Success => Texture != null;

        public SyncTextureLoader(Uri url)
        {
            Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(url.OriginalString);
            if (Texture == null)
            {
                Error = $"Couldn't load texture at {url.OriginalString}";
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Texture = null;
        }
    }
}
