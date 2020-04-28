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

namespace GLTFast.Loading {
    public class DefaultDownloadProvider : IDownloadProvider {
        public IDownload Request(string url) {
            return new AwaitableDownload(url);
        }

        public ITextureDownload RequestTexture(string url) {
            return new AwaitableTextureDownload(url);
        }
    }

    public class AwaitableDownload : IDownload {
        protected UnityWebRequest request;
        protected UnityWebRequestAsyncOperation asynOperation;


        public AwaitableDownload() {}

        public AwaitableDownload(string url) {
            Init(url);
        }

        protected virtual void Init(string url) {
            request = UnityWebRequest.Get(url);
            asynOperation = request.SendWebRequest();
        }

        public object Current { get { return asynOperation; } }
        public bool MoveNext() { return !asynOperation.isDone; }
        public void Reset() {}

        public bool success {
            get {
                return request.isDone && !request.isNetworkError && !request.isHttpError;
            }
        }

        public string error { get { return request.error; } }
        public byte[] data { get { return request.downloadHandler.data; } }
        public string text { get { return request.downloadHandler.text; } }
    }

    public class AwaitableTextureDownload : AwaitableDownload, ITextureDownload {

        public AwaitableTextureDownload():base() {}
        public AwaitableTextureDownload(string url):base(url) {}

        protected static UnityWebRequest CreateRequest(string url) {
            return UnityWebRequestTexture.GetTexture(url
                /// TODO: Loading non-readable here would save memory, but
                /// breaks texture instantiation in case of multiple samplers:
                // ,true // nonReadable
                );
        }

        protected override void Init(string url) {
            request = CreateRequest(url);
            asynOperation = request.SendWebRequest();
        }

        public Texture2D texture {
            get {
                return (request.downloadHandler as  DownloadHandlerTexture ).texture;
            }
        }
    }
}
