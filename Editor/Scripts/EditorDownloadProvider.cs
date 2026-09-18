// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace Unity.Cloud.Gltfast.Editor
{

    using Loading;

    class EditorDownloadProvider : IDownloadProvider
    {

        public List<GltfAssetDependency> assetDependencies = new List<GltfAssetDependency>();

#pragma warning disable 1998
        public async Task<IDownload> RequestAsync(Uri url)
        {
            var dependency = new GltfAssetDependency
            {
                originalUri = url.OriginalString
            };
            assetDependencies.Add(dependency);
            var req = new SyncFileLoader(url);
            return req;
        }

        public async Task<ITextureDownload> RequestTextureAsync(Uri url, bool nonReadable)
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

    class SyncFileLoader : IDownload
    {
        NativeArray<byte> m_FileBytes;
        bool m_Success;

        protected SyncFileLoader() { }

        public SyncFileLoader(Uri url)
        {
            if (NativeFileReader.TryReadAllBytes(url.OriginalString, out m_FileBytes, out var error))
            {
                Data = m_FileBytes.AsReadOnly();
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

        public NativeArray<byte>.ReadOnly Data { get; private set; }

        public string Text => m_Success ? System.Text.Encoding.UTF8.GetString(m_FileBytes.AsReadOnlySpan()) : null;

        public bool? IsBinary
        {
            get
            {
                if (Success)
                {
                    return GltfGlobals.IsGltfBinary(Data);
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
                Data = default;
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
