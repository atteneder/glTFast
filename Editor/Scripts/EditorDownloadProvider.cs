// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GLTFast.Editor
{

    using Loading;

    class EditorDownloadProvider : IEditorDownloadProvider
    {

        public List<GltfAssetDependency> assetDependencies
        {
            get => innerAssetDependencies;
            set => innerAssetDependencies = value;
        }

        public List<GltfAssetDependency> innerAssetDependencies = new List<GltfAssetDependency>();
        private readonly GltfAssetDependency[] previousDependencies;

        public EditorDownloadProvider(GltfAssetDependency[] gltfAssetDependencies)
        {
            previousDependencies = gltfAssetDependencies ?? Array.Empty<GltfAssetDependency>();
        }

#pragma warning disable 1998
        public async  Task<IDownload> Request(Uri url) {
            var req = new SyncFileLoader(GetDependencyFromPreviousImport(url, GltfAssetDependency.Type.Buffer));
            return req;
        }

        public async Task<ITextureDownload> RequestTexture(Uri url,bool nonReadable,bool forceLinear) {
            var req = new SyncTextureLoader(GetDependencyFromPreviousImport(url, GltfAssetDependency.Type.Texture));
            return req;
        }

#pragma warning restore 1998

        private Uri GetDependencyFromPreviousImport(Uri url, GltfAssetDependency.Type type)
        {
            var previousDependency = previousDependencies.FirstOrDefault(d => d.originalUri == url.OriginalString);

            if (previousDependency.type == GltfAssetDependency.Type.Unknown)
            {
                var newDependency = new GltfAssetDependency
                {
                    originalUri = url.OriginalString,
                    type = type,
                };
                innerAssetDependencies.Add(newDependency);
                return new Uri(newDependency.originalUri, UriKind.Relative);
            }
            
            innerAssetDependencies.Add(previousDependency);
            return new Uri(previousDependency.assetPath, UriKind.Relative);
        }
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
        private Texture2D texture;

        public override bool Success => texture != null;
        public IDisposableTexture GetTexture(bool forceSampleLinear)
        {
            return texture.ToDisposableTexture();
        }

        public SyncTextureLoader(Uri url)
            : base(url)
        {
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(url.OriginalString);
            if (texture == null)
            {
                Error = $"Couldn't load texture at {url.OriginalString}";
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            texture = null;
        }
    }
}
