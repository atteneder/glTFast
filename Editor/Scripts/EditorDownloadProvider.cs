// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

    class SyncFileLoader : IDownload
    {
        public SyncFileLoader(Uri url)
        {
            var path = url.OriginalString;
            if (File.Exists(path))
            {
                Data = File.ReadAllBytes(path);
            }
            else
            {
                Error = $"Cannot find resource at path {path}";
            }
        }

        public object Current => null;
        public bool MoveNext() { return false; }
        public void Reset() { }

        public virtual bool Success => Data != null;

        public string Error { get; protected set; }
        public byte[] Data { get; private set; }

        public string Text => System.Text.Encoding.UTF8.GetString(Data);

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

        public virtual void Dispose()
        {
            Data = null;
        }
    }

    class SyncTextureLoader : SyncFileLoader, ITextureDownload
    {

        public Texture2D Texture { get; private set; }

        public override bool Success => Texture != null;

        public SyncTextureLoader(Uri url)
            : base(url)
        {
            Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(url.OriginalString);
            if (Texture == null)
            {
                Error = $"Couldn't load texture at {url.OriginalString}";
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Texture = null;
        }
    }
}
