// Copyright 2020 Andreas Atteneder
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

namespace GLTFast.Loading {

    [Serializable]
    public struct HttpHeader
    {
        public string Key;
        public string Value;
    }

    public delegate void EditUnityWebRequest( UnityWebRequest request );

    public class CustomHeaderDownloadProvider : IDownloadProvider {

        HttpHeader[] _headers;

        public CustomHeaderDownloadProvider( HttpHeader[] headers ) {
            _headers = headers;
        }

        public IDownload Request(string url) {
            return new CustomHeaderDownload(url,RegisterHttpHeaders);
        }

        public ITextureDownload RequestTexture(string url) {
            return new CustomHeaderTextureDownload(url,RegisterHttpHeaders);
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
        public CustomHeaderDownload(string url, EditUnityWebRequest editor) : base() {
            request = UnityWebRequest.Get(url);
            editor(request);
            asynOperation = request.SendWebRequest();
        }
    }

    public class CustomHeaderTextureDownload : AwaitableTextureDownload {

        public CustomHeaderTextureDownload(string url, EditUnityWebRequest editor) : base() {
            request = CreateRequest(url);
            editor(request);
            asynOperation = request.SendWebRequest();
        }
    }
}
