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
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GLTFast.Loading {
    
    /// <summary>
    /// Default <see cref="IDownloadProvider"/> implementation
    /// </summary>
    public class DefaultDownloadProvider : IDownloadProvider {
        
        /// <summary>
        /// Sends a URI request
        /// </summary>
        /// <param name="url">URI to request</param>
        /// <returns>Object representing the request</returns>
        public async  Task<IDownload> Request(Uri url) {
            var req = new AwaitableDownload(url);
            while (req.MoveNext()) {
                await Task.Yield();
            }
            return req;
        }

        /// <summary>
        /// Sends a URI request to load a texture
        /// </summary>
        /// <param name="url">URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not CPU readable (uses less memory)</param>
        /// <returns>Object representing the request</returns>
#pragma warning disable CS1998
        public async Task<ITextureDownload> RequestTexture(Uri url,bool nonReadable) {
#pragma warning restore CS1998
#if UNITY_WEBREQUEST_TEXTURE
            var req = new AwaitableTextureDownload(url,nonReadable);
            while (req.MoveNext()) {
                await Task.Yield();
            }
            return req;
#else
            return null;
#endif
        }
    }

    /// <summary>
    /// Default <see cref="IDownload"/> implementation that loads URIs via <see cref="UnityWebRequest"/>
    /// </summary>
    public class AwaitableDownload : IDownload {
        const string GLB_MIME = "model/gltf-binary";
        const string GLTF_MIME = "model/gltf+json";

        /// <summary>
        /// <see cref="UnityWebRequest"/> that is used for the download
        /// </summary>
        protected UnityWebRequest m_Request;
        
        /// <summary>
        /// The download's <see cref="UnityWebRequestAsyncOperation"/>
        /// </summary>
        protected UnityWebRequestAsyncOperation m_AsyncOperation;

        /// <summary>
        /// Empty constructor
        /// </summary>
        protected AwaitableDownload() {}

        /// <summary>
        /// Creates a download of a URI
        /// </summary>
        /// <param name="url">URI to request</param>
        public AwaitableDownload(Uri url) {
            Init(url);
        }

        void Init(Uri url) {
            m_Request = UnityWebRequest.Get(url);
            m_AsyncOperation = m_Request.SendWebRequest();
        }

        /// <summary>
        /// Part of <see cref="IEnumerator"/>, the mechanism that is used for async download
        /// </summary>
        public object Current { get { return m_AsyncOperation; } }
        
        /// <summary>
        /// Part of <see cref="IEnumerator"/>, the mechanism that is used for async download
        /// </summary>
        /// <returns>True if the download is still pending; false if the download has finished.</returns>
        public bool MoveNext() { return !m_AsyncOperation.isDone; }
        
        /// <summary>
        /// Not used. Part of <see cref="IEnumerator"/>, the mechanism that is used for async download.
        /// </summary>
        public void Reset() {}

        /// <summary>
        /// True if the download finished and was successful
        /// </summary>
#if UNITY_2020_1_OR_NEWER
        public bool success => m_Request.isDone && m_Request.result == UnityWebRequest.Result.Success;
#else
        public bool success => m_Request.isDone && !m_Request.isNetworkError && !m_Request.isHttpError;
#endif

        /// <summary>
        /// If the download failed, error description
        /// </summary>
        public string error { get { return m_Request.error; } }
        
        /// <summary>
        /// Downloaded data as byte array
        /// </summary>
        public byte[] data { get { return m_Request.downloadHandler.data; } }
        
        /// <summary>
        /// Downloaded data as string
        /// </summary>
        public string text { get { return m_Request.downloadHandler.text; } }
        
        /// <summary>
        /// True if the requested download is a glTF-Binary file.
        /// False if it is a regular JSON-based glTF file.
        /// Null if the type could not be determined.
        /// </summary>
        public bool? isBinary
        {
            get
            {
                if (success)
                {
                    string contentType = m_Request.GetResponseHeader("Content-Type");
                    if (contentType == GLB_MIME)
                        return true;
                    if (contentType == GLTF_MIME)
                        return false;
                }
                return null;
            }
        }
    }

#if UNITY_WEBREQUEST_TEXTURE
    /// <summary>
    /// Default <see cref="ITextureDownload"/> implementation that loads
    /// texture URIs via <seealso cref="UnityWebRequest"/>.
    /// </summary>
    public class AwaitableTextureDownload : AwaitableDownload, ITextureDownload {
        
        /// <summary>
        /// Parameter-less constructor, required for inheritance.
        /// </summary>
        protected AwaitableTextureDownload() {}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="url">Texture URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not CPU readable (uses less memory)</param>
        public AwaitableTextureDownload(Uri url, bool nonReadable) {
            Init(url,nonReadable);
        }

        /// <summary>
        /// Generates the UnityWebRequest used for sending the request.
        /// </summary>
        /// <param name="url">Texture URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not CPU readable (uses less memory)</param>
        /// <returns>UnityWebRequest used for sending the request</returns>
        protected static UnityWebRequest CreateRequest(Uri url, bool nonReadable) {
            return UnityWebRequestTexture.GetTexture(url,nonReadable);
        }

        void Init(Uri url, bool nonReadable) {
            m_Request = CreateRequest(url,nonReadable);
            m_AsyncOperation = m_Request.SendWebRequest();
        }

        /// <inheritdoc />
        public Texture2D texture {
            get {
                return (m_Request.downloadHandler as  DownloadHandlerTexture ).texture;
            }
        }
    }
#endif
}
