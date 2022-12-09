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
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace GLTFast.Loading
{

    /// <summary>
    /// Represents an HTTP request header key-value pair
    /// </summary>
    [Serializable]
    public struct HttpHeader
    {
        /// <summary>
        /// HTTP header key/name
        /// </summary>
        public string Key => key;

        /// <summary>
        /// HTTP header value
        /// </summary>
        public string Value => value;

        [SerializeField] string key;
        [SerializeField] string value;

        /// <summary>
        /// Creates a new HTTP header entry
        /// </summary>
        /// <param name="key">Identifier</param>
        /// <param name="value">Value</param>
        public HttpHeader(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    /// <summary>
    /// DownloadProvider that sends HTTP request with custom header entries
    /// </summary>
    public class CustomHeaderDownloadProvider : IDownloadProvider
    {

        HttpHeader[] m_Headers;

        /// <summary>
        /// Creates a new CustomHeaderDownloadProvider with a given set of HTTP request header entries
        /// </summary>
        /// <param name="headers">HTTP request header entries to send</param>
        public CustomHeaderDownloadProvider(HttpHeader[] headers)
        {
            m_Headers = headers;
        }

        /// <inheritdoc />
        public async Task<IDownload> Request(Uri url)
        {
            var req = new CustomHeaderDownload(url, RegisterHttpHeaders);
            await req.WaitAsync();
            return req;
        }

        /// <inheritdoc />
#pragma warning disable CS1998
        public async Task<ITextureDownload> RequestTexture(Uri url, bool nonReadable)
        {
#pragma warning restore CS1998
#if UNITY_WEBREQUEST_TEXTURE
            var req = new CustomHeaderTextureDownload(url,nonReadable,RegisterHttpHeaders);
            await req.WaitAsync();
            return req;
#else
            return null;
#endif
        }

        void RegisterHttpHeaders(UnityWebRequest request)
        {
            if (m_Headers != null)
            {
                foreach (var header in m_Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
        }
    }

    /// <summary>
    /// Download that allows modifying the HTTP request before it's sent
    /// </summary>
    public class CustomHeaderDownload : AwaitableDownload
    {

        /// <summary>
        /// Constructs an <see cref="IDownload"/> with a modifier
        /// </summary>
        /// <param name="url">URI to request</param>
        /// <param name="editor">Callback that modifies the UnityWebRequest before it's sent</param>
        public CustomHeaderDownload(Uri url, Action<UnityWebRequest> editor)
        {
            m_Request = UnityWebRequest.Get(url);
            editor(m_Request);
            m_AsyncOperation = m_Request.SendWebRequest();
        }
    }

#if UNITY_WEBREQUEST_TEXTURE
    /// <summary>
    /// Texture download that allows modifying the HTTP request before it's sent
    /// </summary>
    public class CustomHeaderTextureDownload : AwaitableTextureDownload {

        /// <summary>
        /// Constructs an <see cref="ITextureDownload"/> with a modifier
        /// </summary>
        /// <param name="url">URI to request</param>
        /// <param name="nonReadable">If true, resulting texture is not readable (uses less memory)</param>
        /// <param name="editor">Callback that modifies the UnityWebRequest before it's sent</param>
        public CustomHeaderTextureDownload(Uri url, bool nonReadable, Action<UnityWebRequest> editor) {
            m_Request = CreateRequest(url,nonReadable);
            editor(m_Request);
            m_AsyncOperation = m_Request.SendWebRequest();
        }
    }
#endif
}
