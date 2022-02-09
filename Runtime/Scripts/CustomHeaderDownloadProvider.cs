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

using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;

namespace GLTFast.Loading {

    /// <summary>
    /// Represents an HTTP request header key-value pair
    /// </summary>
    [Serializable]
    public struct HttpHeader
    {
        public string Key;
        public string Value;
    }

    public delegate void EditUnityWebRequest( UnityWebRequest request );

    /// <summary>
    /// DownloadProvider that sends HTTP request with custom header entries
    /// </summary>
    public class CustomHeaderDownloadProvider : IDownloadProvider {

        HttpHeader[] _headers;

        /// <summary>
        /// Creates a new CustomHeaderDownloadProvider with a given set of HTTP request header entries
        /// </summary>
        /// <param name="headers">HTTP request header entries to send</param>
        public CustomHeaderDownloadProvider( HttpHeader[] headers ) {
            _headers = headers;
        }

        public async Task<IDownload> Request(Uri url) {
            var req = new CustomHeaderDownload(url,RegisterHttpHeaders);
            while (req.MoveNext()) {
                await Task.Yield();
            }
            return req;
        }

        public async Task<ITextureDownload> RequestTexture(Uri url,bool nonReadable) {
            var req = new CustomHeaderTextureDownload(url,nonReadable,RegisterHttpHeaders);
            while (req.MoveNext()) {
                await Task.Yield();
            }
            return req;
        }

        void RegisterHttpHeaders(UnityWebRequest request)
        {
            if(_headers!=null) {
                foreach(var header in _headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
        }
    }

    public class CustomHeaderDownload : AwaitableDownload {
        public CustomHeaderDownload(Uri url, EditUnityWebRequest editor) : base() {
            request = UnityWebRequest.Get(url);
            editor(request);
            asynOperation = request.SendWebRequest();
        }
    }

    public class CustomHeaderTextureDownload : AwaitableTextureDownload {

        public CustomHeaderTextureDownload(Uri url, bool nonReadable, EditUnityWebRequest editor) : base() {
            request = CreateRequest(url,nonReadable);
            editor(request);
            asynOperation = request.SendWebRequest();
        }
    }
}
