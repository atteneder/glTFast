// Copyright 2020-2021 Andreas Atteneder
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

namespace GLTFast.Loading {
    public class DefaultDownloadProvider : IDownloadProvider {
        public async  Task<IDownload> Request(Uri url) {
            var req = new AwaitableDownload(url);
            while (req.MoveNext()) {
                await Task.Yield();
            }
            return req;
        }

        public async Task<ITextureDownload> RequestTexture(Uri url,bool nonReadable) {
            var req = new AwaitableTextureDownload(url,nonReadable);
            while (req.MoveNext()) {
                await Task.Yield();
            }
            return req;
        }
    }

    public class AwaitableDownload : IDownload {
        const string GLB_MIME = "model/gltf-binary";
        const string GLTF_MIME = "model/gltf+json";

        protected UnityWebRequest request;
        protected UnityWebRequestAsyncOperation asynOperation;


        public AwaitableDownload() {}

        public AwaitableDownload(Uri url) {
            Init(url);
        }

        protected void Init(Uri url) {
            request = UnityWebRequest.Get(url);
            asynOperation = request.SendWebRequest();
        }

        public object Current { get { return asynOperation; } }
        public bool MoveNext() { return !asynOperation.isDone; }
        public void Reset() {}

#if UNITY_2020_1_OR_NEWER
        public bool success => request.isDone && request.result == UnityWebRequest.Result.Success;
#else
        public bool success => request.isDone && !request.isNetworkError && !request.isHttpError;
#endif

        public string error { get { return request.error; } }
        public byte[] data { get { return request.downloadHandler.data; } }
        public string text { get { return request.downloadHandler.text; } }
        public bool? isBinary
        {
            get
            {
                if (success)
                {
                    string contentType = request.GetResponseHeader("Content-Type");
                    if (contentType == GLB_MIME)
                        return true;
                    if (contentType == GLTF_MIME)
                        return false;
                }
                return null;
            }
        }
    }

    public class AwaitableTextureDownload : AwaitableDownload, ITextureDownload {

        public AwaitableTextureDownload():base() {}
        public AwaitableTextureDownload(Uri url):base(url) {}

        public AwaitableTextureDownload(Uri url, bool nonReadable) {
            Init(url,nonReadable);
        }

        protected static UnityWebRequest CreateRequest(Uri url, bool nonReadable) {
            return UnityWebRequestTexture.GetTexture(url,nonReadable);
        }

        protected void Init(Uri url, bool nonReadable) {
            request = CreateRequest(url,nonReadable);
            asynOperation = request.SendWebRequest();
        }

        public Texture2D texture {
            get {
                return (request.downloadHandler as  DownloadHandlerTexture ).texture;
            }
        }
    }
}
