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

namespace GLTFast {
    public class DefaultDownloadProvider : IDownloadProvider {
        public IDownload Request(string url) {
            return new AwaitableDownload(url);
        }

        public ITextureDownload RequestTexture(string url) {
            return new AwaitableTextureDownload(url);
        }
    }

    public class AwaitableDownload : IDownload {
        protected UnityWebRequest www;
        protected UnityWebRequestAsyncOperation asynOperation;


        public AwaitableDownload(string url) {
            Init(url);
        }

        protected virtual void Init(string url) {
            www = UnityWebRequest.Get(url);
            asynOperation = www.SendWebRequest();
        }

        public object Current { get { return asynOperation; } }
        public bool MoveNext() { return !asynOperation.isDone; }
        public void Reset() {}

        public bool success {
            get {
                return www.isDone && !www.isNetworkError && !www.isHttpError;
            }
        }

        public string error { get { return www.error; } }
        public byte[] data { get { return www.downloadHandler.data; } }
        public string text { get { return www.downloadHandler.text; } }
    }

    public class AwaitableTextureDownload : AwaitableDownload, ITextureDownload {

        public AwaitableTextureDownload(string url):base(url) {}

        protected override void Init(string url) {
            www = UnityWebRequestTexture.GetTexture(url
                /// TODO: Loading non-readable here would save memory, but
                /// breaks texture instantiation in case of multiple samplers:
                // ,true // nonReadable
                );
            asynOperation = www.SendWebRequest();
        }

        public Texture2D texture {
            get {
                return (www.downloadHandler as  DownloadHandlerTexture ).texture;
            }
        }
    }
}
